using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class ThumbnailControllerTests
{
    [Fact]
    public async Task GetThumbnail_RefusesInvalidSizes()
    {
        var mediumId = Given.FileUpload.Of("square.png");
        await When.UsingApiClient(c =>
            c.GetAsync($"/thumbnail/{mediumId}_xl.jpg", TestContext.Current.CancellationToken));
        Then.Response.Value.StatusCode.Should().NotBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetThumbnail_Can404()
    {
        await When.UsingApiClient(c =>
            c.GetAsync($"/thumbnail/{Guid.NewGuid()}_s.jpg", TestContext.Current.CancellationToken));
        Then.Response.Value.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("s")]
    [InlineData("l")]
    public async Task GetThumbnail_AcceptsValidSizes(string abbreviation)
    {
        var mediumId = Given.FileUpload.Of("square.png");
        await When.UsingApiClient(c =>
            c.GetAsync($"/thumbnail/{mediumId}_{abbreviation}.jpg", TestContext.Current.CancellationToken));
        Then.Response.Value.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
