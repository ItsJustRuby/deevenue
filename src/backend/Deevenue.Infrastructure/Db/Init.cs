using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Thumbnails;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Deevenue.Infrastructure.Db;

internal static class DbSetup
{
    public static void AddDb(this IDependencyInjection dependencyInjection)
    {
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = Config.Db.Host,
            Database = Config.Db.Database,
            Port = Config.Db.Port,
            Username = Config.Db.User,
            Password = Config.Db.Password,
            IncludeErrorDetail = Config.Environment.AllowsSensitiveDataLogging
        };

        void Setup(DbContextOptionsBuilder options)
        {
            options.UseNpgsql(builder.ConnectionString, o =>
            {
                // Seems safe enough: https://go.microsoft.com/fwlink/?linkid=2134277
                o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);

            });
            if (Config.Environment.AllowsSensitiveDataLogging)
                options.EnableSensitiveDataLogging();
        }
        dependencyInjection.Services.AddDbContext<DeevenueContext>(Setup);
        dependencyInjection.Services.AddDbContextFactory<DeevenueContext>(
            Setup,
            lifetime: Config.Environment.RequiresScopedDbContextFactoryLifetime
                ? ServiceLifetime.Scoped
                : ServiceLifetime.Singleton);

        dependencyInjection.Services.AddTransient<IMediumRepository, MediumRepository>();
        dependencyInjection.Services.AddTransient<IMediumTagRepository, MediumTagRepository>();
        dependencyInjection.Services.AddTransient<ITagRepository, TagRepository>();
        dependencyInjection.Services.AddTransient<IOrphanTagsRepository, OrphanTagsRepository>();
        dependencyInjection.Services.AddTransient<IThumbnailSheetRepository, ThumbnailSheetRepository>();

        dependencyInjection.Services.AddTransient<IJobResultRepository, JobResultRepository>();
        dependencyInjection.Services.AddTransient<IInternalTagRepository, TagRepository>();
    }
}
