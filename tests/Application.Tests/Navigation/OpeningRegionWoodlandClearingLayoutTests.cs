using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionWoodlandClearingLayoutTests
{
    private const float OpeningWidthPixels = 48.0f;
    private const float WallThicknessPixels = 8.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds ClearingBounds =
        new(240.0f, 24.0f, 536.0f, 120.0f);
    private static readonly Vector2 VillageOpening = new(497.0f, 120.0f);
    private static readonly Vector2 ClosedPathOpening = new(438.0f, 120.0f);
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
    public void CreatesFourTopologyAlignedOpeningsAroundClearing()
    {
        OpeningRegionWoodlandClearingSceneBinding binding = Binding();

        OpeningRegionWoodlandClearingLayout layout = new(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageOpening,
            ClosedPathOpening);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(OpeningWidthPixels, layout.OpeningWidthPixels);
        Assert.Equal(WallThicknessPixels, layout.WallThicknessPixels);
        Assert.Equal(new Vector2(536.0f, 70.0f), layout.LaundryOpening);
        Assert.Equal(new Vector2(240.0f, 70.0f), layout.ShrineOpening);
        Assert.Equal(VillageOpening, layout.VillageEdgeOpening);
        Assert.Equal(ClosedPathOpening, layout.ClosedForestPathOpening);
    }

    [Fact]
    public void RejectsInvalidDimensionsMisalignedAndOverlappingOpenings()
    {
        OpeningRegionWoodlandClearingSceneBinding binding = Binding();
        float clearance = binding.Projection.MinimumPathClearancePixels;

        Assert.Throws<ArgumentOutOfRangeException>(() => Layout(
            binding,
            (clearance * 2.0f) - 1.0f,
            WallThicknessPixels,
            VillageOpening,
            ClosedPathOpening));
        Assert.Throws<ArgumentOutOfRangeException>(() => Layout(
            binding,
            OpeningWidthPixels,
            clearance * 2.0f,
            VillageOpening,
            ClosedPathOpening));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageOpening + Vector2.UnitY,
            ClosedPathOpening));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageOpening,
            new Vector2(480.0f, 120.0f)));
    }

    [Fact]
    public void RoutePolicyAcceptsTopologyOpeningsAndRejectsTreeBoundaries()
    {
        OpeningRegionWoodlandClearingLayout layout = Layout();
        Vector2 clearing = layout.SceneBinding.WoodlandClearingAnchor.Position;

        Assert.True(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.Projection.Anchors[5].Position, clearing]));
        Assert.True(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.Projection.Anchors[7].Position, clearing]));
        Assert.True(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [layout.VillageEdgeOpening, clearing, layout.ClosedForestPathOpening]));
        Assert.False(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [clearing, new Vector2(470.0f, 0.0f)]));
        Assert.False(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [clearing, new Vector2(536.0f, 105.0f)]));
        Assert.False(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(
            layout,
            [clearing, new Vector2(1601.0f, 70.0f)]));
        Assert.False(OpeningRegionWoodlandClearingRoutePolicy.IsAcceptable(layout, [clearing]));
    }

    private static OpeningRegionWoodlandClearingLayout Layout() => Layout(
        Binding(),
        OpeningWidthPixels,
        WallThicknessPixels,
        VillageOpening,
        ClosedPathOpening);

    private static OpeningRegionWoodlandClearingLayout Layout(
        OpeningRegionWoodlandClearingSceneBinding binding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 villageOpening,
        Vector2 closedPathOpening) => new(
            binding,
            openingWidthPixels,
            wallThicknessPixels,
            villageOpening,
            closedPathOpening);

    private static OpeningRegionWoodlandClearingSceneBinding Binding()
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
        return new OpeningRegionWoodlandClearingSceneBinding(
            projection,
            projection.Anchors[6],
            ClearingBounds);
    }
}
