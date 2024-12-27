using System.Reactive.Linq;
using Deevenue.Domain;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;

namespace Deevenue.Infrastructure.Storage;

internal class StorageBase(IMinioClientFactory minioClientFactory)
{
    protected async Task RemoveAsync(IStorageLocation location)
    {
        using var minioClient = CreateClient();

        await minioClient.RemoveObjectAsync(
            new RemoveObjectArgs()
            .WithBucket(location.Bucket.Name)
            .WithObject(location.FileName)
        );
    }

    protected async Task StoreAsync(StoreAsyncArgs args)
    {
        using var minioClient = CreateClient();

        await minioClient.PutObjectAsync(
            new PutObjectArgs()
            .WithBucket(args.Location.Bucket.Name)
            .WithContentType(args.ContentType)
            .WithObject(args.Location.FileName)
            .WithObjectSize(args.Size)
            .WithStreamData(args.ReadStream)
        );
    }

    protected async Task<MediumData?> GetAsync(IStorageLocation args)
    {
        using var minioClient = CreateClient();

        try
        {
            using var memoryStream = new MemoryStream();

            var fromStorage = await minioClient.GetObjectAsync(
                new GetObjectArgs()
                .WithBucket(args.Bucket.Name)
                .WithObject(args.FileName)
                .WithCallbackStream(s => s.CopyTo(memoryStream))
            );

            return new MediumData(memoryStream.ToArray(), fromStorage.ContentType);
        }
        catch (ObjectNotFoundException)
        {
            // It wasn't my idea to use exceptions for ordinary control flow.
            // There seems to be no alternative (even StatObjectAsync throws).
            return null;
        }
    }

    protected async Task<Stream?> StreamAsync(IStorageLocation args)
    {
        using var minioClient = CreateClient();
        var memoryStream = new MemoryStream();

        try
        {
            await minioClient.GetObjectAsync(
                new GetObjectArgs()
                .WithBucket(args.Bucket.Name)
                .WithObject(args.FileName)
                .WithCallbackStream(s => s.CopyTo(memoryStream))
            );
        }
        catch (ObjectNotFoundException)
        {
            await memoryStream.DisposeAsync();
            return null;
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        return memoryStream;
    }

    protected IMinioClient CreateClient() => minioClientFactory.CreateClient();

    protected record StoreAsyncArgs(IStorageLocation Location, string ContentType, long Size, Stream ReadStream);
}
