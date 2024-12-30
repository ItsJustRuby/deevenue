using System.Text.Json;

namespace Deevenue.Domain;

public static class JsonSerialization
{
    public static readonly JsonSerializerOptions DefaultOptions = new()
    {
        AllowOutOfOrderMetadataProperties = true,
        PropertyNameCaseInsensitive = true,
    };
}
