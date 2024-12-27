namespace Deevenue.Domain.Thumbnails;

public interface IImageThumbnails
{
    IReadOnlySet<CreatedThumbnail> Create(Stream stream);
}
