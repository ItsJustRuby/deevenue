using System.Text.Json;

namespace Deevenue.Cli;

internal static class JsonSerializerSettings
{
    public static readonly JsonSerializerOptions Default = new()
    {
        PropertyNameCaseInsensitive = true
    };
}
