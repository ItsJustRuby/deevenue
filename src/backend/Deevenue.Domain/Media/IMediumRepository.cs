namespace Deevenue.Domain.Media;

public interface IMediumRepository
{
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Guid?> FindByHashAsync(string hash);
    Task<IReadOnlySet<SmallMediumDocument>> GetAllSearchableAsync(bool isSfw);
    Task<SmallMediumDocument?> GetSearchableAsync(Guid id, bool isSfw);
    Task<PaginateAllResult> PaginateAllAsync(PaginationParameters pagination, bool isSfw);
    Task<Guid> PutAsync(PutAsyncArgs args);
    Task UpdateContentMetadataAsync(Guid id, string contentType, string hash);
    Task<bool> SetRatingAsync(Guid id, Rating rating);
    Task<MediumDataSet?> TryGetAsync(Guid id);
    Task UpdateDimensionsAsync(Guid id, MediumDimensions dimensions);
}

public record MediumDataSet(
    Guid Id,
    string ContentType,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> AbsentTags,
    IReadOnlyList<Guid> ThumbnailSheetIds,
    Rating Rating);

public record PaginateAllResult(int PageCount, IReadOnlyList<PaginateAllDataSet> Page);

public record PaginateAllDataSet(Guid Id, string ContentType);

public class PutAsyncArgs
{
    public required string ContentType { get; init; }
    public required string Hash { get; init; }
    public required Rating Rating { get; init; }
    public required Stream ReadStream { get; init; }
    public required long Size { get; init; }
    public required IReadOnlySet<string> TagNames { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
}
