using Deevenue.Domain.Search;

namespace Deevenue.Domain.Media;

public interface IMediumService
{
    Task DeleteAsync(Guid mediumId);
    Task<IReadOnlySet<SmallMediumDocument>> GetAllSearchableAsync(bool? isSfwOverride = null);
    Task<SmallMediumDocument?> GetEnforceableAsync(Guid id);
    Task<PaginationViewModel<SearchResultViewModel>> PaginateAllAsync(PaginationParameters pagination);
    Task<bool> SetRatingAsync(Guid id, Rating rating);
    Task<ITryGetResult> TryGetAsync(Guid id);
}

public interface ITryGetResultVisitor<out T>
{
    T VisitSuccess(MediumViewModel medium);
    T VisitNotSfw();
    T VisitNotFound();
}

public interface ITryGetResult
{
    T Accept<T>(ITryGetResultVisitor<T> visitor);
}
