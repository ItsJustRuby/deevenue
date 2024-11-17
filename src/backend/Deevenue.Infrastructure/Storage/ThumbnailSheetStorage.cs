using Deevenue.Domain;
using Deevenue.Domain.Thumbnails;
using Minio;

namespace Deevenue.Infrastructure.Storage;

internal class ThumbnailSheetStorage(IMinioClientFactory minioClientFactory) :
    StorageBase(minioClientFactory), IThumbnailSheetStorage
{
    public Task<MediumData?> GetAsync(Guid sheetId, int index, IThumbnailSize thumbnailSize)
        => GetAsync(new Location(sheetId, index, thumbnailSize));

    public Task StoreAsync(Guid mediumId, int index, Stream stream, IThumbnailSize thumbnailSize)
    {
        return StoreAsync(new StoreAsyncArgs(
            new Location(mediumId, index, thumbnailSize),
            ContentType: "image/jpeg",
            ReadStream: stream,
            Size: stream.Length
        ));
    }

    public Task<Stream?> StreamAsync(Guid sheetId, int index, IThumbnailSize thumbnailSize)
        => StreamAsync(new Location(sheetId, index, thumbnailSize));

    private class Location(Guid sheetId, int itemIndex, IThumbnailSize thumbnailSize) : IStorageLocation
    {
        public IBucket Bucket => new ThumbnailSheetBucket(sheetId);
        public string FileName => $"{itemIndex}_{thumbnailSize}.jpg";
    }
}
