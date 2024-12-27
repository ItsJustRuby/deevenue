using System.Text.Json.Serialization;

namespace Deevenue.Domain.Thumbnails;

public static class ThumbnailSizes
{
    public static readonly IReadOnlySet<IThumbnailSize> All = new HashSet<IThumbnailSize>([
        new ThumbnailSize(ThumbnailSizeAbbreviation.S, 240),
        new ThumbnailSize(ThumbnailSizeAbbreviation.L, 600)
    ]);

    private static readonly Dictionary<ThumbnailSizeAbbreviation, IThumbnailSize> byAbbreviation
        = All.ToDictionary(ts => ts.Abbreviation);

    public static IThumbnailSize ByAbbreviation(ThumbnailSizeAbbreviation abbreviation)
        => byAbbreviation[abbreviation];

    public static bool IsKnown(ThumbnailSizeAbbreviation abbreviation)
        => byAbbreviation.ContainsKey(abbreviation);

    private record ThumbnailSize(ThumbnailSizeAbbreviation Abbreviation, int PixelCount) : IThumbnailSize;
}

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum ThumbnailSizeAbbreviation
{
    S,
    L
}

public interface IThumbnailSize
{
    public ThumbnailSizeAbbreviation Abbreviation { get; }
    public int PixelCount { get; }
}
