using Allrpcs.Application.Presentation;

namespace Allrpcs.Application.Tests;

public sealed class DepthDrawOrderTests
{
    [Fact]
    public void RanksFurtherSouthFeetInFront()
    {
        // Sage and cask at their authored Y (cask north of the Sage); Healer south of the Sage, Seeker
        // north of the cask.
        int[] ranks = DepthDrawOrder.Ranks([950.0f, 900.0f, 1010.0f, 880.0f]);

        Assert.Equal([2, 1, 3, 0], ranks);
    }

    [Fact]
    public void BreaksTiesByListOrder()
    {
        int[] ranks = DepthDrawOrder.Ranks([950.0f, 950.0f, 900.0f]);

        Assert.Equal([1, 2, 0], ranks);
    }

    [Fact]
    public void ReturnsNoRanksForNoItems()
    {
        Assert.Empty(DepthDrawOrder.Ranks([]));
    }

    [Fact]
    public void CountsOnlyItemsStrictlyNorthOfAFixedProp()
    {
        // Stone marker foot at 368: the Seeker at 345 is north (behind it); the Healer level with it
        // and the Sage and cask south of it are not.
        Assert.Equal(1, DepthDrawOrder.CountNorthOf([950.0f, 900.0f, 368.0f, 345.0f], 368.0f));
    }

    [Fact]
    public void CountsNothingNorthOfAPropWithNoItems()
    {
        Assert.Equal(0, DepthDrawOrder.CountNorthOf([], 368.0f));
    }
}
