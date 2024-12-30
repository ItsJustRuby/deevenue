using System.Net;
using System.Net.Http.Json;
using Deevenue.Api.Framework;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class SessionControllerTests
{
    [Fact]
    public async Task SessionGet_ShouldReturnIsSfwTrueByDefault()
    {
        var client = ApiFixture.Instance.CreateClient();
        var result = await client.GetAsync("/session", TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var viewModel = await result.JsonAsync<SessionViewModel>();
        viewModel.IsSfw.Should().BeTrue();
    }

    // TODO: Build a NSFW/SFW (auto)fixture
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task SessionUpdate_Succeeds(bool isSfw)
    {
        var client = ApiFixture.Instance.CreateClient();
        var content = JsonContent.Create(new Controllers.SessionController.SessionUpdateParameters
        {
            IsSfw = isSfw
        });
        var result = await client.PatchAsync("/session", content, TestContext.Current.CancellationToken);
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}

public class JobControllerTests { }
public class RuleControllerTests { }
public class TagControllerTests { }
