namespace Deevenue.Domain.Thumbnails;

public interface IThumbnailSheetService
{
    Task RejectAsync(Guid sheetId);
    Task SelectAsync(Guid sheetId, int thumbnailIndex);
    Task<Guid> ScheduleJobAsync(Guid mediumId, int thumbnailCount);
}

public record ThumbnailSheetViewModel(Guid Id, Guid MediumId, int Count, bool IsComplete);
