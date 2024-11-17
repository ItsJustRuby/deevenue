using Deevenue.Domain.Thumbnails;

namespace Deevenue.Domain.Video;

public interface IVideoThumbnails
{
    Task<IReadOnlySet<CreatedThumbnail>> CreateAsync(Stream inputStream, CancellationToken ct = default);

    Task<IReadOnlySet<CreatedThumbnail>> CreateTimeLimitedAsync(Stream inputStream);
}
