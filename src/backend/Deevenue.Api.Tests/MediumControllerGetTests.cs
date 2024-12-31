using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class MediumControllerGetTests
{
    [Fact]
    public async Task Get_Returns404_IfMediumDoesNotExist()
    {
        await When.UsingApiClient(c =>
        {
            return c.GetAsync($"/medium/{Guid.NewGuid()}", TestContext.Current.CancellationToken);
        });

        Then.Response.Value.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }
}
