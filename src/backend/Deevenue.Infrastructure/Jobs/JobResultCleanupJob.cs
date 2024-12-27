using Deevenue.Infrastructure.Db;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

[JobKindName("Job result cleanup job")]
internal class JobResultCleanupJob(
    IJobResultRepository jobRepository,
    ILogger<JobResultCleanupJob> logger) : DeevenueJobBase(jobRepository)
{
    public override JobSummaryData GetSummaryData(IJobExecutionContext context)
        => new(null);

    protected override async Task ActuallyExecute(IJobExecutionContext context)
    {
        logger.LogInformation("Job result cleanup job starting");
        var deleteCount = await jobRepository.DeleteOutdatedAsync(DateTime.UtcNow.AddDays(-30));
        logger.LogInformation("Removed {n} outdated job results", deleteCount);
    }
}
