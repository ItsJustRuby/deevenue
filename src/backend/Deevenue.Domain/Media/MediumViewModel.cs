namespace Deevenue.Domain.Media;

public record MediumViewModel(
    Guid Id,
    MediaKind MediaKind,
    IReadOnlyList<string> Tags,
    IReadOnlyList<string> AbsentTags,
    IReadOnlyList<Guid> ThumbnailSheetIds,
    Rating Rating);
