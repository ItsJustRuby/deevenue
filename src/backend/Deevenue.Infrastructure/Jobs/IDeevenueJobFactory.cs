using Quartz;

namespace Deevenue.Infrastructure.Jobs;

[AttributeUsage(AttributeTargets.Class)]
#pragma warning disable CS9113 // Parameter is unread.
internal class JobKindNameAttribute(string Name) : Attribute { }
#pragma warning restore CS9113 // Parameter is unread.

internal interface IDeevenueJobFactory<TJob, TParameters> where TJob : IDeevenueJob
{
    JobDataMap StoreParameters(TParameters parameters);
    string CreateIdentity(TParameters parameters);

    public async Task ScheduleNewJob(IJobScheduler jobScheduler, TParameters parameters)
    {
        var identity = CreateIdentity(parameters);
        var jobData = StoreParameters(parameters);

        var job = JobBuilder.Create<TJob>()
            .SetJobData(jobData)
            .WithIdentity(identity, typeof(TJob).Name)
            .StoreDurably()
            .Build();

        var scheduler = await jobScheduler.UnwrapAsync();

        await scheduler.AddJob(job, replace: true);
        await scheduler.TriggerJob(new JobKey(identity, typeof(TJob).Name));
    }
}
