using System.Numerics;
using Allrpcs.Application.Actions;

namespace Allrpcs.Application.Tests.Actions;

public sealed class HandToolTapAccessPolicyTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(32, true)]
    [InlineData(32.001f, false)]
    public void InclusiveReachSamplesBothEndpointsWithoutChangingSupport(float distance, bool expected)
    {
        List<Vector2> visited = []; Vector2 target = new(1700, 900); Vector2 actor = target - new Vector2(distance, 0);
        bool result = HandToolTapAccessPolicy.CanReach(actor, target, p => { visited.Add(p); return true; });
        Assert.Equal(expected, result);
        if (!expected) { Assert.Empty(visited); return; }
        Assert.Equal(actor, visited[0]); Assert.Equal(target, visited[^1]);
        for (int i = 1; i < visited.Count; i++) { Assert.True(Vector2.Distance(visited[i - 1], visited[i]) <= 4); }
    }
    [Fact]
    public void RejectsUnsupportedEndpointOrInteriorAndNonfiniteInputs()
    {
        Vector2 actor = new(1668, 900), target = new(1700, 900);
        Assert.False(HandToolTapAccessPolicy.CanReach(actor, target, p => p != actor));
        Assert.False(HandToolTapAccessPolicy.CanReach(actor, target, p => p != target));
        Assert.False(HandToolTapAccessPolicy.CanReach(actor, target, p => p.X != 1684));
        Assert.False(HandToolTapAccessPolicy.CanReach(new(float.NaN, 0), target, _ => true));
        Assert.False(HandToolTapAccessPolicy.CanReach(actor, new(float.PositiveInfinity, 0), _ => true));
        Assert.Throws<ArgumentNullException>(() => HandToolTapAccessPolicy.CanReach(actor, target, null!));
    }
    [Fact]
    public void DiagonalSegmentUsesBoundedSpacingAndChecksInterior()
    {
        Vector2 target = new(1700, 900), actor = target - new Vector2(20, 20);
        List<Vector2> visited = [];
        Assert.True(HandToolTapAccessPolicy.CanReach(actor, target, p => { visited.Add(p); return true; }));
        Assert.True(visited.Count > 2);
        for (int i = 1; i < visited.Count; i++) { Assert.True(Vector2.Distance(visited[i - 1], visited[i]) <= 4); }
        Assert.False(HandToolTapAccessPolicy.CanReach(actor, target, p => p != visited[visited.Count / 2]));
    }
}
