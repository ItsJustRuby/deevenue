using System.Reactive.Linq;
using Deevenue.Domain;
using Minio;
using Minio.DataModel.Args;

namespace Deevenue.Infrastructure.Storage;

internal class BucketStorage(IMinioClientFactory minioClientFactory)
    : StorageBase(minioClientFactory), IBucketStorage
{
    public async Task CreateAsync(IBucket bucket)
    {
        using var minioClient = CreateClient();
        await minioClient.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucket.Name));
    }

    public async Task RemoveAsync(IBucket bucket)
    {
        using var minioClient = CreateClient();
        var doesBucketExist = await minioClient
            .BucketExistsAsync(new BucketExistsArgs().WithBucket(bucket.Name));

        var objects = minioClient.ListObjectsEnumAsync(new ListObjectsArgs().WithBucket(bucket.Name));
        var keys = objects.ToBlockingEnumerable().Select(o => o.Key).ToList();
        if (keys.Count > 0)
            await minioClient.RemoveObjectsAsync(new RemoveObjectsArgs().WithBucket(bucket.Name).WithObjects(keys));

        await minioClient.RemoveBucketAsync(new RemoveBucketArgs().WithBucket(bucket.Name));
    }
}
