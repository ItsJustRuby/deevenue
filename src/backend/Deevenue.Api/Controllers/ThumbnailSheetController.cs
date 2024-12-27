using Deevenue.Domain.Thumbnails;
using Microsoft.AspNetCore.Mvc;

namespace Deevenue.Api.Controllers;

public class ThumbnailSheetController(
    IThumbnailSheetService service,
    IThumbnailSheetRepository repository) : DeevenueApiControllerBase
{
    [HttpGet("{id:Guid}", Name = "getThumbnailSheet")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<ThumbnailSheetViewModel>> Get(Guid id)
    {
        var maybeSheet = await repository.GetAsync(id);
        if (maybeSheet == null)
            return NotFound();
        return maybeSheet;
    }

    [HttpDelete("{id:Guid}", Name = "rejectThumbnailSheet")]
    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Reject(Guid id)
    {
        await service.RejectAsync(id);
        return Ok();
    }

    [HttpPost("{id:Guid}/selection/{thumbnailIndex:int}", Name = "selectNewThumbnail")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<ActionResult> Select(Guid id, int thumbnailIndex)
    {
        await service.SelectAsync(id, thumbnailIndex);
        return Ok();
    }
}

// This is a separate controller not annotated as [ApiController] so it does not generate OpenAPI schema.
[Route("thumbnailsheet")]
public class ThumbnailSheetFileController(IThumbnailSheetStorage storage) : DeevenueControllerBase
{
    [HttpGet("{id:Guid}/{index:int}_{size:ThumbnailSizeAbbreviation}.jpg")]
    public async Task<ActionResult> GetFile(Guid id, int index, ThumbnailSizeAbbreviation size)
    {
        var maybeFile = await storage.GetAsync(id, index, ThumbnailSizes.ByAbbreviation(size));

        if (maybeFile == null)
            return NotFound();

        return File(maybeFile.Bytes, maybeFile.ContentType);
    }
}
