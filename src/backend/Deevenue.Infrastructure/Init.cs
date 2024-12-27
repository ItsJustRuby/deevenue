using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Thumbnails;
using Deevenue.Domain.Video;
using Deevenue.Infrastructure.Db;
using Deevenue.Infrastructure.Jobs;
using Deevenue.Infrastructure.MediaProcessing;
using Deevenue.Infrastructure.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Deevenue.Infrastructure;

public static class InfrastructureSetup
{
    public static void AddInfrastructure(this IDependencyInjection dependencyInjection)
    {
        dependencyInjection.AddDomain();

        dependencyInjection.AddDb();
        dependencyInjection.AddStorage();
        dependencyInjection.AddJobs();

        dependencyInjection.Services.AddSingleton<IMediumOracle, MediumOracle>();
        dependencyInjection.Services.AddSingleton<IVideoInspector, VideoInspector>();
        dependencyInjection.Services.AddSingleton<IImageInspector, ImageInspector>();
        dependencyInjection.Services.AddSingleton<IVideoThumbnails, VideoThumbnails>();
        dependencyInjection.Services.AddSingleton<IImageThumbnails, ImageThumbnails>();
        dependencyInjection.Services.AddSingleton<IVideoAnalysis, VideoAnalysis>();
    }
}

public static class InfrastructureInit
{
    public static void UseInfrastructure(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        host.UseJobs();
        host.UseStorageAsync().Wait();
    }
}
