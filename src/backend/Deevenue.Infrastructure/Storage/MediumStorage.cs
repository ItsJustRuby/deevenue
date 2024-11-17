using Deevenue.Domain;
using Deevenue.Domain.Media;
using Minio;

namespace Deevenue.Infrastructure.Storage;

internal class MediumStorage(IMinioClientFactory minioClientFactory) :
    StorageBase(minioClientFactory), IMediumStorage
{
    public Task<MediumData?> GetAsync(Guid id)
        => GetAsync(new Location(id));

    public Task RemoveAsync(Guid id) => RemoveAsync(new Location(id));

    public Task StoreAsync(Guid id, string ContentType, long Size, Stream ReadStream)
        => StoreAsync(new StoreAsyncArgs(new Location(id), ContentType, Size, ReadStream));

    private class Location(Guid mediumId) : IStorageLocation
    {
        public IBucket Bucket => StaticBuckets.Media;
        public string FileName => mediumId.ToString();
    }
}
