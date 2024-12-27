using FFMpegCore.Enums;
using Microsoft.Extensions.Logging;

namespace Deevenue.Infrastructure.MediaProcessing;

internal static class ILoggerExtensions
{
    public static FFMpegLogLevel GetFFMpegLogLevel(this ILogger logger)
    {
        var ffmpegEquivalentOf = new Dictionary<LogLevel, FFMpegLogLevel>
        {
            [LogLevel.Trace] = FFMpegLogLevel.Trace,
            [LogLevel.Debug] = FFMpegLogLevel.Debug,
            [LogLevel.Information] = FFMpegLogLevel.Info,
            [LogLevel.Warning] = FFMpegLogLevel.Warning,
            [LogLevel.Error] = FFMpegLogLevel.Error,
        };

        foreach (var loggerLogLevel in Enum.GetValues<LogLevel>().OrderBy(ll => (int)ll))
        {
            if (logger.IsEnabled(loggerLogLevel))
                return ffmpegEquivalentOf[loggerLogLevel];
        }

        return FFMpegLogLevel.Error;
    }
}
