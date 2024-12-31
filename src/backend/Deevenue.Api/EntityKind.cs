using System.Text.Json.Serialization;
using Deevenue.Domain;

namespace Deevenue.Api;

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum EntityKind
{
    Medium
}
