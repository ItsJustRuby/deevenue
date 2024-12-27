namespace Deevenue.Domain.Media;

internal interface ISimilarMediaService
{
    Task<IReadOnlyList<SimilarMedium>> GetSimilarAsync(Guid mediumId);
}

public record SimilarMedium(Guid Id, MediaKind MediaKind);
