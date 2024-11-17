using Deevenue.Domain.Media;
using Deevenue.Domain.Rules;
using Deevenue.Domain.Search;
using Deevenue.Domain.Thumbnails;
using Microsoft.Extensions.DependencyInjection;

namespace Deevenue.Domain;

public static class DomainSetup
{
    public static void AddDomain(this IDependencyInjection di)
    {
        StaticConfig.Bootstrap(di);

        di.Services.AddTransient<ISearchService, SearchService>();
        di.Services.AddTransient<IMediumService, MediumService>();
        di.Services.AddTransient<IAddMediumService, AddMediumService>();
        di.Services.AddTransient<ISimilarMediaService, SimilarMediaService>();
        di.Services.AddTransient<IMediumTagService, MediumTagService>();
        di.Services.AddTransient<IRulesService, RulesService>();
        di.Services.AddTransient<IThumbnailSheetService, ThumbnailSheetService>();

        // internal
        di.Services.AddSingleton<ITaggyFileName, TaggyFileName>();
        di.Services.AddSingleton<IAddImageService, AddImageService>();
        di.Services.AddTransient<IAddVideoService, AddVideoService>();
    }
}
