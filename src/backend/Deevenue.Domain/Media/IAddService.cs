using Deevenue.Domain.Thumbnails;

namespace Deevenue.Domain.Media;

internal interface IAddService
{
    Task<PreProcessResult> PreProcessAsync(Stream stream);
    Task PostProcessAsync(Guid mediumId);
}

internal record PreProcessResult(MediumDimensions? Dimensions, IReadOnlySet<CreatedThumbnail> Thumbnails);

