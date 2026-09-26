using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionOldShrineSceneBindingTests
{
    private static readonly NavigationWorldBounds ShrineBounds =
        new(24.0f, 24.0f, 216.0f, 136.0f);

    [Fact]
    public void BindsOnlyExactShrineAnchorWithClearance()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor shrine = projection.Anchors[7];

        OpeningRegionOldShrineSceneBinding binding = new(projection, shrine, ShrineBounds);

        Assert.Same(projection, binding.Projection);
        Assert.Same(shrine, binding.OldShrineAnchor);
        Assert.Equal(ShrineBounds, binding.OldShrineBounds);
        Assert.Throws<ArgumentException>(() => new OpeningRegionOldShrineSceneBinding(
            projection,
            new OpeningRegionAnchor(shrine.Location, shrine.Position),
            ShrineBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionOldShrineSceneBinding(
            projection,
            projection.Anchors[6],
            ShrineBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionOldShrineSceneBinding(
            projection,
            shrine,
            new NavigationWorldBounds(150.0f, 24.0f, 216.0f, 136.0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionOldShrineSceneBinding(
            projection,
            shrine,
            new NavigationWorldBounds(-1.0f, 24.0f, 216.0f, 136.0f)));
    }

    internal static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        Vector2[] positions =
        [
            new(920.0f, 820.0f),
            new(880.0f, 620.0f),
            new(880.0f, 400.0f),
            new(650.0f, 400.0f),
            new(808.0f, 260.0f),
            new(700.0f, 60.0f),
            new(470.0f, 70.0f),
            new(160.0f, 70.0f),
            new(620.0f, 620.0f),
            new(720.0f, 820.0f),
            new(120.0f, 620.0f),
        ];
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, positions[index]))
            .ToArray();
        return new OpeningRegionProjection(
            topology,
            new NavigationWorldBounds(0.0f, 0.0f, 1600.0f, 900.0f),
            24.0f,
            120.0f,
            anchors);
    }
}
