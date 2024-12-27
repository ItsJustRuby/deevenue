namespace Deevenue.Domain.Thumbnails;

public interface IThumbnailSheetRepository
{
    Task CompleteAsync(Guid id);
    Task<Guid> CreateAsync(Guid mediumId, int count);
    Task<IReadOnlySet<Guid>> GetAllIncompleteAsync();
    Task<ThumbnailSheetViewModel?> GetAsync(Guid id);
    Task RemoveAsync(Guid id);
}
