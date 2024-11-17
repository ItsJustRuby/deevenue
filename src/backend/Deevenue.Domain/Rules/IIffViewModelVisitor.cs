namespace Deevenue.Domain.Rules;

internal interface IIffViewModelVisitor<T>
{
    T VisitHasRatingIff(HasRatingIffViewModel iff);
    T VisitAllIff(AllIffViewModel iff);
    T VisitHasAnyTagsInIff(HasAnyTagsInIffViewModel iff);
    T VisitHasAnyTagsLikeIff(HasAnyTagsLikeIffViewModel iff);
}
