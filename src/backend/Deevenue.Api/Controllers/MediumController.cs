using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Search;
using Deevenue.Domain.Thumbnails;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class MediumController(
    IAddMediumService addMediumService,
    IMediumService mediumService,
    IThumbnailSheetService thumbnailSheetService,
    IMediumTagService mediumTagService) : DeevenueApiControllerBase
{
    [HttpGet("", Name = "getAllMedia")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<PaginationViewModel<SearchResultViewModel>> Get(PaginationQueryParameters parameters)
    {
        return await mediumService.PaginateAllAsync(
            new PaginationParameters(parameters.PageNumber, parameters.PageSize)
        );
    }

    [HttpGet("{id:Guid}", Name = "getMedium")]
    [ProducesResponseType(200, Type = typeof(MediumViewModel))]
    [ProducesResponseType(404, Type = typeof(NotificationViewModel))]
    [ProducesResponseType(403, Type = typeof(NotificationViewModel))]
    public async Task<ActionResult> Get(Guid id)
    {
        var tryGetResult = await mediumService.TryGetAsync(id);
        return tryGetResult.Accept(new TryGetResultVisitor(this));
    }

    [HttpGet("withHash/md5/{hash}", Name = "findMediumByMD5Hash")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> FindByHash(string hash)
    {
        var existingGuid = await mediumService.TryGetByHashAsync(hash);
        if (existingGuid != null)
            return Ok();
        return NotFound();
    }

    [HttpPost(Name = "uploadMedium")]
    [DisableRequestSizeLimit]
    [ProducesResponseType(200, Type = typeof(NotificationViewModel))]
    [ProducesResponseType(400, Type = typeof(NotificationViewModel))]
    [ProducesResponseType(409, Type = typeof(NotificationViewModel))]
    public async Task<NotificationViewModel> Upload(IFormFile file)
    {
        using var readStream = file.OpenReadStream();

        var tryAddResult = await addMediumService.TryAddAsync(
            fileName: file.FileName,
            contentType: file.ContentType,
            readStream: readStream,
            size: file.Length
        );

        return tryAddResult.Accept(new TryAddResultVisitor(this));
    }

    [HttpPut("{id:Guid}/rating/{rating:Rating}", Name = "setMediumRating")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404, Type = typeof(NotificationViewModel))]
    [ProducesResponseType(403, Type = typeof(MediumViewModel))]
    [ProducesResponseType(400)]
    public async Task<ActionResult<MediumViewModel>> SetRating(Guid id, Rating rating)
    {
        var success = await mediumService.SetRatingAsync(id, rating);
        if (!success)
            return NotFoundNotification();

        return await Get(id);
    }

    [HttpPut("{id:Guid}/tags/{tagName}", Name = "addTagToMedium")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<MediumViewModel>> AddTag(Guid id, string tagName)
    {
        await mediumTagService.LinkAsync(id, tagName);
        return await Get(id);
    }

    [HttpDelete("{id:Guid}/tags/{tagName}", Name = "removeTagFromMedium")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<MediumViewModel>> RemoveTag(Guid id, string tagName)
    {
        await mediumTagService.UnlinkAsync(id, tagName);
        return await Get(id);
    }

    [HttpPut("{id:Guid}/absentTags/{tagName}", Name = "addAbsentTagToMedium")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<MediumViewModel>> AddAbsentTag(Guid id, string tagName)
    {
        await mediumTagService.LinkAbsentAsync(id, tagName);
        return await Get(id);
    }

    [HttpDelete("{id:Guid}/absentTags/{tagName}", Name = "removeAbsentTagFromMedium")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    public async Task<ActionResult<MediumViewModel>> RemoveAbsentTag(Guid id, string tagName)
    {
        await mediumTagService.UnlinkAbsentAsync(id, tagName);
        return await Get(id);
    }

    [HttpPost("{id:Guid}/thumbnailSheets/", Name = "requestThumbnailSheet")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<ThumbnailSheetJobViewModel>> StartThumbnailSheetJob(
        Guid id, [FromBody] StartThumbnailSheetJobParameters bodyParameters)
    {
        await new StartThumbnailSheetJobParametersValidator().ValidateAndThrowAsync(bodyParameters);

        var sheetId = await thumbnailSheetService.ScheduleJobAsync(id, bodyParameters.ThumbnailCount);
        return new ThumbnailSheetJobViewModel(sheetId);
    }

    private class StartThumbnailSheetJobParametersValidator : AbstractValidator<StartThumbnailSheetJobParameters>
    {
        public StartThumbnailSheetJobParametersValidator()
        {
            RuleFor(p => p.ThumbnailCount).NotEmpty().InclusiveBetween(1, 100);
        }
    }

    public record StartThumbnailSheetJobParameters(int ThumbnailCount);
    public record ThumbnailSheetJobViewModel(Guid Id);

    [HttpDelete("{id:Guid}", Name = "deleteMedium")]
    [ProducesResponseType(200, Type = typeof(NotificationViewModel))]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Delete(Guid id)
    {
        await mediumService.DeleteAsync(id);
        return Ok(
            Notifications.CreateBuilder(NotificationLevel.Info)
            .WithText("Successfully deleted medium").Build()
        );
    }

    private NotFoundObjectResult NotFoundNotification()
    {
        var notification = Notifications.CreateBuilder(NotificationLevel.Error)
            .WithText("Medium not found")
            .Build();
        return NotFound(notification);
    }

    private class TryGetResultVisitor(MediumController controller)
        : ITryGetResultVisitor<ActionResult>
    {
        public ActionResult VisitNotFound() => controller.NotFoundNotification();

        public ActionResult VisitNotSfw()
        {
            var notification = Notifications.CreateBuilder(NotificationLevel.Error)
                .WithText("That medium is not SFW.")
                .Build();
            controller.Response.StatusCode = 403;
            return new ObjectResult(notification);
        }

        public ActionResult VisitSuccess(MediumViewModel medium) => controller.Ok(medium);
    }

    private class TryAddResultVisitor(MediumController controller)
        : ITryAddResultVisitor<NotificationViewModel>
    {
        public NotificationViewModel VisitConflictingMedium(Guid conflictingMediumId)
        {
            controller.Response.StatusCode = 409;
            return Notifications.CreateBuilder(NotificationLevel.Warning)
                .WithText("Nope, that file already exists")
                .WithEntity(EntityKind.Medium, conflictingMediumId)
                .Build();
        }

        public NotificationViewModel VisitSuccess(Guid createdMediumId)
        {
            controller.Response.StatusCode = 200;
            return Notifications.CreateBuilder(NotificationLevel.Info)
                .WithText("Successfully uploaded medium ")
                .WithLink($"/show/{createdMediumId}", $"with ID {createdMediumId}")
                .WithEntity(EntityKind.Medium, createdMediumId)
                .Build();
        }

        public NotificationViewModel VisitUnusableMediaKind(string contentType)
        {
            return Notifications.CreateBuilder(NotificationLevel.Error)
                .WithText($"Unknown Content type \"{contentType}\".")
                .Build();
        }
    }
}
