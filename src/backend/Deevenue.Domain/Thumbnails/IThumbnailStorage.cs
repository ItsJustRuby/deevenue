namespace Deevenue.Domain.Thumbnails;

public interface IThumbnailStorage
{
    Task<MediumData?> GetAsync(Guid mediumId, bool isAnimated, IThumbnailSize size);
    Task RemoveAllAsync(Guid mediumId);
    Task StoreAsync(Guid mediumId, bool isAnimated, IThumbnailSize size, Stream stream);
}
