using Deevenue.Domain;
using Deevenue.Domain.Rules;
using Minio;

namespace Deevenue.Infrastructure.Storage;

internal class RulesStorage(IMinioClientFactory minioClientFactory) :
    StorageBase(minioClientFactory), IRulesStorage
{
    public async Task StoreAsync(Stream stream)
    {
        await StoreAsync(
            new StoreAsyncArgs(
                RulesStorageLocation.Instance,
                "application/json",
                stream.Length,
                stream
            )
        );
    }

    public Task<Stream?> StreamAsync() => StreamAsync(RulesStorageLocation.Instance);

    private class RulesStorageLocation : IStorageLocation
    {
        public static readonly RulesStorageLocation Instance = new();
        private RulesStorageLocation() { }

        public IBucket Bucket => StaticBuckets.Rules;
        public string FileName => "rules.json";
    }
}
