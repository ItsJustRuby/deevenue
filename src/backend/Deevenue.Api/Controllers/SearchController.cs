using Deevenue.Domain;
using Deevenue.Domain.Search;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Deevenue.Api.Controllers;

public class SearchController(ISearchService search) : DeevenueApiControllerBase
{
    private readonly ISearchService search = search;

    [HttpGet(Name = "search")]
    [ProducesResponseType(200)]
    [ProducesResponseType(400)]
    public async Task<ActionResult<PaginationViewModel<SearchResultViewModel>>> Search(SearchParams searchParams)
    {
        BeLenientWith(searchParams);
        await new SearchParamsValidator().ValidateAndThrowAsync(searchParams);
        return await search.RunAsync(SearchParameterAdapter.Wrap(searchParams));
    }

    private static void BeLenientWith(SearchParams p)
    {
        p.PageSize = Math.Clamp(p.PageSize, 10, 100);
        p.PageNumber = Math.Max(1, p.PageNumber);
    }

    public class SearchParamsValidator : AbstractValidator<SearchParams>
    {
        public SearchParamsValidator()
        {
            RuleFor(p => p.Query).NotEmpty();
        }
    }

    public class SearchParams : PaginationQueryParameters
    {
        [FromQuery(Name = "q")]
        [BindRequired]
        public required string Query { get; set; }
    }

    private class SearchParameterAdapter : ISearchParameters
    {
        public PaginationParameters Pagination { get; init; }

        public string Query { get; init; }

        public static ISearchParameters Wrap(SearchParams p)
            => new SearchParameterAdapter(p.Query, new PaginationParameters(p.PageNumber, p.PageSize));

        private SearchParameterAdapter(string query, PaginationParameters pagination)
        {
            Query = query;
            Pagination = pagination;
        }
    }
}
