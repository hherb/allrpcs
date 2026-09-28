using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionSouthernRoadWagonLayoutTests
{
    private const float RoadHalfWidthPixels = 24.0f;
    private const float BoundaryThicknessPixels = 8.0f;
    private const float WagonOpeningWidthPixels = 48.0f;
    private static readonly Vector2 WagonOpening = new(690.0f, 744.0f);

    [Fact]
    public void CreatesMinimumClearanceRoadAndInertWagonShell()
    {
        OpeningRegionSouthernRoadWagonSceneBinding binding = Binding();

        OpeningRegionSouthernRoadWagonLayout layout = Layout(binding);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(RoadHalfWidthPixels, layout.RoadHalfWidthPixels);
        Assert.Equal(BoundaryThicknessPixels, layout.BoundaryThicknessPixels);
        Assert.Equal(WagonOpeningWidthPixels, layout.WagonOpeningWidthPixels);
        Assert.Equal(WagonOpening, layout.WagonOpening);
        Assert.Equal(
            [
                binding.SouthernRoadAnchor.Position,
                WagonOpening,
                binding.AbandonedWagonAnchor.Position,
            ],
            layout.RouteCenterline);
        Assert.Equal(2, layout.RoadBoundarySegments.Count);
        Assert.Equal(5, layout.WagonBoundarySegments.Count);
    }

    [Fact]
    public void RejectsMalformedClearanceAndMisalignedOpening()
    {
        OpeningRegionSouthernRoadWagonSceneBinding binding = Binding();

        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSouthernRoadWagonLayout(
            binding,
            23.0f,
            BoundaryThicknessPixels,
            WagonOpeningWidthPixels,
            WagonOpening));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSouthernRoadWagonLayout(
            binding,
            RoadHalfWidthPixels,
            24.0f,
            WagonOpeningWidthPixels,
            WagonOpening));
        Assert.Throws<ArgumentOutOfRangeException>(() => new OpeningRegionSouthernRoadWagonLayout(
            binding,
            RoadHalfWidthPixels,
            BoundaryThicknessPixels,
            47.0f,
            WagonOpening));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSouthernRoadWagonLayout(
            binding,
            RoadHalfWidthPixels,
            BoundaryThicknessPixels,
            WagonOpeningWidthPixels,
            new Vector2(660.0f, 744.0f)));
    }

    [Fact]
    public void RoutePolicyAcceptsCenterlineAndRejectsRoadAndWagonBoundaryCrossings()
    {
        OpeningRegionSouthernRoadWagonLayout layout = Layout(Binding());

        Assert.True(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            layout.RouteCenterline));
        Assert.True(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            layout.RouteCenterline.Reverse().ToArray()));
        Assert.True(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            [new Vector2(800.0f, 876.0f), new Vector2(900.0f, 876.0f)]));
        Vector2 roadDirection = Vector2.Normalize(
            layout.RouteCenterline[1] - layout.RouteCenterline[0]);
        Assert.False(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            [
                layout.SceneBinding.SouthernRoadAnchor.Position
                    + (roadDirection * layout.RoadHalfWidthPixels),
                new Vector2(500.0f, 760.0f),
            ]));
        Assert.False(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.AbandonedWagonAnchor.Position, new Vector2(800.0f, 820.0f)]));
        Assert.False(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.SouthernRoadAnchor.Position, new Vector2(1601.0f, 620.0f)]));
        Assert.False(OpeningRegionSouthernRoadWagonRoutePolicy.IsAcceptable(
            layout,
            [layout.SceneBinding.SouthernRoadAnchor.Position]));
    }

    private static OpeningRegionSouthernRoadWagonLayout Layout(
        OpeningRegionSouthernRoadWagonSceneBinding binding) => new(
            binding,
            RoadHalfWidthPixels,
            BoundaryThicknessPixels,
            WagonOpeningWidthPixels,
            WagonOpening);

    private static OpeningRegionSouthernRoadWagonSceneBinding Binding()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        return new OpeningRegionSouthernRoadWagonSceneBinding(
            OpeningRegionSouthernRoadWagonSceneBindingTests.ShrineLayout(projection),
            projection.Anchors[8],
            projection.Anchors[9],
            new NavigationWorldBounds(648.0f, 744.0f, 776.0f, 876.0f));
    }
}
