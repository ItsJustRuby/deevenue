using Deevenue.Domain.Jobs;

namespace Deevenue.Domain.Thumbnails;

internal class ThumbnailSheetService(
    IJobs jobs,
    IBucketStorage bucketStorage,
    IThumbnailStorage thumbnailStorage,
    IThumbnailSheetStorage thumbnailSheetStorage,
    IThumbnailSheetRepository repository) : IThumbnailSheetService
{
    public Task RejectAsync(Guid sheetId)
    {
        return Task.WhenAll(
            repository.RemoveAsync(sheetId),
            bucketStorage.RemoveAsync(new ThumbnailSheetBucket(sheetId))
        );
    }

    public async Task<Guid> ScheduleJobAsync(Guid mediumId, int thumbnailCount)
    {
        var sheetId = await repository.CreateAsync(mediumId, thumbnailCount);
        await jobs.ScheduleThumbnailSheetJobAsync(sheetId, mediumId, thumbnailCount);
        return sheetId;
    }

    public async Task SelectAsync(Guid sheetId, int thumbnailIndex)
    {
        var sheet = await repository.GetAsync(sheetId);
        if (sheet == null)
            return;

        await PersistAsync(sheet.MediumId, sheetId, thumbnailIndex);
        await RejectAsync(sheetId);
    }

    private async Task PersistAsync(Guid mediumId, Guid sheetId, int thumbnailIndex)
    {
        foreach (var size in ThumbnailSizes.All)
        {
            using var thumbnailStream = await thumbnailSheetStorage.StreamAsync(sheetId, thumbnailIndex, size);

            if (thumbnailStream == null)
                return;

            await thumbnailStorage.StoreAsync(mediumId, isAnimated: false, size, thumbnailStream);
        }
    }
}
