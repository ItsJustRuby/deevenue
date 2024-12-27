using System.Diagnostics;
using System.Text.RegularExpressions;
using Deevenue.Domain.Rules;

namespace Deevenue.Domain.Search;

internal static class FilterFactories
{
    public static readonly IReadOnlyList<IFilterFactory> Factories =
    [
        CountingFilterFactory.Instance,
        CategoryFilterFactory.Instance,
        RatingFilterFactory.Instance,
        AgeFilterFactory.Instance,
        FilesizeFilterFactory.Instance,
        DimensionFilterFactory.Instance,
        AspectRatioFilterFactory.Instance,
        RuleFilterFactory.Instance,
        ExactFilterFactory.Instance,
        PositiveFilterFactory.Instance,
    ];
}

internal static partial class Regexes
{
    internal const string ValidTagRegexInner = "(?<category>[a-z]+:)?([a-zA-Z0-9.]+)";
    internal const string Comparison = @"(?<operator>(:|=|<|>|<=|>=|!=))";
    internal const string IntComparison = Comparison + @"(?<number>[0-9]+)";
}

internal partial class CountingFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new CountingFilterFactory();

    [GeneratedRegex(@"^tags" + Regexes.IntComparison)]
    private static partial Regex CountingTermRegex();

    public Regex Regex => CountingTermRegex();

    public IFilter Create(GroupCollection groups)
    {
        return new CountingFilter(groups["operator"].Value, groups["number"].Value);
    }

    [DebuggerDisplay("{ToString()}")]
    private class CountingFilter(string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
            => GenericComparison.Rejects(document.InnateTags.Count, op, target);

        public override string? ToString() => $"tags{op}{target}";
    }
}

internal partial class CategoryFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new CategoryFilterFactory();

    [GeneratedRegex(@"^(?<category>[a-z]+)tags" + Regexes.IntComparison)]
    private static partial Regex CategoryTermRegex();

    public Regex Regex => CategoryTermRegex();

    public IFilter Create(GroupCollection groups)
        => new CategoryFilter(groups["category"].Value, groups["operator"].Value, groups["number"].Value);

    [DebuggerDisplay("{ToString()}")]
    private class CategoryFilter(string category, string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            var actualValue = document.InnateTags.Count(t => t.StartsWith($"{category}:"));
            return GenericComparison.Rejects(actualValue, op, target);
        }

        public override string? ToString() => $"{category}tags{op}{target}";
    }
}

internal partial class RatingFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new RatingFilterFactory();

    [GeneratedRegex(@"^rating:(u|s|e|q)")]
    private static partial Regex RatingTermRegex();

    public Regex Regex => RatingTermRegex();

    public IFilter Create(GroupCollection groups) => new RatingFilter(Ratings.ToRating[groups[1].Value[0]]);

    [DebuggerDisplay("{ToString()}")]
    private class RatingFilter(Rating rating) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
            => document.Rating != rating;

        public override string? ToString() => $"rating:{rating}";
    }
}

internal partial class AgeFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new AgeFilterFactory();

    const string Age = "(?<period>w|weeks?|d|days?|m|months?|y|years?)";

    [GeneratedRegex("^age" + Regexes.IntComparison + Age)]
    private static partial Regex AgeTermRegex();
    public Regex Regex => AgeTermRegex();

    public IFilter Create(GroupCollection groups)
    {
        return new AgeFilter(groups["period"].Value, groups["operator"].Value, groups["number"].Value);
    }

    private class AgeFilter(string period, string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            var numberOfPeriods = int.Parse(target);

            var targetValue = DateOnly.FromDateTime(DateTime.UtcNow);
            var actualValue = DateOnly.FromDateTime(document.InsertDate);

            switch (period)
            {
                case "w": case "weeks": targetValue = targetValue.AddDays(-1 * 7 * numberOfPeriods); break;
                case "d": case "days": targetValue = targetValue.AddDays(-1 * numberOfPeriods); break;
                case "m": case "months": targetValue = targetValue.AddMonths(-1 * numberOfPeriods); break;
                case "y": case "years": targetValue = targetValue.AddYears(-1 * numberOfPeriods); break;
            }

            return op switch
            {
                ":" or "=" => actualValue == targetValue,
                "<" => actualValue < targetValue,
                "<=" => actualValue <= targetValue,
                ">" => actualValue > targetValue,
                ">=" => actualValue >= targetValue,
                "!=" => actualValue != targetValue,
                _ => throw new ArgumentOutOfRangeException(op),
            };
        }
    }
}

internal partial class FilesizeFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new FilesizeFilterFactory();

    const string Filesize = "(?<unit>[kKmMgG][bB]?)";

    [GeneratedRegex("^filesize" + Regexes.IntComparison + Filesize)]
    private static partial Regex FilesizeTermRegex();

    public Regex Regex => FilesizeTermRegex();

    public IFilter Create(GroupCollection groups)
        => new FilesizeFilter(groups["unit"].Value, groups["operator"].Value, groups["number"].Value);

    private class FilesizeFilter(string unit, string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            int factor = unit.EndsWith('b') ? 1000 : 1024;
            char prefix = unit.ToLowerInvariant()[0];

            decimal targetValueInBytes = decimal.Parse(target);

            int exponent = prefix switch
            {
                'k' => 1,
                'm' => 2,
                'g' => 3,
                _ => throw new ArgumentOutOfRangeException(prefix.ToString())
            };

            for (int i = 0; i < exponent; ++i)
                targetValueInBytes *= factor;

            return GenericComparison.Rejects(document.FilesizeInBytes, op, targetValueInBytes);
        }
    }
}

internal partial class DimensionFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new DimensionFilterFactory();

    [GeneratedRegex("^(?<dimension>width|height)" + Regexes.IntComparison)]
    private static partial Regex DimensionTermRegex();

    public Regex Regex => DimensionTermRegex();

    public IFilter Create(GroupCollection groups)
    {
        return new DimensionFilter(groups["dimension"].Value, groups["operator"].Value, groups["number"].Value);
    }

    [DebuggerDisplay("{ToString()}")]
    private class DimensionFilter(string dimension, string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            var actualValue = (dimension == "width") ? document.Width : document.Height;
            return GenericComparison.Rejects(actualValue, op, target);
        }

        public override string? ToString() => $"{dimension}{op}{target}";
    }
}

internal partial class AspectRatioFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new AspectRatioFilterFactory();

    const string DecimalComparison = Regexes.Comparison + @"(?<number>[0-9]+(\.[0-9]+)?)";

    [GeneratedRegex("^aspectratio" + DecimalComparison)]
    private static partial Regex AspectRatioTermRegex();

    public Regex Regex => AspectRatioTermRegex();

    public IFilter Create(GroupCollection groups)
    {
        return new AspectRatioFilter(groups["operator"].Value, groups["number"].Value);
    }

    [DebuggerDisplay("{ToString()}")]
    private class AspectRatioFilter(string op, string target) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            var actual = (decimal)document.Width / document.Height;
            return GenericComparison.Rejects(actual, op, target);
        }

        public override string? ToString() => $"aspectratio{op}{target}";
    }
}

internal partial class RuleFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new RuleFilterFactory();

    [GeneratedRegex(@"^rule:(?<number>[0-9]+)")]
    private static partial Regex RuleTermRegex();

    public Regex Regex => RuleTermRegex();

    public IFilter Create(GroupCollection groups)
    {
        return new RuleFilter(groups["number"].Value);
    }

    [DebuggerDisplay("{ToString()}")]
    private class RuleFilter(string ruleIndex) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
        {
            var maybeRule = context.RulesService.Get(int.Parse(ruleIndex));

            if (maybeRule == null)
                return true;

            return !RuleViolations.ExistBetween(maybeRule, document);
        }

        public override string? ToString() => $"rule:{ruleIndex}";
    }
}

internal partial class ExactFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new ExactFilterFactory();

    [GeneratedRegex(@"^\+(" + Regexes.ValidTagRegexInner + @")")]
    private static partial Regex ExactTermRegex();

    public Regex Regex => ExactTermRegex();

    public IFilter Create(GroupCollection groups) => new ExactFilter(groups[1].Value);

    [DebuggerDisplay("{ToString()}")]
    private class ExactFilter(string term) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
            => !document.InnateTags.Contains(term);

        public override string? ToString() => $"+{term}";
    }
}

internal partial class PositiveFilterFactory : IFilterFactory
{
    public static readonly IFilterFactory Instance = new PositiveFilterFactory();

    [GeneratedRegex(Regexes.ValidTagRegexInner)]
    private static partial Regex PositiveTermRegex();

    public Regex Regex => PositiveTermRegex();

    public IFilter Create(GroupCollection groups) => new PositiveFilter(groups[0].Value);

    [DebuggerDisplay("{ToString()}")]
    private class PositiveFilter(string term) : IFilter
    {
        public bool Rejects(SmallMediumDocument document, IFilterContext context)
            => !document.SearchableTags.Contains(term);

        public override string? ToString() => term;
    }
}

internal static class GenericComparison
{
    public static bool Rejects(decimal actualValue, string op, string target)
        => !Allows(actualValue, op, decimal.Parse(target));

    public static bool Rejects(decimal actualValue, string op, decimal targetValue)
        => !Allows(actualValue, op, targetValue);

    private static bool Allows(decimal actualValue, string op, decimal targetValue)
    {
        return op switch
        {
            ":" or "=" => actualValue == targetValue,
            "<" => actualValue < targetValue,
            "<=" => actualValue <= targetValue,
            ">" => actualValue > targetValue,
            ">=" => actualValue >= targetValue,
            "!=" => actualValue != targetValue,
            _ => throw new ArgumentOutOfRangeException(op),
        };
    }
}
