using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Deevenue.Domain.Tests;

public class TaggyFileNameTests
{
    private readonly TaggyFileName taggy;
    private TaggyParseResults? parseResults;

    public TaggyFileNameTests()
    {
        // Just do whatever, IDC
        var mockLogger = new Mock<ILogger<TaggyFileName>>(MockBehavior.Loose);
        taggy = new TaggyFileName(mockLogger.Object);
    }

    [Theory]
    [InlineData("1 - example.jpg")]
    public void Parse_Succeeds_OnTaggyFileNames(string filename)
    {
        WhenParsing(filename);

        ThenResults().Should().NotBeNull().And.NotBe(TaggyParseResults.Empty);

        ThenResults().Rating.Should().Be(Rating.Unknown);
        ThenResults().Tags.Should().Equal(["example"]);
    }

    [Theory]
    [InlineData("1 - rating:s.jpg")]
    public void Parse_Succeeds_OnFileNamesWithRating(string filename)
    {
        WhenParsing(filename);

        ThenResults().Should().NotBeNull().And.NotBe(TaggyParseResults.Empty);

        ThenResults().Rating.Should().Be(Rating.Safe);
        ThenResults().Tags.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("foobar.jpg")]
    public void Parse_SucceedsAndReturnsEmpty_OnNonTaggyFileNames(string filename)
    {
        WhenParsing(filename);
        ThenResults().Should().NotBeNull().And.Be(TaggyParseResults.Empty);
    }

    private void WhenParsing(string filename)
    {
        parseResults = taggy.Parse(filename);
    }

    private TaggyParseResults ThenResults() => parseResults!;
}
