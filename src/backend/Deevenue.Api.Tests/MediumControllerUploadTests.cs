using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class MediumControllerUploadTests
{
    private HttpResponseMessage response = null!;

    [Fact]
    public async Task Upload_CanSucceed()
    {
        await WhenUploadingAsync("placeholder.jpg");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        // TODO: Check for Notification and associated header
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

        var client = ApiFixture.Instance.CreateClient();
        response = await client.PostAsync("/medium", new MultipartFormDataContent
        {
            { new StreamContent(resourceStream), "file", fileName }
        });
    }
}
