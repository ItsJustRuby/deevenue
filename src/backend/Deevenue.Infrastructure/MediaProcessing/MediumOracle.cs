using Deevenue.Domain.Media;
using SixLabors.ImageSharp;

namespace Deevenue.Infrastructure.MediaProcessing;

internal class MediumOracle : IMediumOracle
{
    public MediaKind GuessMediaKind(MediumFileData mediumFileData)
    {
        if (mediumFileData.ContentType != "image/gif")
            return MediaKinds.Parse(mediumFileData.ContentType);

        var gif = Image.Load(mediumFileData.Stream);
        mediumFileData.Stream.Seek(0, SeekOrigin.Begin);

        if (gif.Frames.Count > 1)
            // In this case, the caller should be able to figure out that they might want to
            // convert image/gif to a video instead of using that weird hybrid format.
            return MediaKind.Video;
        return MediaKind.Image;
    }
}
