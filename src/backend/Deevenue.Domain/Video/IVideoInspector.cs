using Deevenue.Domain.Media;

namespace Deevenue.Domain.Video;

public interface IVideoInspector
{
    Task<MediumDimensions?> MeasureAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    );
}
