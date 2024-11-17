
namespace Deevenue.Domain.Search;

internal static class Sorters
{
    public static ISorter? TryParse(string term)
    {
        foreach (var factory in SorterFactories.Factories)
        {
            var potentialMatch = factory.Regex.Match(term);

            if (potentialMatch.Success)
                return SorterFactories.Create(factory, potentialMatch.Groups);
        }

        return null;
    }
}
