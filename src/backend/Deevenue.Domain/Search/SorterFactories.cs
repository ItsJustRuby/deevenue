using System.Text.RegularExpressions;

namespace Deevenue.Domain.Search;

internal static class SorterFactories
{
    public static readonly IReadOnlyList<ISorterFactory> Factories =
    [
        DimensionSorterFactory.Instance,
        RotationSorterFactory.Instance,
        FileSizeSorterFactory.Instance,
        AgeSorterFactory.Instance,
    ];

    public static ISorter Create(ISorterFactory factory, GroupCollection groups)
    {
        return SortOrderFactory.Create(factory, groups);
    }

    internal static Regex GetSorter(string inner)
    {
        return new Regex("(sort|order):" + inner + "(_(?<direction>desc|asc))?");
    }
}

internal static class SortOrderFactory
{
    public enum SortOrder { Ascending, Descending };
    private static readonly SortOrder defaultSortOrder = SortOrder.Descending;

    internal static SortOrder Parse(GroupCollection groups)
    {
        if (!groups.ContainsKey("direction") || groups["direction"].Value == "")
            return defaultSortOrder;

        var sortOrders = new Dictionary<string, SortOrder>
            { { "asc", SortOrder.Ascending }, { "desc", SortOrder.Descending} };

        return sortOrders[groups["direction"].Value];
    }

    internal static ISorter Create(ISorterFactory factory, GroupCollection groups)
    {
        var sortOrder = Parse(groups);
        var innerTerm = factory.Create(groups);
        return new OrderingSorter(innerTerm, sortOrder);
    }

    private class OrderingSorter(ISorter innerTerm, SortOrder sortOrder) : ISorter
    {
        private readonly ISorter innerTerm = innerTerm;
        private readonly SortOrder sortOrder = sortOrder;

        public IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents)
        {
            var innerResult = innerTerm.Sort(documents);
            if (sortOrder == SortOrder.Descending)
                return innerResult.Reverse().ToList();
            return innerResult;
        }
    }
}


internal class DimensionSorterFactory : ISorterFactory
{
    public static readonly ISorterFactory Instance = new DimensionSorterFactory();

    public Regex Regex => SorterFactories.GetSorter("(?<dimension>width|height)");
    public ISorter Create(GroupCollection groups)
        => new DimensionSorter(groups["dimension"].Value);

    private record DimensionSorter(string Dimension) : ISorter
    {
        public IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents)
        {
            if (Dimension == "width")
                return [.. documents.OrderBy(d => d.Width)];
            return [.. documents.OrderBy(d => d.Height)];
        }
    }
}

internal class RotationSorterFactory : ISorterFactory
{
    public static readonly ISorterFactory Instance = new RotationSorterFactory();

    public Regex Regex => SorterFactories.GetSorter("(?<rotation>portrait|landscape)");
    public ISorter Create(GroupCollection groups)
        => new RotationSorter(groups["rotation"].Value);

    private class RotationSorter(string Rotation) : ISorter
    {
        public IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents)
        {
            if (Rotation == "portrait")
                return [.. documents.OrderBy(d => (decimal)d.Height / d.Width)];
            return [.. documents.OrderBy(d => (decimal)d.Width / d.Height)];
        }
    }
}

internal class AgeSorterFactory : ISorterFactory
{
    public static readonly ISorterFactory Instance = new AgeSorterFactory();

    public Regex Regex => SorterFactories.GetSorter("age");
    public ISorter Create(GroupCollection groups) => new AgeSorter();

    private class AgeSorter : ISorter
    {
        public IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents)
        {
            return [.. documents.OrderBy(d => d.InsertDate)];
        }
    }
}

internal class FileSizeSorterFactory : ISorterFactory
{
    public static readonly ISorterFactory Instance = new FileSizeSorterFactory();

    public Regex Regex => SorterFactories.GetSorter("filesize");
    public ISorter Create(GroupCollection groups) => new FileSizeSorter();

    private class FileSizeSorter : ISorter
    {
        public IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents)
        {
            return [.. documents.OrderBy(d => d.FilesizeInBytes)];
        }
    }
}
