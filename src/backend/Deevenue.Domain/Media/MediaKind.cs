using System.Text.Json.Serialization;

namespace Deevenue.Domain.Media;

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum MediaKind
{
    Unusable,
    Image,
    Video
}

public static class MediaKinds
{
    public static MediaKind Parse(string contentType)
    {
        if (contentType == null)
            return MediaKind.Unusable;
        if (contentType.StartsWith("image/") && contentType != "image/gif")
            return MediaKind.Image;
        if (contentType.StartsWith("video/") || contentType == "image/gif")
            return MediaKind.Video;
        return MediaKind.Unusable;
    }
}
