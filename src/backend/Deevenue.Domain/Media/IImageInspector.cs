namespace Deevenue.Domain.Media;

public interface IImageInspector
{
    MediumDimensions Measure(Stream stream);
}
