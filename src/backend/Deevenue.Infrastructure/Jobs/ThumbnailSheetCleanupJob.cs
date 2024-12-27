using Deevenue.Domain.Thumbnails;
using Deevenue.Infrastructure.Db;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Deevenue.Infrastructure.Jobs;

[JobKindName("Thumbnail sheet cleanup job")]
internal class ThumbnailSheetCleanupJob(
    IJobResultRepository jobRepository,
    IThumbnailSheetService service,
    IThumbnailSheetRepository repository,
    ILogger<ThumbnailSheetCleanupJob> logger) : DeevenueJobBase(jobRepository)
{
    public override JobSummaryData GetSummaryData(IJobExecutionContext context)
        => new(null);

    protected override async Task ActuallyExecute(IJobExecutionContext context)
    {
        logger.LogInformation("Thumbnail sheet cleanup job starting");
        var all = await repository.GetAllIncompleteAsync();
        await Task.WhenAll(all.Select(service.RejectAsync));
        logger.LogInformation("Removed {n} expired thumbnail sheets", all.Count);
    }
}
