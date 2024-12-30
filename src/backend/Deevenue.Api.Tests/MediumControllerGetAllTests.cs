using System.Net;
using Deevenue.Api.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;

namespace Deevenue.Api.Tests;

public class MediumControllerGetAllTests
{
    private HttpResponseMessage response = null!;

    [Fact]
    public async Task GetAll_ReturnsOk_OnDefaultEmptyQuery()
    {
        await WhenGettingAllMediaAsync(new PaginationQueryParameters
        {
            PageNumber = 1,
            PageSize = 10,
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task WhenGettingAllMediaAsync(PaginationQueryParameters paginationParameters)
    {
        var client = ApiFixture.Instance.CreateClient();

        var dict = new Dictionary<string, string?>()
        {
            ["pageNumber"] = paginationParameters.PageNumber.ToString(),
            ["pageSize"] = paginationParameters.PageSize.ToString(),
        };

        var path = QueryHelpers.AddQueryString("/medium", dict);
        response = await client.GetAsync(path, TestContext.Current.CancellationToken);
    }
}
