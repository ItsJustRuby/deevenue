using System.Diagnostics;

namespace Deevenue.Domain;

[DebuggerDisplay("{ToString()}")]
public sealed record PaginationViewModel<TItem>(
    IReadOnlyList<TItem> Items,
    int PageCount,
    int PageNumber,
    int PageSize
)
{
    private static readonly PaginationViewModel<TItem> empty = new([], 0, 1, 10);

    public static PaginationViewModel<TItem> Empty => empty;

    public override string? ToString()
        => $"Page {PageNumber}/{PageCount}, Length {Items.Count}, PageSize {PageSize}";
}
