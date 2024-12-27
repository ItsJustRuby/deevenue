using Deevenue.Domain.Media;
using Deevenue.Domain.Rules;
using Microsoft.Extensions.Logging;

namespace Deevenue.Domain.Search;

internal class SearchService(
    IMediumService mediumService,
    IRulesService rulesService,
    ILogger<SearchService> logger) : ISearchService
{
    private readonly ILogger<SearchService> logger = logger;

    public async Task<PaginationViewModel<SearchResultViewModel>> RunAsync(ISearchParameters searchParameters)
    {
        var searchTerms = SearchTerms.From(searchParameters.Query);

        logger.LogDebug(
            """Parsed queryString "{queryString}" into "{searchTerms}".""",
            searchParameters.Query,
            searchTerms);

        if (searchTerms.AreEmpty)
            return PaginationViewModel<SearchResultViewModel>.Empty;

        return await RunAsync(searchTerms, searchParameters.Pagination);
    }

    private async Task<PaginationViewModel<SearchResultViewModel>> RunAsync(
        SearchTerms searchTerms, PaginationParameters pagination)
    {
        var documents = await mediumService.GetAllSearchableAsync();
        var filterContext = new FilterContext(rulesService);

        var results = new HashSet<SmallMediumDocument>(documents);
        foreach (var filter in searchTerms.Filters)
            results.RemoveWhere(m => filter.Rejects(m, filterContext));

        var sorter = searchTerms.MaybeSorter ?? Sorters.TryParse("order:age_desc")!;
        var sortedResults = sorter.Sort(results);

        var items = sortedResults
            .Paginate(pagination)
            .Select(d => new SearchResultViewModel(d.Id, MediaKinds.Parse(d.ContentType))).ToList();

        var pageCount = (int)Math.Ceiling((float)sortedResults.Count / pagination.PageSize);

        return new PaginationViewModel<SearchResultViewModel>(
            items, pageCount, pagination.PageNumber, pagination.PageSize);
    }

    private record FilterContext(IRulesService RulesService) : IFilterContext { }
}
