using System.Net;
using Deevenue.Domain;
using Deevenue.Domain.Search;
using FluentAssertions;
using Microsoft.AspNetCore.WebUtilities;
using static Deevenue.Api.Controllers.SearchController;

namespace Deevenue.Api.Tests;

public class SearchControllerTests
{
    [Fact]
    public async Task ReturnsOk_OnDefaultPaginationParameters()
    {
        await WhenSearchingForAsync(new SearchParams
        {
            PageNumber = 1,
            PageSize = 10,
            Query = "foo"
        });

        Then.Response.Value.StatusCode.Should().Be(HttpStatusCode.OK);
        var viewModel = Then.Response.AsViewModel<PaginationViewModel<SearchResultViewModel>>();
        viewModel.PageNumber.Should().Be(1);
        viewModel.PageSize.Should().Be(10);
    }

    [Theory]
    [InlineData(300, -1)]
    [InlineData(4, 999)]
    [InlineData(0, 0)]
    [InlineData(-300, -3)]
    public async Task StillReturnsOk_OnMessyPaginationParameters(int pageSize, int pageNumber)
    {
        await WhenSearchingForAsync(new SearchParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Query = "foo"
        });

        Then.Response.Value.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private async Task WhenSearchingForAsync(SearchParams searchParams)
    {
        var dict = new Dictionary<string, string?>()
        {
            ["pageNumber"] = searchParams.PageNumber.ToString(),
            ["pageSize"] = searchParams.PageSize.ToString(),
            ["q"] = searchParams.Query.ToString(),
        };

        var path = QueryHelpers.AddQueryString("/search", dict);
        await When.UsingApiClient(c => c.GetAsync(path, TestContext.Current.CancellationToken));
    }
}
