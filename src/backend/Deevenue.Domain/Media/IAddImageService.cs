using Deevenue.Domain.Thumbnails;

namespace Deevenue.Domain.Media;

internal interface IAddImageService : IAddService { }

internal class AddImageService(
    IImageInspector inspector,
    IImageThumbnails thumbnails) : IAddImageService
{
    public Task PostProcessAsync(Guid mediumId) => Task.CompletedTask;

    public Task<PreProcessResult> PreProcessAsync(Stream stream)
    {
        return Task.FromResult(new PreProcessResult(
            inspector.Measure(stream),
            thumbnails.Create(stream)
        ));
    }
}
