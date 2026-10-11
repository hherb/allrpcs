using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class FoliageSpatialProjectionTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(48, true)]
    [InlineData(96, true)]
    [InlineData(144, true)]
    [InlineData(191, true)]
    [InlineData(192, false)]
    [InlineData(193, false)]
    public void GardenRangeHasExclusiveWorldEdge(float distance, bool expected) =>
        Assert.Equal(expected, FoliageSpatialProjection.Admitted(
            FoliageSpatialProjection.EmitterPosition + new Vector2(distance, 0), true, true));

    [Theory]
    [InlineData(2480, 1310, false)]
    [InlineData(2800, 1502, false)]
    [InlineData(1420, 120, false)]
    [InlineData(1580, 360, false)]
    [InlineData(1580.1, 360, true)]
    [InlineData(0, 0, true)]
    [InlineData(3200, 1800, true)]
    [InlineData(-0.1, 0, false)]
    [InlineData(3200.1, 1800, false)]
    [InlineData(0, float.NaN, false)]
    public void ListenerSupportUsesFiniteRetainedBoundsAndInclusiveRoomExclusions(float x, float y, bool expected) =>
        Assert.Equal(expected, FoliageSpatialProjection.Exterior(new(x, y), true));

    [Fact]
    public void MissingSupportOrSourceCannotAdmitEvenAtEmitter()
    {
        Assert.False(FoliageSpatialProjection.Admitted(FoliageSpatialProjection.EmitterPosition, false, true));
        Assert.False(FoliageSpatialProjection.Admitted(FoliageSpatialProjection.EmitterPosition, true, false));
        Assert.False(FoliageSpatialProjection.Admitted(new(float.PositiveInfinity, 0), true, true));
    }

    [Theory]
    [InlineData(1280, 720, "Leaves rustling")]
    [InlineData(1281, 360, "Leaves rustling right")]
    [InlineData(640, -1, "Leaves rustling above")]
    [InlineData(-1, 360, "Leaves rustling left")]
    [InlineData(640, 721, "Leaves rustling below")]
    public void CaptionsUseAnonymousScreenBearingsAndInclusiveViewport(float x, float y, string expected) =>
        Assert.Equal(expected, FoliageSpatialProjection.Caption(new(x, y), new(1280, 720)));
}
