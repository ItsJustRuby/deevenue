namespace Deevenue.Domain.Search;

public interface ISearchService
{
    Task<PaginationViewModel<SearchResultViewModel>> RunAsync(ISearchParameters searchParameters);
}

public interface ISearchParameters
{
    public string Query { get; }
    public PaginationParameters Pagination { get; }
}
