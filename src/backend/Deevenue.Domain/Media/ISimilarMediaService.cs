namespace Deevenue.Domain.Media;

public interface ISimilarMediaService
{
    Task<IReadOnlyList<SimilarMedium>> GetSimilarAsync(Guid mediumId);
}

public record SimilarMedium(Guid Id, MediaKind MediaKind);
