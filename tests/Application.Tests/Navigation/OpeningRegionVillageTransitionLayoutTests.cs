using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionVillageTransitionLayoutTests
{
    private const float OpeningWidthPixels = 64.0f;
    private const float WallThicknessPixels = 8.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds SquareBounds =
        new(840.0f, 320.0f, 960.0f, 480.0f);
    private static readonly NavigationWorldBounds EdgeBounds =
        new(570.0f, 320.0f, 730.0f, 480.0f);
    private static readonly Vector2 GardenApproach = new(880.0f, 520.0f);
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
    public void CreatesExplicitGardenSquareEdgeAndOuterOpenings()
    {
        OpeningRegionVillageTransitionSceneBinding binding = Binding();

        OpeningRegionVillageTransitionLayout layout = new(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            GardenApproach);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(OpeningWidthPixels, layout.OpeningWidthPixels);
        Assert.Equal(WallThicknessPixels, layout.WallThicknessPixels);
        Assert.Equal(GardenApproach, layout.GardenApproach);
        Assert.Equal(new Vector2(880.0f, 480.0f), layout.SquareGardenOpening);
        Assert.Equal(new Vector2(840.0f, 400.0f), layout.SquareEdgeOpening);
        Assert.Equal(new Vector2(730.0f, 400.0f), layout.EdgeSquareOpening);
        Assert.Equal(new Vector2(650.0f, 320.0f), layout.EdgeStreamOpening);
        Assert.Equal(new Vector2(570.0f, 400.0f), layout.EdgeWoodlandOpening);
    }

    [Fact]
    public void RejectsNarrowOpeningsInvalidWallsAndMisalignedGardenApproach()
    {
        OpeningRegionVillageTransitionSceneBinding binding = Binding();
        float clearance = binding.Projection.MinimumPathClearancePixels;

        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionVillageTransitionLayout(
            binding,
            (clearance * 2.0f) - 1.0f,
            WallThicknessPixels,
            GardenApproach));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionVillageTransitionLayout(
            binding,
            OpeningWidthPixels,
            float.NaN,
            GardenApproach));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionVillageTransitionLayout(
            binding,
            OpeningWidthPixels,
            clearance * 2.0f,
            GardenApproach));
        Assert.Throws<ArgumentException>(() => new OpeningRegionVillageTransitionLayout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            GardenApproach + Vector2.UnitX));
    }

    [Fact]
    public void RoutePolicyAcceptsGardenSquareEdgeRoundTrip()
    {
        OpeningRegionVillageTransitionLayout layout = Layout();
        Vector2 square = layout.SceneBinding.VillageSquareAnchor.Position;
        Vector2 edge = layout.SceneBinding.VillageEdgeAnchor.Position;

        Assert.True(OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(
            layout,
            [
                GardenApproach,
                layout.SquareGardenOpening,
                square,
                layout.SquareEdgeOpening,
                layout.EdgeSquareOpening,
                edge,
            ]));
        Assert.True(OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(
            layout,
            [
                edge,
                layout.EdgeSquareOpening,
                layout.SquareEdgeOpening,
                square,
                layout.SquareGardenOpening,
                GardenApproach,
            ]));
    }

    [Fact]
    public void RoutePolicyRejectsBoundaryCrossingOutOfBoundsAndIncompleteRoutes()
    {
        OpeningRegionVillageTransitionLayout layout = Layout();

        Assert.False(OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(
            layout,
            [new Vector2(880.0f, 280.0f), layout.SceneBinding.VillageSquareAnchor.Position]));
        Assert.False(OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.VillageEdgeAnchor.Position, new Vector2(-1.0f, 400.0f)]));
        Assert.False(OpeningRegionVillageTransitionRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.VillageSquareAnchor.Position]));
    }

    private static OpeningRegionVillageTransitionLayout Layout() => new(
        Binding(),
        OpeningWidthPixels,
        WallThicknessPixels,
        GardenApproach);

    private static OpeningRegionVillageTransitionSceneBinding Binding()
    {
        OpeningRegionSnapshot topology = RepresentativeOpeningRegionSource.Create();
        OpeningRegionAnchor[] anchors = topology.Locations
            .Select((location, index) => new OpeningRegionAnchor(location, Positions[index]))
            .ToArray();
        OpeningRegionProjection projection = new(
            topology,
            WorldBounds,
            24.0f,
            120.0f,
            anchors);
        return new OpeningRegionVillageTransitionSceneBinding(
            projection,
            projection.Anchors[2],
            SquareBounds,
            projection.Anchors[3],
            EdgeBounds);
    }
}
