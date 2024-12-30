using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class HealthControllerTests
{
    [Fact]
    public async Task HealthCheck_RespondsOk()
    {
        var client = ApiFixture.Instance.CreateClient();
        var result = await client.GetAsync("/health", TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
