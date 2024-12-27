using Deevenue.Domain.Thumbnails;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Deevenue.Infrastructure.MediaProcessing;

internal class ImageThumbnails : IImageThumbnails
{
    private const decimal minimumAspectRatio = 0.5m;

    public IReadOnlySet<CreatedThumbnail> Create(Stream stream)
    {
        using var image = Image.Load(stream);
        var isLandscape = image.Width > image.Height;

        var results = new HashSet<CreatedThumbnail>();
        foreach (var thumbnailSize in ThumbnailSizes.All)
        {
            var outStream = new MemoryStream();

            using var thumb = image.Clone(img =>
            {
                if (isLandscape)
                    img.Resize(0, thumbnailSize.PixelCount);
                else
                    img.Resize(thumbnailSize.PixelCount, 0);

                if (((decimal)image.Width / image.Height) < minimumAspectRatio)
                {
                    var maximumHeight = (int)Math.Ceiling(thumbnailSize.PixelCount / minimumAspectRatio);
                    img.Crop(new Rectangle(0, 0, thumbnailSize.PixelCount, maximumHeight));
                }
            });


            thumb.Save(outStream, new JpegEncoder());
            outStream.Seek(0, SeekOrigin.Begin);
            results.Add(new CreatedThumbnail(thumbnailSize, outStream));
        }

        stream.Seek(0, SeekOrigin.Begin);
        return results;
    }
}

