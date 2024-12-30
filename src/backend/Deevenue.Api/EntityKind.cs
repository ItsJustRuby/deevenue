using System.Text.Json.Serialization;
using Deevenue.Domain;

namespace Deevenue.Api;

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum EntityKind
{
    // TODO: OK this enum still only has one value. Either use it way more or way less.
    Medium
}
