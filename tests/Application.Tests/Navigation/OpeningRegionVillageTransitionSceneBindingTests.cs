using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionVillageTransitionSceneBindingTests
{
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds SquareBounds =
        new(840.0f, 320.0f, 960.0f, 480.0f);
    private static readonly NavigationWorldBounds EdgeBounds =
        new(570.0f, 320.0f, 730.0f, 480.0f);
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
    public void BindsExactVillageAnchorsToDistinctBoundedAreas()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor square = projection.Anchors[2];
        OpeningRegionAnchor edge = projection.Anchors[3];

        OpeningRegionVillageTransitionSceneBinding binding = new(
            projection,
            square,
            SquareBounds,
            edge,
            EdgeBounds);

        Assert.Same(projection, binding.Projection);
        Assert.Same(square, binding.VillageSquareAnchor);
        Assert.Equal(SquareBounds, binding.VillageSquareBounds);
        Assert.Same(edge, binding.VillageEdgeAnchor);
        Assert.Equal(EdgeBounds, binding.VillageEdgeBounds);
    }

    [Fact]
    public void RejectsMissingRebuiltForeignAndSwappedAnchors()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor square = projection.Anchors[2];
        OpeningRegionAnchor edge = projection.Anchors[3];
        OpeningRegionAnchor rebuiltSquare = new(square.Location, square.Position);
        OpeningRegionProjection foreignProjection = Projection();

        Assert.Throws<ArgumentNullException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            null!,
            SquareBounds,
            edge,
            EdgeBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            rebuiltSquare,
            SquareBounds,
            edge,
            EdgeBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            square,
            SquareBounds,
            foreignProjection.Anchors[3],
            EdgeBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            edge,
            SquareBounds,
            square,
            EdgeBounds));
    }

    [Fact]
    public void RejectsOverlappingAndOutOfBoundsAreas()
    {
        OpeningRegionProjection projection = Projection();
        OpeningRegionAnchor square = projection.Anchors[2];
        OpeningRegionAnchor edge = projection.Anchors[3];

        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            square,
            SquareBounds,
            edge,
            new NavigationWorldBounds(700.0f, 320.0f, 850.0f, 480.0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            square,
            new NavigationWorldBounds(800.0f, 320.0f, 1601.0f, 480.0f),
            edge,
            EdgeBounds));
    }

    [Fact]
    public void RejectsAreasWithoutAnchorClearance()
    {
        OpeningRegionProjection projection = Projection();

        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionSceneBinding(
            projection,
            projection.Anchors[2],
            new NavigationWorldBounds(870.0f, 320.0f, 960.0f, 480.0f),
            projection.Anchors[3],
            EdgeBounds));
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
