using Deevenue.Domain.Jobs;
using Deevenue.Domain.Video;

namespace Deevenue.Domain.Media;

internal interface IAddVideoService : IAddService { }

internal class AddVideoService(IVideoThumbnails videoThumbnails, IJobs jobs) : IAddVideoService
{
    private bool hasInitialThumbnails = true;

    public async Task PostProcessAsync(Guid mediumId)
    {
        await jobs.SchedulePostProcessingJobAsync(mediumId, hasInitialThumbnails);
    }

    public async Task<PreProcessResult> PreProcessAsync(Stream stream)
    {
        var createdThumbnails = await videoThumbnails.CreateTimeLimitedAsync(stream);
        if (createdThumbnails.Count == 0)
            hasInitialThumbnails = false;
        return new PreProcessResult(null, createdThumbnails);
    }
}
