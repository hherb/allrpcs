using Allrpcs.Application.Presentation;

namespace Allrpcs.Application.Tests;

public sealed class DepthDrawOrderTests
{
    [Fact]
    public void RanksFurtherSouthFeetInFront()
    {
        // Sage, cask, Healer, Seeker at the village square start positions (cask north of the Sage).
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
}
