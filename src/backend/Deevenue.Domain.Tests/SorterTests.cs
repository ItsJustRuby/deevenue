using Deevenue.Domain.Search;
using FluentAssertions;

namespace Deevenue.Domain.Tests;


public class SorterTests
{
    [Theory]
    [InlineData("sort:width")]
    [InlineData("sort:height")]
    [InlineData("sort:portrait")]
    [InlineData("sort:landscape")]
    [InlineData("sort:age_desc")]
    [InlineData("sort:filesize")]
    public void TryParse_Succeeds_OnGoodTerms(string term)
    {
        var result = Sorters.TryParse(term);

        result.Should().NotBeNull();
    }
}
