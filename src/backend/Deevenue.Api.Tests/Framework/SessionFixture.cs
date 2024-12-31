using System.Net;
using System.Net.Http.Json;
using FluentAssertions;

namespace Deevenue.Api.Tests.Framework;

internal class SessionFixture
{
    public void IsSfw() => HasSfwOf(true);
    public void IsNsfw() => HasSfwOf(false);

    public void HasSfwOf(bool isSfw)
    {
        var content = JsonContent.Create(new Controllers.SessionController.SessionUpdateParameters
        {
            IsSfw = isSfw
        });

        When.UsingApiClient(c =>
        {
            var resultTask = c.PatchAsync("/session", content, TestContext.Current.CancellationToken);
            resultTask.Wait();
            return resultTask.Result;
        });

        Then.Response.Value.Should().NotBeNull();
        Then.Response.Value.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
