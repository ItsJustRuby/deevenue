namespace Deevenue.Domain;

public sealed record PaginationParameters(int PageNumber, int PageSize)
{
    public int SkipCount => (PageNumber - 1) * PageSize;
}

public static class PaginationParametersExtensions
{
    public static IEnumerable<T> Paginate<T>(this IEnumerable<T> enumerable, PaginationParameters p)
    {
        return enumerable.Skip(p.SkipCount).Take(p.PageSize);
    }

    public static IQueryable<T> Paginate<T>(this IOrderedQueryable<T> enumerable, PaginationParameters p)
    {
        return enumerable.Skip(p.SkipCount).Take(p.PageSize);
    }
}
