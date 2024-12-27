using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Rules;
using Deevenue.Domain.Thumbnails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Minio;
using Minio.DataModel.Args;

namespace Deevenue.Infrastructure.Storage;

internal static class StorageSetup
{
    public static void AddStorage(this IDependencyInjection di)
    {
        di.Services.AddMinio(options =>
        {
            options
                .WithEndpoint(Config.Storage.Endpoint)
                .WithCredentials(Config.Storage.AccessKey, Config.Storage.SecretKey)
                .WithSSL(false)
                .Build();
        });

        di.Services.AddSingleton<IMediumStorage, MediumStorage>();
        di.Services.AddSingleton<IRulesStorage, RulesStorage>();
        di.Services.AddSingleton<IBucketStorage, BucketStorage>();
        di.Services.AddSingleton<IThumbnailStorage, ThumbnailStorage>();
        di.Services.AddSingleton<IThumbnailSheetStorage, ThumbnailSheetStorage>();
    }
}

internal static class StorageInit
{
    public static async Task UseStorageAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var clientFactory = services.GetRequiredService<IMinioClientFactory>();
        using var client = clientFactory.CreateClient();

        foreach (var bucket in StaticBuckets.All)
        {
            var bucketName = bucket.Name;
            var doesBucketExist = await client.BucketExistsAsync(
                new BucketExistsArgs().WithBucket(bucketName)
            );

            if (!doesBucketExist)
                await client.MakeBucketAsync(new MakeBucketArgs().WithBucket(bucketName));
        }
    }
}
