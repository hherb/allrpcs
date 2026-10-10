using System.Numerics;
using Allrpcs.Application.Actions;

namespace Allrpcs.Application.Tests.Actions;

public sealed class CaskCoverHandlingAccessPolicyTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(32, true)]
    [InlineData(32.001f, false)]
    public void InclusiveReachSamplesBothEndpointsWithoutChangingSupport(float distance, bool expected)
    {
        List<Vector2> visited = []; Vector2 target = new(2200, 900); Vector2 actor = target - new Vector2(distance, 0);
        bool result = CaskCoverHandlingAccessPolicy.CanReach(actor, target, p => { visited.Add(p); return true; });
        Assert.Equal(expected, result);
        if (!expected) { Assert.Empty(visited); return; }
        Assert.Equal(actor, visited[0]); Assert.Equal(target, visited[^1]);
        for (int i = 1; i < visited.Count; i++) { Assert.True(Vector2.Distance(visited[i - 1], visited[i]) <= 4); }
    }
    [Fact]
    public void RejectsUnsupportedEndpointOrInteriorAndNonfiniteInputs()
    {
        Vector2 actor = new(2168, 900), target = new(2200, 900);
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(actor, target, p => p != actor));
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(actor, target, p => p != target));
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(actor, target, p => p.X != 2184));
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(new(float.NaN, 0), target, _ => true));
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(actor, new(float.PositiveInfinity, 0), _ => true));
        Assert.Throws<ArgumentNullException>(() => CaskCoverHandlingAccessPolicy.CanReach(actor, target, null!));
    }
    [Fact]
    public void UsesActualMovedTargetRatherThanInitialPlacement()
    {
        Assert.True(CaskCoverHandlingAccessPolicy.CanReach(new(1868, 1640), new(1900, 1640), _ => true));
        Assert.False(CaskCoverHandlingAccessPolicy.CanReach(new(2168, 900), new(1900, 1640), _ => true));
    }
}
