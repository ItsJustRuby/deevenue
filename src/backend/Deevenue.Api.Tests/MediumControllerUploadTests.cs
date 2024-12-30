using System.Net;
using System.Net.Http.Headers;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class MediumControllerUploadTests(ITestOutputHelper output)
{
    private HttpClient client = null!;
    private HttpResponseMessage response = null!;

    [Theory]
    [InlineData("1234 - foo bar.jpg")]
    [InlineData("square.png")]
    [InlineData("tall.png")]
    [InlineData("tiny_video.mp4")]
    [InlineData("wide.png")]
    public async Task Upload_CanSucceed(string fileName)
    {
        await WhenUploadingAsync(fileName);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        response.Headers.Should().ContainKey("X-Deevenue-Schema");
        var headerValues = response.Headers.GetValues("X-Deevenue-Schema");
        headerValues.Should().HaveCount(1);
        headerValues.Single().Should().Be("Notification");

        var notification = await response.JsonAsync<NotificationViewModel>();
        notification.Contents.Should().Contain(c => c is Entity);
        var contentPart = (Entity)notification.Contents.Single(c => c is Entity);
        contentPart.EntityKind.Should().Be(EntityKind.Medium);

        var mediumId = contentPart.Id;
        await ThenMediumFileExists(mediumId);
    }

    private async Task ThenMediumFileExists(Guid mediumId)
    {
        var response = await client.GetAsync($"/file/{mediumId}", TestContext.Current.CancellationToken);
        response.Should().HaveStatusCode(HttpStatusCode.OK);
    }

    // TODO: Turn this into a cool [WithFile("placeholder.jpg")] fixture
    // that gets you its Guid as parameter. Maybe a Factory as Assembly fixture?
    // Just ensure that that factory keeps track of what is already uploaded
    // (or make it use the hash-based duplicate check).
    private async Task WhenUploadingAsync(string fileName)
    {
        var assembly = typeof(MediumControllerUploadTests).Assembly;
        var availableResourceNames = assembly.GetManifestResourceNames();
        var matchingResourceName = availableResourceNames.Single(n => n.EndsWith(fileName));
        using var resourceStream = assembly.GetManifestResourceStream(matchingResourceName)!;

        client = ApiFixture.Instance.CreateClient();

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

        response = await client.PostAsync("/medium", formContent, TestContext.Current.CancellationToken);
    }
}
