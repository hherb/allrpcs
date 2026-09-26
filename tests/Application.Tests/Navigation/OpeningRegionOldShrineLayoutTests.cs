using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionOldShrineLayoutTests
{
    private const float OpeningWidthPixels = 48.0f;
    private const float WallThicknessPixels = 8.0f;
    private static readonly Vector2 RoadOpening = new(192.0f, 136.0f);

    [Fact]
    public void CreatesClearingAndRoadOpenings()
    {
        OpeningRegionOldShrineSceneBinding binding = Binding();

        OpeningRegionOldShrineLayout layout = new(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            RoadOpening);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(new Vector2(216.0f, 70.0f), layout.WoodlandClearingOpening);
        Assert.Equal(RoadOpening, layout.SouthernRoadOpening);
    }

    [Fact]
    public void RejectsInvalidDimensionsAndMisalignedRoadOpening()
    {
        OpeningRegionOldShrineSceneBinding binding = Binding();

        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionOldShrineLayout(
            binding,
            47.0f,
            WallThicknessPixels,
            RoadOpening));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionOldShrineLayout(
            binding,
            OpeningWidthPixels,
            48.0f,
            RoadOpening));
        Assert.Throws<ArgumentException>(() => new OpeningRegionOldShrineLayout(
            binding,
            OpeningWidthPixels,
            WallThicknessPixels,
            new Vector2(150.0f, 136.0f)));
    }

    [Fact]
    public void RoutePolicyAcceptsOpeningsAndRejectsShrineWalls()
    {
        OpeningRegionOldShrineLayout layout = Layout();
        Vector2 shrine = layout.SceneBinding.OldShrineAnchor.Position;
        Vector2 clearing = layout.SceneBinding.Projection.Anchors[6].Position;

        Assert.True(OpeningRegionOldShrineRoutePolicy.IsAcceptable(layout, [clearing, shrine]));
        Assert.True(OpeningRegionOldShrineRoutePolicy.IsAcceptable(
            layout,
            [layout.SouthernRoadOpening, shrine]));
        Assert.False(OpeningRegionOldShrineRoutePolicy.IsAcceptable(
            layout,
            [shrine, new Vector2(160.0f, 0.0f)]));
        Assert.False(OpeningRegionOldShrineRoutePolicy.IsAcceptable(
            layout,
            [shrine, new Vector2(24.0f, 70.0f)]));
        Assert.False(OpeningRegionOldShrineRoutePolicy.IsAcceptable(layout, [shrine]));
    }

    private static OpeningRegionOldShrineLayout Layout() => new(
        Binding(),
        OpeningWidthPixels,
        WallThicknessPixels,
        RoadOpening);

    private static OpeningRegionOldShrineSceneBinding Binding()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        return new OpeningRegionOldShrineSceneBinding(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
    }
}
