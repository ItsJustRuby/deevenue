namespace Deevenue.Domain.Jobs;

public interface IJobs
{
    Task ScheduleThumbnailSheetJobAsync(Guid sheetId, Guid mediumId, int count);
    Task SchedulePostProcessingJobAsync(Guid mediumId, bool hasInitialThumbnails);
}
