using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class WindSpatialProjectionTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(64, true)]
    [InlineData(128, true)]
    [InlineData(192, true)]
    [InlineData(255, true)]
    [InlineData(256, false)]
    [InlineData(257, false)]
    public void SouthernRoadRangeHasExclusiveWorldEdge(float distance, bool expected) =>
        Assert.Equal(expected, WindSpatialProjection.Admitted(
            WindSpatialProjection.EmitterPosition + new Vector2(distance, 0), true, true));

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
        Assert.Equal(expected, WindSpatialProjection.Exterior(new(x, y), true));

    [Fact]
    public void MissingSupportOrSourceCannotAdmitEvenAtEmitter()
    {
        Assert.False(WindSpatialProjection.Admitted(WindSpatialProjection.EmitterPosition, false, true));
        Assert.False(WindSpatialProjection.Admitted(WindSpatialProjection.EmitterPosition, true, false));
        Assert.False(WindSpatialProjection.Admitted(new(float.PositiveInfinity, 0), true, true));
    }

    [Theory]
    [InlineData(1280, 720, "Wind blowing")]
    [InlineData(1281, 360, "Wind blowing right")]
    [InlineData(640, -1, "Wind blowing above")]
    [InlineData(-1, 360, "Wind blowing left")]
    [InlineData(640, 721, "Wind blowing below")]
    [InlineData(1281, -1, "Wind blowing above/right")]
    [InlineData(1281, 721, "Wind blowing below/right")]
    [InlineData(-1, 721, "Wind blowing below/left")]
    [InlineData(-1, -1, "Wind blowing above/left")]
    public void CaptionsUseAnonymousScreenBearingsAndInclusiveViewport(float x, float y, string expected) =>
        Assert.Equal(expected, WindSpatialProjection.Caption(new(x, y), new(1280, 720)));
}
