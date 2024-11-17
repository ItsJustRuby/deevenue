using System.Diagnostics;
using Deevenue.Domain.Thumbnails;
using Deevenue.Domain.Video;
using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Exceptions;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.MediaProcessing;

internal class VideoThumbnails(
    IVideoAnalysis videoAnalysis,
    ILogger<VideoThumbnails> logger) : IVideoThumbnails
{
    public async Task<IReadOnlySet<CreatedThumbnail>> CreateAsync(Stream inputStream, CancellationToken ct = default)
    {
        using var inputFile = TemporaryVideoFile.From(inputStream);

        IMediaAnalysis analysis;
        try
        {
            analysis = await videoAnalysis.ForAsync(inputFile, ct);
        }
        catch (FFMpegException e)
        {
            logger.LogError("Video file could not be analyzed: {e}.", e.Message);
            throw new Exception("Video file could not be analyzed", e);
        }

        var isLandscape = analysis.PrimaryVideoStream!.Width > analysis.PrimaryVideoStream!.Height;

        var results = new HashSet<CreatedThumbnail>();
        foreach (var thumbnailSize in ThumbnailSizes.All)
        {
            var stopwatch = new Stopwatch();
            var tempOutputFileName = Path.GetTempFileName();

            var arguments = FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(tempOutputFileName, overwrite: true, options =>
                {
                    var customArgument = isLandscape ?
                        $"-vf thumbnail,scale={thumbnailSize.PixelCount}:-1" :
                        $"-vf thumbnail,scale=-1:{thumbnailSize.PixelCount}";

                    options
                        .WithCustomArgument(customArgument)
                        .WithFrameOutputCount(1)
                        .WithCustomArgument("-update 1")
                        // This is JPEG, don't worry
                        .WithArgument(new ForceFormatArgument("image2"));
                })
                .WithLogLevel(logger.GetFFMpegLogLevel())
                .CancellableThrough(ct);

            logger.LogDebug("Calling ffmpeg: {cmd}", arguments.Arguments);

            stopwatch.Start();
            await arguments.ProcessAsynchronously(throwOnError: true);

            logger.LogDebug("Completed thumbnailing after {t} ms", stopwatch.ElapsedMilliseconds);
            using var outputFile = File.OpenRead(tempOutputFileName);
            var memoryStream = new MemoryStream();
            outputFile.CopyTo(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);
            outputFile.Close();
            File.Delete(tempOutputFileName);

            results.Add(new CreatedThumbnail(thumbnailSize, memoryStream));
        }

        return results;
    }

    public async Task<IReadOnlySet<CreatedThumbnail>> CreateTimeLimitedAsync(Stream stream)
    {
        var ffmpegTask = await TimeLimitedTask.Run(
            TimeSpan.FromMilliseconds(Config.Media.OperationTimeoutMs),
            (ct) => CreateAsync(stream, ct)
        );

        if (ffmpegTask.ExceededTimeLimit)
            return new HashSet<CreatedThumbnail>();

        return ffmpegTask.Result!;
    }
}
