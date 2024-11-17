using System.Text.Json;
using System.Text.Json.Serialization;

namespace Deevenue.Domain;

// Note: Ideally, you would move this to .Api, but while ASP .Net Core can handle that,
// the OpenAPI schema generator struggles to understand it.
// Leaving it here is not 100% clean, but way more pragmatic.
public class LowercaseJsonStringEnumConverter : JsonStringEnumConverter
{
    public LowercaseJsonStringEnumConverter() : base(JsonNamingPolicy.SnakeCaseLower, allowIntegerValues: false)
    { }
}
