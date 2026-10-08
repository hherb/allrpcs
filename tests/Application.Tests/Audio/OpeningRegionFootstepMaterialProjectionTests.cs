using System.Numerics;
using Allrpcs.Application.Audio;

namespace Allrpcs.Application.Tests.Audio;

public sealed class OpeningRegionFootstepMaterialProjectionTests
{
    [Theory]
    [InlineData(2200, 900, FootstepMaterial.Earth)]
    [InlineData(1800, 500, FootstepMaterial.Timber)]
    [InlineData(2528, 1250, FootstepMaterial.Gravel)]
    [InlineData(690, 325, FootstepMaterial.Leaves)]
    [InlineData(825, 250, FootstepMaterial.Leaves)]
    public void ExplicitRulesOverrideEarthOnlyOnSupportedGround(float x, float y, FootstepMaterial expected)
    {
        Assert.Equal(expected, OpeningRegionFootstepMaterialProjection.Select(new(x, y), true));
        Assert.Null(OpeningRegionFootstepMaterialProjection.Select(new(x, y), false));
    }

    [Theory]
    [InlineData(2640, 1450)]
    [InlineData(1500, 220)]
    [InlineData(2528, 1310)]
    [InlineData(1420, 120)]
    [InlineData(-1, 100)]
    [InlineData(3201, 100)]
    public void UnmappedInteriorAndInvalidCoordinatesNeverFallBackToEarth(float x, float y)
    {
        Assert.Null(OpeningRegionFootstepMaterialProjection.Select(new(x, y), true));
    }

    [Fact]
    public void NonfiniteInputAndUnknownMaterialAreSilent()
    {
        Assert.Null(OpeningRegionFootstepMaterialProjection.Select(new(float.NaN, 10), true));
        Assert.Null(OpeningRegionFootstepMaterialProjection.Select(new(10, float.PositiveInfinity), true));
        Assert.Null(OpeningRegionFootstepMaterialProjection.ClipId((FootstepMaterial)99));
    }

    [Fact]
    public void RectangleAndCapsuleEdgesHaveDeterministicInclusiveMembership()
    {
        Assert.Equal(FootstepMaterial.Timber, OpeningRegionFootstepMaterialProjection.Select(new(1736, 452), true));
        Assert.Equal(FootstepMaterial.Earth, OpeningRegionFootstepMaterialProjection.Select(new(1735.9f, 452), true));
        Assert.Equal(FootstepMaterial.Gravel, OpeningRegionFootstepMaterialProjection.Select(new(2480, 1214), true));
        Assert.Equal(FootstepMaterial.Leaves, OpeningRegionFootstepMaterialProjection.Select(new(356, 300), true));
        Assert.Equal(FootstepMaterial.Earth, OpeningRegionFootstepMaterialProjection.Select(new(355.9f, 300), true));
    }
}
