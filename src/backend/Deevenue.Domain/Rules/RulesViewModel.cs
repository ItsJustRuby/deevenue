using System.Text.Json.Serialization;

namespace Deevenue.Domain.Rules;

public class RulesViewModel
{
    public static readonly RulesViewModel Empty = new() { Rules = [] };
    public required RuleViewModel[] Rules { get; init; }
}

public class RuleViewModel
{
    public required IEnumerable<IIffViewModel> Iffs { get; init; }
    public required IEnumerable<IThenViewModel> Thens { get; init; }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HasRatingIffViewModel), "hasRating")]
[JsonDerivedType(typeof(AllIffViewModel), "all")]
[JsonDerivedType(typeof(HasAnyTagsInIffViewModel), "hasAnyTagsIn")]
[JsonDerivedType(typeof(HasAnyTagsLikeIffViewModel), "hasAnyTagsLike")]
public interface IIffViewModel
{
    internal T Accept<T>(IIffViewModelVisitor<T> visitor);
}

public class HasRatingIffViewModel : IIffViewModel
{
    public required Rating Rating { get; init; }

    T IIffViewModel.Accept<T>(IIffViewModelVisitor<T> visitor)
        => visitor.VisitHasRatingIff(this);
}

public class AllIffViewModel : IIffViewModel
{
    T IIffViewModel.Accept<T>(IIffViewModelVisitor<T> visitor)
        => visitor.VisitAllIff(this);
}

public class HasAnyTagsInIffViewModel : IIffViewModel
{
    public required IEnumerable<string> Tags { get; init; }

    T IIffViewModel.Accept<T>(IIffViewModelVisitor<T> visitor)
        => visitor.VisitHasAnyTagsInIff(this);
}

public class HasAnyTagsLikeIffViewModel : IIffViewModel
{
    public required IEnumerable<string> Regexes { get; init; }

    T IIffViewModel.Accept<T>(IIffViewModelVisitor<T> visitor)
        => visitor.VisitHasAnyTagsLikeIff(this);
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(typeof(HasSpecificRatingThenViewModel), "hasRating")]
[JsonDerivedType(typeof(HasAnyRatingThenViewModel), "hasAnyRating")]
[JsonDerivedType(typeof(FailThenViewModel), "fail")]
[JsonDerivedType(typeof(HasAllAbsentOrPresentThenViewModel), "hasAllAbsentOrPresent")]
[JsonDerivedType(typeof(HasAnyTagsInThenViewModel), "hasAnyTagsIn")]
[JsonDerivedType(typeof(HasAnyTagsLikeThenViewModel), "hasAnyTagsLike")]
public interface IThenViewModel
{
    internal T Accept<T>(IThenViewModelVisitor<T> visitor);
}

public class HasSpecificRatingThenViewModel : IThenViewModel
{
    public required Rating Rating { get; init; }

    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitHasSpecificRatingThen(this);
}

public class HasAnyRatingThenViewModel : IThenViewModel
{
    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitHasAnyRatingThen();
}

public class FailThenViewModel : IThenViewModel
{
    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitFailThen();
}

public class HasAllAbsentOrPresentThenViewModel : IThenViewModel
{
    public required IEnumerable<string> Tags { get; init; }

    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitHasAllAbsentOrPresentThen(this);
}

public class HasAnyTagsInThenViewModel : IThenViewModel
{
    public required IEnumerable<string> Tags { get; init; }

    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitHasAnyTagsInThen(this);
}

public class HasAnyTagsLikeThenViewModel : IThenViewModel
{
    public required IEnumerable<string> Regexes { get; init; }

    T IThenViewModel.Accept<T>(IThenViewModelVisitor<T> visitor)
        => visitor.VisitHasAnyTagsLikeThen(this);
}
