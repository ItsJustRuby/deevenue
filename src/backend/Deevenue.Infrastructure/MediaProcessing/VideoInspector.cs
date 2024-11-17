using Deevenue.Domain.Media;
using Deevenue.Domain.Video;
using FFMpegCore;
using FFMpegCore.Exceptions;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.MediaProcessing;

internal class VideoInspector(
    IVideoAnalysis videoAnalysis,
    ILogger<VideoInspector> logger) : IVideoInspector
{
    public async Task<MediumDimensions?> MeasureAsync(
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        IMediaAnalysis analysis;
        try
        {
            analysis = await videoAnalysis.ForAsync(stream, cancellationToken);
        }
        catch (FFMpegException e)
        {
            logger.LogError("Video file could not be analyzed: {e}.", e.Message);
            return null;
        }

        var videoStreamCount = analysis.VideoStreams.Count;
        if (videoStreamCount == 0)
        {
            logger.LogError("Video file does not contain any video streams.");
            return null;
        }
        else if (videoStreamCount > 1)
        {
            logger.LogWarning(
                "Analyzed video file had multiple ({n}) video streams. Using only the primary one.",
                videoStreamCount);
        }

        return new MediumDimensions(analysis.PrimaryVideoStream!.Width, analysis.PrimaryVideoStream!.Height);
    }
}
