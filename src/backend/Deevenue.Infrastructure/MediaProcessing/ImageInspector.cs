using Deevenue.Domain.Media;
using SixLabors.ImageSharp;

namespace Deevenue.Infrastructure.MediaProcessing;

internal class ImageInspector : IImageInspector
{
    public MediumDimensions Measure(Stream stream)
    {
        using var image = Image.Load(stream);
        stream.Seek(0, SeekOrigin.Begin);
        return new MediumDimensions(image.Width, image.Height);
    }
}
