namespace Deevenue.Domain;

public sealed record SmallMediumDocument(
    Guid Id,
    string ContentType,
    IReadOnlySet<string> InnateTags,
    IReadOnlySet<string> SearchableTags,
    IReadOnlySet<string> AbsentTags,
    Rating Rating,
    decimal FilesizeInBytes,
    DateTime InsertDate,
    int Width,
    int Height
);
