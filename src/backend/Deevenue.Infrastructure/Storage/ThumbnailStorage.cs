using Deevenue.Domain;
using Deevenue.Domain.Thumbnails;
using Minio;

namespace Deevenue.Infrastructure.Storage;

internal class ThumbnailStorage(IMinioClientFactory minioClientFactory) :
    StorageBase(minioClientFactory), IThumbnailStorage
{
    public Task<MediumData?> GetAsync(Guid mediumId, bool isAnimated, IThumbnailSize size)
        => GetAsync(new Location(mediumId, size, isAnimated));

    public Task RemoveAllAsync(Guid mediumId)
    {
        // This seems more readable than taking the cross product of (ThumnailSizes.All x [false, true]).
        var jobs = new List<Task>();
        foreach (var size in ThumbnailSizes.All)
        {
            jobs.Add(RemoveAsync(new Location(mediumId, size, isAnimated: true)));
            jobs.Add(RemoveAsync(new Location(mediumId, size, isAnimated: false)));
        }

        return Task.WhenAll(jobs);
    }

    public Task StoreAsync(Guid mediumId, bool isAnimated, IThumbnailSize size, Stream stream)
    {
        return StoreAsync(new StoreAsyncArgs(
            new Location(mediumId, size, isAnimated),
            ContentType: isAnimated ? "video/webm" : "image/jpeg",
            ReadStream: stream,
            Size: stream.Length
        ));
    }

    private class Location(Guid mediumId, IThumbnailSize size, bool isAnimated) : IStorageLocation
    {
        public IBucket Bucket => StaticBuckets.Thumbs;

        public string FileName
        {
            get
            {
                var extension = isAnimated ? "webm" : "jpg";
                return $"{mediumId}_{size.Abbreviation}.{extension}".ToLowerInvariant();
            }
        }
    }
}
