using System.Net;
using FluentAssertions;

namespace Deevenue.Api.Tests;

public class MediumControllerGetTests
{
    private HttpResponseMessage response = null!;

    [Fact]
    public async Task Get_Returns404_IfMediumDoesNotExist()
    {
        await WhenGettingAsync(Guid.NewGuid());
        response.Should().HaveStatusCode(HttpStatusCode.NotFound);
    }

    private async Task WhenGettingAsync(Guid id)
    {
        var client = ApiFixture.Instance.CreateClient();
        response = await client.GetAsync($"/medium/{id}", TestContext.Current.CancellationToken);
    }
}
