using Deevenue.Infrastructure.Jobs;
using FluentAssertions;

namespace Deevenue.Infrastructure.Tests;

public class StrideTests
{
    [Theory]
    [InlineData(12, 5)]
    [InlineData(200, 5)]
    [InlineData(5, 5)]
    public void AlwaysReturnsExactlyCountToPick(int totalCount, int countToPick)
    {
        var results = Stride.GetEvenlySpacedIndices(totalCount, countToPick);
        results.Should().HaveCount(countToPick);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ThrowsIfCountToPickTooSmall(int countToPick)
    {
        var act = () => Stride.GetEvenlySpacedIndices(10, countToPick);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Theory]
    [InlineData(3, 5)]
    public void ThrowsIfTotalCountTooSmall(int totalCount, int countToPick)
    {
        var act = () => Stride.GetEvenlySpacedIndices(totalCount, countToPick);
        act.Should().Throw<ArgumentOutOfRangeException>();
    }
}
