using Deevenue.Domain.Jobs;

namespace Deevenue.Infrastructure.Jobs;

internal class Jobs(IJobScheduler jobScheduler) : IJobs
{
    public async Task ScheduleThumbnailSheetJobAsync(Guid sheetId, Guid mediumId, int count)
        => await ThumbnailSheetJobFactory.Instance.ScheduleNewJob(
            jobScheduler, new ThumbnailSheetJobParameters(sheetId, mediumId, count));

    public async Task SchedulePostProcessingJobAsync(Guid mediumId, bool hasInitialThumbnails)
        => await VideoPostProcessingJobFactory.Instance.ScheduleNewJob(
            jobScheduler, new PostProcessingJobParameters(mediumId, hasInitialThumbnails));
}
