using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class HealthControllerTests
{
    [Fact]
    public async Task HealthCheck_RespondsOk()
    {
        await When.UsingApiClient(c => c.GetAsync("/health", TestContext.Current.CancellationToken));
        var result = Then.Response.Value;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
