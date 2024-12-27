using Deevenue.Domain;
using Deevenue.Domain.Media;
using Deevenue.Domain.Thumbnails;
using Deevenue.Domain.Video;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.Jobs;

internal class VideoPostProcessor(
    IThumbnailStorage thumbnailStorage,
    IVideoInspector videoInspector,
    IVideoThumbnails videoThumbnails,
    IMediumRepository mediumRepository,
    ILogger<VideoPostProcessor> logger) : IVideoPostProcessor
{
    public async Task CreateAndPersistThumbnailsAsync(Guid mediumId, MediumData mediumData)
    {
        if (mediumData == null)
        {
            logger.LogWarning("Tried to create thumbnail for non-existant medium {id}", mediumId);
            return;
        }

        using var memoryStream = new MemoryStream(mediumData.Bytes);
        var createdThumbnails = await videoThumbnails.CreateAsync(memoryStream);

        await Task.WhenAll(createdThumbnails.Select(t =>
        {
            return thumbnailStorage.StoreAsync(mediumId, isAnimated: false, t.Size, t.Stream);
        }));
    }

    public async Task PersistMeasurementsAsync(Guid mediumId, MediumData mediumData)
    {
        if (mediumData == null)
        {
            logger.LogWarning("Tried to persist measurements of non-existant medium {id}", mediumId);
            return;
        }

        if (MediaKinds.Parse(mediumData.ContentType) != MediaKind.Video)
        {
            logger.LogWarning(
                "Tried to persist measurements of a medium that was not a video. " +
                "Its actual ContentType was \"{ct}\"", mediumData.ContentType);
            return;
        }

        using var memoryStream = new MemoryStream(mediumData.Bytes);
        var dimensions = await videoInspector.MeasureAsync(memoryStream);

        if (dimensions == null)
        {
            logger.LogError("Failed to measure medium {id}", mediumId);
            return;
        }

        await mediumRepository.UpdateDimensionsAsync(mediumId, dimensions);
        logger.LogDebug("Updated dimensions of medium {id} to {w} x {h}",
            mediumId,
            dimensions.Width,
            dimensions.Height);
    }
}
