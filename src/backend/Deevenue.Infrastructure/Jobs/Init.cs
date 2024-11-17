using Deevenue.Domain;
using Deevenue.Domain.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

internal static class JobsSetup
{
    public static void AddJobs(this IDependencyInjection di)
    {
        di.Services.AddQuartz();
        di.Services.AddSingleton<IJobs, Jobs>();
        di.Services.AddSingleton<IJobsSummary, JobsSummary>();

        di.Services.AddTransient<IJobResultService, JobResultService>();
        di.Services.AddTransient<IAnimatedThumbnails, AnimatedThumbnails>();
        di.Services.AddTransient<IVideoPostProcessor, VideoPostProcessor>();
    }
}

internal static class JobsInit
{
    public static void UseJobs(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;
        var schedulerFactory = services.GetRequiredService<ISchedulerFactory>();
        var scheduler = schedulerFactory.GetScheduler().Result;

        foreach (var jobType in new List<Type>([typeof(ThumbnailSheetCleanupJob), typeof(JobResultCleanupJob)]))
        {
            // Once on startup
            scheduler.ScheduleJob(
                JobBuilder.Create(jobType).Build(), TriggerBuilder.Create()
                .StartAt(DateTimeOffset.Now.AddSeconds(10))
                .Build()
            ).Wait();

            // Then at 5 am every day (+- time zone shenanigans)
            scheduler.ScheduleJob(
                JobBuilder.Create(jobType).Build(),
                TriggerBuilder.Create()
                .WithCronSchedule("0 0 5 * * ?")
                .Build()
            ).Wait();
        }

        // Run backup weekly on sunday
        scheduler.ScheduleJob(
            JobBuilder.Create<BackupJob>().Build(),
            TriggerBuilder.Create()
            .WithCronSchedule("0 0 2 ? * 1")
            .Build()
        ).Wait();
    }
}
