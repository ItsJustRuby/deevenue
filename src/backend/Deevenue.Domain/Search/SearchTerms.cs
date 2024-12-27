namespace Deevenue.Domain.Search;

internal class SearchTerms(IReadOnlySet<IFilter> filters, ISorter? maybeSorter)
{
    public readonly IReadOnlySet<IFilter> Filters = filters;
    public readonly ISorter? MaybeSorter = maybeSorter;

    public static SearchTerms From(string queryString)
    {
        var searchTermList = queryString.Split(" ");

        ISorter? maybeSortingTerm = null;
        var filteringTerms = new HashSet<IFilter>();

        foreach (var term in searchTermList)
        {
            if (maybeSortingTerm == null)
            {
                var maybeCurrentSortingTerm = Sorters.TryParse(term);
                if (maybeCurrentSortingTerm != null)
                {
                    maybeSortingTerm = maybeCurrentSortingTerm;
                    continue;
                }
            }

            var maybeFilteringSearchTerm = Search.Filters.TryParse(term);
            if (maybeFilteringSearchTerm != null)
                filteringTerms.Add(maybeFilteringSearchTerm);
        }

        return new SearchTerms(filteringTerms, maybeSortingTerm);
    }

    public bool AreEmpty => Filters.Any() && MaybeSorter != null;

    public override string? ToString()
    {
        return $"Sorter: {MaybeSorter}, #Filters: {string.Join(" ", Filters.Select(f => f.ToString()))}";
    }
}
