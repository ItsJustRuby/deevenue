namespace Deevenue.Domain.Rules;

internal interface IThenViewModelVisitor<T>
{
    T VisitHasSpecificRatingThen(HasSpecificRatingThenViewModel then);
    T VisitFailThen();
    T VisitHasAllAbsentOrPresentThen(HasAllAbsentOrPresentThenViewModel then);
    T VisitHasAnyTagsInThen(HasAnyTagsInThenViewModel then);
    T VisitHasAnyTagsLikeThen(HasAnyTagsLikeThenViewModel then);
    T VisitHasAnyRatingThen();
}
