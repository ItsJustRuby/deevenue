using System.Text.RegularExpressions;

namespace Deevenue.Domain.Rules;

internal static class RuleViolations
{
    public static bool ExistBetween(RuleViewModel rule, SmallMediumDocument medium)
    {
        var result = new HashSet<IThenViewModel>();

        if (rule.Iffs.Any(i => !AppliesTo(i, medium)))
            return false;

        return rule.Thens.Any(t => Violates(t, medium));
    }

    private static bool AppliesTo(IIffViewModel iff, SmallMediumDocument medium)
        => iff.Accept(new DoesIffApplyVisitor(medium));
    private static bool Violates(IThenViewModel then, SmallMediumDocument medium)
        => then.Accept(new DoesMediumViolateVisitor(medium));

    private class DoesIffApplyVisitor(SmallMediumDocument medium) : IIffViewModelVisitor<bool>
    {
        public bool VisitAllIff(AllIffViewModel iff) => true;

        public bool VisitHasAnyTagsInIff(HasAnyTagsInIffViewModel iff)
            => Util.HasOverlappingTags(medium, iff.Tags);

        public bool VisitHasAnyTagsLikeIff(HasAnyTagsLikeIffViewModel iff)
            => Util.HasTagsMatchingRegexes(medium, iff.Regexes);

        public bool VisitHasRatingIff(HasRatingIffViewModel iff) => Util.HasGoodRating(medium, iff.Rating);
    }

    private class DoesMediumViolateVisitor(SmallMediumDocument medium) : IThenViewModelVisitor<bool>
    {
        public bool VisitFailThen() => true;

        public bool VisitHasAllAbsentOrPresentThen(HasAllAbsentOrPresentThenViewModel then)
        {
            var combined = new HashSet<string>();
            combined.UnionWith(medium.InnateTags);
            combined.UnionWith(medium.AbsentTags);

            return then.Tags.Except(combined).Any();
        }

        public bool VisitHasAnyRatingThen() => medium.Rating == Rating.Unknown;

        public bool VisitHasAnyTagsInThen(HasAnyTagsInThenViewModel then)
            => !Util.HasOverlappingTags(medium, then.Tags);

        public bool VisitHasAnyTagsLikeThen(HasAnyTagsLikeThenViewModel then)
            => !Util.HasTagsMatchingRegexes(medium, then.Regexes);

        public bool VisitHasSpecificRatingThen(HasSpecificRatingThenViewModel then)
            => !Util.HasGoodRating(medium, then.Rating);
    }

    private static class Util
    {
        public static bool HasGoodRating(SmallMediumDocument medium, Rating expected)
        {
            if (medium.Rating == Rating.Unknown)
                return false;

            return medium.Rating == expected;
        }

        public static bool HasOverlappingTags(SmallMediumDocument medium, IEnumerable<string> expected)
            => medium.InnateTags.ToHashSet().Intersect(expected).Any();

        public static bool HasTagsMatchingRegexes(SmallMediumDocument medium, IEnumerable<string> regexStrings)
        {
            var regexes = regexStrings.Select(s => new Regex(s));
            return regexes.All(regex => medium.InnateTags.Any(regex.IsMatch));
        }
    }
}
