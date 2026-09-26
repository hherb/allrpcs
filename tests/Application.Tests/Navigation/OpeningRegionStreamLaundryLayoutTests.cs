using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionStreamLaundryLayoutTests
{
    private const float OpeningWidthPixels = 64.0f;
    private const float WallThicknessPixels = 8.0f;
    private static readonly NavigationWorldBounds WorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);
    private static readonly NavigationWorldBounds StreamBounds =
        new(752.0f, 80.0f, 864.0f, 286.0f);
    private static readonly NavigationWorldBounds LaundryBounds =
        new(620.0f, 24.0f, 748.0f, 104.0f);
    private static readonly Vector2 VillageEdgeApproach = new(650.0f, 400.0f);
    private static readonly Vector2 BridgeNorthApproach = new(808.0f, 64.0f);
    private static readonly Vector2[] WestBank =
    [
        new(0.0f, 148.0f),
        new(752.0f, 117.3f),
        new(752.0f, 189.3f),
        new(0.0f, 220.0f),
    ];
    private static readonly Vector2[] EastBank =
    [
        new(864.0f, 132.4f),
        new(1600.0f, 108.0f),
        new(1600.0f, 180.0f),
        new(864.0f, 204.4f),
    ];
    private static readonly Vector2[] Positions =
    [
        new(920.0f, 820.0f),
        new(880.0f, 620.0f),
        new(880.0f, 400.0f),
        VillageEdgeApproach,
        new(808.0f, 260.0f),
        new(700.0f, 60.0f),
        new(470.0f, 70.0f),
        new(160.0f, 70.0f),
        new(620.0f, 620.0f),
        new(720.0f, 820.0f),
        new(120.0f, 620.0f),
    ];

    [Fact]
    public void CreatesCollisionAwareBanksAndTwoLaundryRouteOpenings()
    {
        OpeningRegionStreamLaundrySceneBinding binding = Binding();

        OpeningRegionStreamLaundryLayout layout = new(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach,
            WestBank,
            EastBank);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(OpeningWidthPixels, layout.OpeningWidthPixels);
        Assert.Equal(WallThicknessPixels, layout.WallThicknessPixels);
        Assert.Equal(VillageEdgeApproach, layout.VillageEdgeApproach);
        Assert.Equal(BridgeNorthApproach, layout.BridgeNorthApproach);
        Assert.Equal(new Vector2(620.0f, 64.0f), layout.LaundryWoodlandOpening);
        Assert.Equal(new Vector2(748.0f, 64.0f), layout.LaundryStreamOpening);
        Assert.Equal(WestBank, layout.WestStreamBank);
        Assert.Equal(EastBank, layout.EastStreamBank);
    }

    [Fact]
    public void RejectsInvalidDimensionsApproachesAndBankGeometry()
    {
        OpeningRegionStreamLaundrySceneBinding binding = Binding();
        float clearance = binding.Projection.MinimumPathClearancePixels;

        Assert.Throws<ArgumentOutOfRangeException>(() => Layout(
            binding,
            (clearance * 2.0f) - 1.0f,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach,
            WestBank,
            EastBank));
        Assert.Throws<ArgumentOutOfRangeException>(() => Layout(
            binding,
            OpeningWidthPixels,
            clearance * 2.0f,
            VillageEdgeApproach,
            BridgeNorthApproach,
            WestBank,
            EastBank));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach + Vector2.UnitX,
            BridgeNorthApproach,
            WestBank,
            EastBank));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach + Vector2.UnitX,
            WestBank,
            EastBank));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach,
            WestBank[..2],
            EastBank));
        Assert.Throws<ArgumentOutOfRangeException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach,
            WestBank,
            [.. EastBank, new Vector2(float.NaN, 100.0f)]));
        Assert.Throws<ArgumentException>(() => Layout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            VillageEdgeApproach,
            BridgeNorthApproach,
            [new Vector2(0.0f, 0.0f), new Vector2(1.0f, 1.0f), new Vector2(2.0f, 2.0f)],
            EastBank));
    }

    [Fact]
    public void RoutePolicyAcceptsVillageStreamLaundryRoundTrip()
    {
        OpeningRegionStreamLaundryLayout layout = Layout();
        Vector2 stream = layout.SceneBinding.StreamAnchor.Position;
        Vector2 laundry = layout.SceneBinding.AbandonedLaundryAnchor.Position;

        Assert.True(OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(
            layout,
            [
                layout.VillageEdgeApproach,
                stream,
                layout.BridgeNorthApproach,
                layout.LaundryStreamOpening,
                laundry,
                layout.LaundryStreamOpening,
                layout.BridgeNorthApproach,
                stream,
                layout.VillageEdgeApproach,
            ]));
    }

    [Fact]
    public void RoutePolicyRejectsBanksLaundryWallsWorldBoundsAndIncompleteRoutes()
    {
        OpeningRegionStreamLaundryLayout layout = Layout();

        Assert.False(OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(
            layout,
            [new Vector2(650.0f, 260.0f), new Vector2(650.0f, 120.0f)]));
        Assert.False(OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.AbandonedLaundryAnchor.Position, new Vector2(700.0f, 130.0f)]));
        Assert.False(OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.StreamAnchor.Position, new Vector2(1601.0f, 260.0f)]));
        Assert.False(OpeningRegionStreamLaundryRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.StreamAnchor.Position]));
    }

    private static OpeningRegionStreamLaundryLayout Layout() => Layout(
        Binding(),
        OpeningWidthPixels,
        WallThicknessPixels,
        VillageEdgeApproach,
        BridgeNorthApproach,
        WestBank,
        EastBank);

    private static OpeningRegionStreamLaundryLayout Layout(
        OpeningRegionStreamLaundrySceneBinding binding,
        float openingWidthPixels,
        float wallThicknessPixels,
        Vector2 villageEdgeApproach,
        Vector2 bridgeNorthApproach,
        IEnumerable<Vector2> westBank,
        IEnumerable<Vector2> eastBank) => new(
            binding,
            openingWidthPixels,
            wallThicknessPixels,
            villageEdgeApproach,
            bridgeNorthApproach,
            westBank,
            eastBank);

    private static OpeningRegionStreamLaundrySceneBinding Binding()
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
        return new OpeningRegionStreamLaundrySceneBinding(
            projection,
            projection.Anchors[4],
            StreamBounds,
            projection.Anchors[5],
            LaundryBounds);
    }
}
