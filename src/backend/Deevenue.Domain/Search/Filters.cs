namespace Deevenue.Domain.Search;

internal static class Filters
{
    public static IFilter? TryParse(string term)
    {
        if (term.Length == 0) return null;

        var isInverted = false;

        if (term[0] == '-')
        {
            isInverted = true;
            term = term[1..];
        }

        foreach (var factory in FilterFactories.Factories)
        {
            var potentialMatch = factory.Regex.Match(term);

            if (potentialMatch.Success)
            {
                var result = factory.Create(potentialMatch.Groups);
                if (isInverted)
                    return new NegatingFilter(result);
                return result;
            }
        }

        return null;
    }

    private class NegatingFilter(IFilter inner) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext ctx) => !inner.Rejects(document, ctx);
    }
}
