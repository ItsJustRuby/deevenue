using FFMpegCore;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.MediaProcessing;
internal interface IVideoAnalysis
{
    Task<IMediaAnalysis> ForAsync(Stream stream, CancellationToken cancellationToken = default);
    Task<IMediaAnalysis> ForAsync(string path, CancellationToken cancellationToken = default);
}

internal class VideoAnalysis(ILogger<VideoAnalysis> logger) : IVideoAnalysis
{
    public Task<IMediaAnalysis> ForAsync(string path, CancellationToken cancellationToken = default)
    {
        return FFProbe.AnalyseAsync(path, new FFOptions
        {
            LogLevel = logger.GetFFMpegLogLevel()
        }, cancellationToken);
    }

    public async Task<IMediaAnalysis> ForAsync(Stream stream, CancellationToken cancellationToken = default)
    {
        IMediaAnalysis result;
        using (var inputFile = TemporaryVideoFile.From(stream))
        {
            result = await ForAsync(inputFile, cancellationToken);
        }
        return result;
    }
}
