namespace Deevenue.Domain.Thumbnails;

public interface IThumbnailSheetStorage
{
    Task<MediumData?> GetAsync(Guid sheetId, int index, IThumbnailSize thumbnailSize);
    Task StoreAsync(Guid sheetId, int index, Stream stream, IThumbnailSize thumbnailSize);
    Task<Stream?> StreamAsync(Guid sheetId, int index, IThumbnailSize thumbnailSize);
}
