using System.Net;
using Deevenue.Api.Framework;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class SessionControllerTests
{
    [Fact]
    public async Task SessionGet_ShouldReturnIsSfwTrueByDefault()
    {
        await When.UsingApiClient(c => c.GetAsync("/session", TestContext.Current.CancellationToken));
        var result = Then.Response.Value;
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.OK);

        var viewModel = Then.Response.AsViewModel<SessionViewModel>();
        viewModel.IsSfw.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SessionUpdate_Succeeds(bool isSfw)
    {
        Given.Session.HasSfwOf(isSfw);
    }
}

public class JobControllerTests { }
public class RuleControllerTests { }
public class TagControllerTests { }
