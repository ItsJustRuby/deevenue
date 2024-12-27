namespace Deevenue.Infrastructure.Jobs;

public interface IJobsSummary
{
    Task<IReadOnlyDictionary<string, IReadOnlySet<JobSummaryData>>> GetRunningAsync();
}

public record JobSummaryData(Guid? MediumId);

internal class JobsSummary(IJobScheduler jobScheduler) : IJobsSummary
{
    public async Task<IReadOnlyDictionary<string, IReadOnlySet<JobSummaryData>>> GetRunningAsync()
    {
        var quartzScheduler = await jobScheduler.UnwrapAsync();
        var currentExecutionContexts = await quartzScheduler.GetCurrentlyExecutingJobs();

        var results = new Dictionary<string, ISet<JobSummaryData>>();
        foreach (var jobKindName in JobNames.JobKindNames)
            results[jobKindName] = new HashSet<JobSummaryData>();

        foreach (var executionContext in currentExecutionContexts)
        {
            var jobTypeName = executionContext.JobDetail.JobType.Name;
            var jobKindName = JobNames.JobKindNameByJobTypeName[jobTypeName];

            var dataSet = ((IDeevenueJob)executionContext.JobInstance).GetSummaryData(executionContext);
            results[jobKindName].Add(dataSet);
        }

        return results.ToDictionary(kvp => kvp.Key, kvp => (IReadOnlySet<JobSummaryData>)kvp.Value);
    }
}
