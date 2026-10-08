using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class OpeningRegionInteriorAudioProjectionTests
{
    private static InteriorAudioLayout Layout() => new(new(2528, 1310), 64,
        new(1420, 240.8f), new(1580, 294.66666f), 96);

    [Theory]
    [InlineData(2640, 1450, InteriorAudioRoom.Sickhouse, FootstepMaterial.Timber)]
    [InlineData(1500, 220, InteriorAudioRoom.Laundry, FootstepMaterial.Earth)]
    [InlineData(2528, 1310, InteriorAudioRoom.Sickhouse, FootstepMaterial.Timber)]
    [InlineData(1420, 240.8, InteriorAudioRoom.Laundry, FootstepMaterial.Earth)]
    [InlineData(1580, 294.66666, InteriorAudioRoom.Laundry, FootstepMaterial.Earth)]
    public void SupportedFloorsAndThresholdsOwnOnlyTheirProposedMaterial(float x, float y,
        InteriorAudioRoom room, FootstepMaterial material)
    {
        InteriorAudioSnapshot result = OpeningRegionInteriorAudioProjection.Select(Layout(), new(x, y), true);
        Assert.Equal(room, result.Room);
        Assert.Equal(material, result.Material);
    }

    [Theory]
    [InlineData(2480, 1400)]
    [InlineData(2488, 1400)]
    [InlineData(2792, 1400)]
    [InlineData(2640, 1494)]
    [InlineData(2496, 1310)]
    [InlineData(2560, 1310)]
    [InlineData(1428, 140)]
    [InlineData(1500, 128)]
    [InlineData(1500, 352)]
    [InlineData(1580, 360)]
    public void WallsAndConservativeInsetEdgesRemainSilent(float x, float y)
    {
        Assert.Equal(InteriorAudioRoom.None, OpeningRegionInteriorAudioProjection.Select(Layout(), new(x, y), true).Room);
        Assert.Null(OpeningRegionInteriorAudioProjection.MaterialAt(Layout(), new(x, y), true));
    }

    [Fact]
    public void PortalWidthsAreOpenAndDepthIsClosed()
    {
        InteriorAudioLayout layout = Layout();
        foreach (Vector2 portal in new[] { layout.LaundryWest, layout.LaundryEast })
        {
            Assert.Null(OpeningRegionInteriorAudioProjection.MaterialAt(layout, portal + new Vector2(0, 48), true));
            Assert.NotNull(OpeningRegionInteriorAudioProjection.MaterialAt(layout, portal + new Vector2(0, 47.99f), true));
        }
        Assert.Equal(FootstepMaterial.Timber, OpeningRegionInteriorAudioProjection.MaterialAt(layout, new(2528, 1318), true));
        Assert.Equal(FootstepMaterial.Timber, OpeningRegionInteriorAudioProjection.MaterialAt(layout, new(2488.01f, 1400), true));
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(16, 0.5)]
    [InlineData(32, 1)]
    [InlineData(64, 1)]
    public void DistanceToOpeningControlsBlendWithoutWallLeakage(float depth, double blend)
    {
        InteriorAudioSnapshot result = OpeningRegionInteriorAudioProjection.Select(Layout(), new(2528, 1310 + depth), true);
        Assert.Equal(blend, result.Blend, 6);
        Assert.Equal(1, result.Gain(0));
        Assert.Equal(Math.Pow(10, -9 * blend / 20), result.Gain(1), 8);
    }

    [Fact]
    public void LaundryUsesEitherLiveOpeningAndItsOwnProfile()
    {
        InteriorAudioLayout layout = Layout();
        Assert.Equal(0.5, OpeningRegionInteriorAudioProjection.Select(layout, layout.LaundryWest + new Vector2(16, 0), true).Blend, 6);
        Assert.Equal(0.5, OpeningRegionInteriorAudioProjection.Select(layout, layout.LaundryEast - new Vector2(16, 0), true).Blend, 6);
        Assert.Equal(Math.Pow(10, -3.0 / 20), OpeningRegionInteriorAudioProjection.Select(layout, new(1500, 220), true).Gain(2), 8);
    }

    [Theory]
    [InlineData(500, 900)]
    [InlineData(2528, 1309.99)]
    [InlineData(1419.99, 220)]
    [InlineData(1580.01, 300)]
    [InlineData(1800, 500)]
    public void ExteriorDelegatesExactlyToRetainedProjection(float x, float y)
    {
        Assert.Equal(OpeningRegionFootstepMaterialProjection.Select(new(x, y), true),
            OpeningRegionInteriorAudioProjection.MaterialAt(Layout(), new(x, y), true));
        Assert.Equal(InteriorAudioRoom.None, OpeningRegionInteriorAudioProjection.Select(Layout(), new(x, y), true).Room);
    }

    [Fact]
    public void InvalidOrUnsupportedInputGrantsNoInteriorMaterial()
    {
        foreach (Vector2 position in new[] { new Vector2(float.NaN, 1450), new Vector2(1500, float.PositiveInfinity), new Vector2(-1, 0) })
        {
            Assert.Equal(InteriorAudioRoom.None, OpeningRegionInteriorAudioProjection.Select(Layout(), position, true).Room);
            Assert.Null(OpeningRegionInteriorAudioProjection.MaterialAt(Layout(), position, true));
        }
        Assert.Null(OpeningRegionInteriorAudioProjection.MaterialAt(Layout(), new(2640, 1450), false));
        Assert.Null(OpeningRegionInteriorAudioProjection.MaterialAt(null, new(2640, 1450), true));
        Assert.Equal(FootstepMaterial.Earth, OpeningRegionInteriorAudioProjection.MaterialAt(null, new(500, 900), true));
    }

    [Fact]
    public void DescriptorRejectsForeignPortalsRatherThanUsingAnUnusedHeightConstant()
    {
        Assert.Throws<ArgumentException>(() => new InteriorAudioLayout(new(2528, 1310), 64, new(1420, 220), new(1580, 220), 96));
        Assert.Throws<ArgumentException>(() => new InteriorAudioLayout(new(2529, 1310), 64, new(1420, 240.8f), new(1580, 294.66666f), 96));
        Assert.Throws<ArgumentException>(() => new InteriorAudioLayout(new(2528, 1310), 65, new(1420, 240.8f), new(1580, 294.66666f), 96));
    }
}
