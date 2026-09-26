using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionWoodlandClearingSceneBindingTests
{
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds ClearingBounds =
        new(240.0f, 24.0f, 536.0f, 120.0f);
    private static readonly Vector2[] Positions =
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

    [Fact]
    public void BindsExactClearingAnchorToBoundedArea()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor clearing = projection.Anchors[6];

        OpeningRegionWoodlandClearingSceneBinding binding = new(
            projection,
            clearing,
            ClearingBounds);

        Assert.Same(projection, binding.Projection);
        Assert.Same(clearing, binding.WoodlandClearingAnchor);
        Assert.Equal(ClearingBounds, binding.WoodlandClearingBounds);
    }

    [Fact]
    public void RejectsMissingRebuiltForeignAndWrongAnchors()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor clearing = projection.Anchors[6];
        OpeningRegionAnchor rebuilt = new(clearing.Location, clearing.Position);
        OpeningRegionProjection foreign = Projection();

        Assert.Throws<ArgumentNullException>(() => new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            null!,
            ClearingBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            rebuilt,
            ClearingBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            foreign.Anchors[6],
            ClearingBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            projection.Anchors[7],
            ClearingBounds));
    }

    [Fact]
    public void RejectsMalformedOutOfBoundsAndInsufficientClearanceAreas()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor clearing = projection.Anchors[6];

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new OpeningRegionWoodlandClearingSceneBinding(
                projection,
                clearing,
                new NavigationWorldBounds(240.0f, 24.0f, 1601.0f, 120.0f)));
        Assert.Throws<ArgumentException>(() => new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            clearing,
            new NavigationWorldBounds(450.0f, 24.0f, 536.0f, 120.0f)));
    }

    private static OpeningRegionProjection Projection()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        return new OpeningRegionProjection(topology, WorldBounds, 24.0f, 120.0f, anchors);
    }
}
