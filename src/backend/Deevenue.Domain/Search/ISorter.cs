namespace Deevenue.Domain.Search;

internal interface ISorter
{
    IReadOnlyList<SmallMediumDocument> Sort(IReadOnlySet<SmallMediumDocument> documents);
}
