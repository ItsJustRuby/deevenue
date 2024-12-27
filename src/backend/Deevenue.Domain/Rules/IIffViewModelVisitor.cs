namespace Deevenue.Domain.Rules;

internal interface IIffViewModelVisitor<out T>
{
    T VisitHasRatingIff(HasRatingIffViewModel iff);
    T VisitAllIff(AllIffViewModel iff);
    T VisitHasAnyTagsInIff(HasAnyTagsInIffViewModel iff);
    T VisitHasAnyTagsLikeIff(HasAnyTagsLikeIffViewModel iff);
}
