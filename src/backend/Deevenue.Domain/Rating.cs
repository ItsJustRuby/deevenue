using System.Text.Json.Serialization;

namespace Deevenue.Domain;

[JsonConverter(typeof(LowercaseJsonStringEnumConverter))]
public enum Rating
{
    Explicit,
    Questionable,
    Safe,
    Unknown
}

public static class Ratings
{
    public static readonly IReadOnlyDictionary<Rating, char> ToChar = new Dictionary<Rating, char>
    {
        [Rating.Explicit] = 'e',
        [Rating.Questionable] = 'q',
        [Rating.Safe] = 's',
        [Rating.Unknown] = 'u',
    };

    public static readonly IReadOnlyDictionary<char, Rating> ToRating = new Dictionary<char, Rating>
    {
        ['e'] = Rating.Explicit,
        ['q'] = Rating.Questionable,
        ['s'] = Rating.Safe,
        ['u'] = Rating.Unknown,
    };
}
