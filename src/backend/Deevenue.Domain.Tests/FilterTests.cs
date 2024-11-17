using Deevenue.Domain.Rules;
using Deevenue.Domain.Search;
using FluentAssertions;
using Moq;

namespace Deevenue.Domain.Tests;

public class FilterTests
{
    [Theory]
    [InlineData("aspectratio>2")]
    [InlineData("width<800")]
    [InlineData("filesize>2MB")]
    [InlineData("age>1y")]
    [InlineData("rating:s")]
    [InlineData("artisttags>0")]
    [InlineData("ctags>0")]
    [InlineData("tags=0")]
    [InlineData("rule:3")]
    [InlineData("+foo")]
    [InlineData("foo")]
    [InlineData("artist:foo")]
    public void TryParse_Succeeds_OnGoodTerms(string term)
    {
        var result = Filters.TryParse(term);
        result.Should().NotBeNull();
    }

    [Theory]
    [InlineData("rating:s", true)]
    [InlineData("rating:q", false)]
    [InlineData("tag", true)]
    [InlineData("otherTag", false)]
    [InlineData("tagAlias", true)]
    [InlineData("impliedTag", true)]
    [InlineData("+tag", true)]
    [InlineData("+otherTag", false)]
    [InlineData("+impliedTag", false)]
    [InlineData("stags>1", false)]
    [InlineData("stags:1", true)]
    [InlineData("stags:0", false)]
    [InlineData("filesize>1mb", true)]
    [InlineData("filesize<5M", true)]
    [InlineData("filesize<1GB", true)]
    [InlineData("age>1d", true)]
    [InlineData("age<1w", true)]
    [InlineData("age>1y", false)]
    [InlineData("aspectratio>1", true)]
    [InlineData("width>1000", true)]
    [InlineData("width=1000", false)]
    [InlineData("height<800", true)]
    [InlineData("height>=800", false)]
    [InlineData("rule:1234", false)]
    public void Rejects_IsImplementedCorrectlyForAllFilters(string term, bool accepts)
    {
        var filter = Filters.TryParse(term);
        filter.Should().NotBeNull();

        var mockRulesService = new Mock<IRulesService>();
        mockRulesService.Setup(s => s.Get(It.IsAny<int>())).Returns(() => null);
        var mockContext = new Mock<IFilterContext>();
        mockContext.Setup(m => m.RulesService).Returns(mockRulesService.Object);

        filter!.Rejects(imageDocument, mockContext.Object).Should().Be(!accepts);
    }

    private readonly SmallMediumDocument imageDocument = new(
        Id: Guid.NewGuid(),
        "image/png",
        InnateTags: new HashSet<string>(["tag", "tagAlias", "s:foo"]),
        SearchableTags: new HashSet<string>(["tag", "tagAlias", "s:foo", "impliedTag"]),
        AbsentTags: new HashSet<string>(["absentTag", "absentTagAlias"]),
        Rating.Safe,
        FilesizeInBytes: 2 * 1024 * 1024,
        InsertDate: DateTime.UtcNow.AddDays(-1).AddHours(-12),
        Width: 1024,
        Height: 768
    );
}
