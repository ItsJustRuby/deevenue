using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;

namespace Deevenue.Api.Tests.Framework;

internal class FileUploadFixture
{
    private readonly Dictionary<string, Guid> pastUploads = [];

    public Guid? Of(string fileName)
    {
        if (pastUploads.ContainsKey(fileName))
            return pastUploads[fileName];

        var assembly = typeof(MediumControllerUploadTests).Assembly;
        var availableResourceNames = assembly.GetManifestResourceNames();
        var matchingResourceName = availableResourceNames.Single(n => n.EndsWith(fileName));
        using var resourceStream = assembly.GetManifestResourceStream(matchingResourceName)!;

        var streamContent = new StreamContent(resourceStream);
        var contentTypesByExtension = new Dictionary<string, string>
        {
            [".jpg"] = "image/jpeg",
            [".png"] = "image/png",
            [".mp4"] = "video/mp4",
        };
        streamContent.Headers.ContentType =
            new MediaTypeHeaderValue(contentTypesByExtension[Path.GetExtension(fileName)]);

        var formContent = new MultipartFormDataContent
        {
            { streamContent, "file", fileName }
        };

        When.UsingApiClient(c =>
        {
            var task = c.PostAsync("/medium", formContent, TestContext.Current.CancellationToken);
            task.Wait();
            return task.Result;
        });

        var response = Then.Response.Value;
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.Should().ContainKey("X-Deevenue-Schema");
        var headerValues = response.Headers.GetValues("X-Deevenue-Schema");
        headerValues.Should().HaveCount(1);
        headerValues.Single().Should().Be("Notification");

        var notification = Then.Response.AsViewModel<NotificationViewModel>();
        notification.Contents.Should().Contain(c => c is Entity);
        var contentPart = (Entity)notification.Contents.Single(c => c is Entity);
        contentPart.EntityKind.Should().Be(EntityKind.Medium);

        var mediumId = contentPart.Id;
        pastUploads[fileName] = mediumId;
        return mediumId;
    }
}
