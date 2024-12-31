using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class MediumControllerUploadTests
{
    [Theory]
    [InlineData("1234 - foo bar.jpg")]
    [InlineData("square.png")]
    [InlineData("tall.png")]
    [InlineData("tiny_video.mp4")]
    [InlineData("wide.png")]
    public async Task Upload_CanSucceed(string fileName)
    {
        var mediumId = Given.FileUpload.Of(fileName);
        await ThenMediumFileExists(mediumId);
    }

    private static async Task ThenMediumFileExists(Guid? mediumId)
    {
        mediumId.Should().HaveValue();
        await When.UsingApiClient(c => c.GetAsync($"/file/{mediumId}", TestContext.Current.CancellationToken));
        Then.Response.Value.Should().HaveStatusCode(HttpStatusCode.OK);
    }
}
