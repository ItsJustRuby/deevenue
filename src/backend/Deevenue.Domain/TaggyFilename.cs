using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;

namespace Deevenue.Domain;

[DebuggerDisplay("{ToString()}")]
internal class TaggyParseResults
{
    public static readonly TaggyParseResults Empty = new()
    {
        Tags = new HashSet<string>(),
        Rating = Rating.Unknown
    };

    public required IReadOnlySet<string> Tags { get; init; }
    public Rating Rating { get; init; }

    public override string? ToString()
    {
        var stringBuilder = new StringBuilder();
        if (this == Empty)
            return "Empty";

        stringBuilder.Append($"rating:{Rating} ");

        stringBuilder.Append(string.Join(" ", Tags));

        return stringBuilder.ToString();
    }
}

internal interface ITaggyFileName
{
    TaggyParseResults Parse(string filename);
}

internal partial class TaggyFileName(ILogger<TaggyFileName> logger) : ITaggyFileName
{
    [GeneratedRegex(@"^\d+ - (.*)\.([a-zA-Z0-9]+)$")]
    private static partial Regex TaggyFileNameRegex();


    [GeneratedRegex(@"rating:(u|q|s|e)")]
    private static partial Regex RatingTagRegex();

    public TaggyParseResults Parse(string filename)
    {
        var potentialMatch = TaggyFileNameRegex().Match(filename);

        if (!potentialMatch.Success)
        {
            logger.LogDebug("File name not taggy: \"{filename}\"", filename);
            return TaggyParseResults.Empty;
        }

        var entireTagsString = potentialMatch.Groups[1].Value;
        logger.LogDebug("Parsing tagString \"{entireTagsString}\"", entireTagsString);
        var tagArray = entireTagsString.Replace("_", ":").Split(" ");
        logger.LogDebug("as Array: \"{a}\"", string.Join(", ", tagArray));

        string? parsedRating = null;
        var parsedTags = new HashSet<string>();

        var ratingTagRegex = RatingTagRegex();

        foreach (var tagString in tagArray)
        {
            var ratingMatch = ratingTagRegex.Match(tagString);
            if (ratingMatch.Success)
                parsedRating = ratingMatch.Groups[1].Value;
            else
                parsedTags.Add(tagString);
        }

        return new TaggyParseResults
        {
            Tags = parsedTags,
            Rating = parsedRating != null ? Ratings.ToRating[parsedRating[0]] : Rating.Unknown
        };
    }
}
