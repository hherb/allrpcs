using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionSouthernRoadWagonSceneBindingTests
{
    private static readonly NavigationWorldBounds WagonBounds =
        new(648.0f, 744.0f, 776.0f, 876.0f);

    [Fact]
    public void RetainsExactRoadWagonAndShrineProvenance()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionOldShrineLayout shrineLayout = ShrineLayout(projection);
        OpeningRegionAnchor road = projection.Anchors[8];
        OpeningRegionAnchor wagon = projection.Anchors[9];

        OpeningRegionSouthernRoadWagonSceneBinding binding = new(
            shrineLayout,
            road,
            wagon,
            WagonBounds);

        Assert.Same(shrineLayout, binding.OldShrineLayout);
        Assert.Same(projection, binding.Projection);
        Assert.Same(road, binding.SouthernRoadAnchor);
        Assert.Same(wagon, binding.AbandonedWagonAnchor);
        Assert.Equal(WagonBounds, binding.AbandonedWagonBounds);
    }

    [Fact]
    public void RejectsRebuiltForeignAndSwappedAnchors()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionOldShrineLayout shrineLayout = ShrineLayout(projection);
        OpeningRegionAnchor road = projection.Anchors[8];
        OpeningRegionAnchor wagon = projection.Anchors[9];
        OpeningRegionProjection foreign = OpeningRegionOldShrineSceneBindingTests.Projection();

        Assert.Throws<ArgumentException>(() => new OpeningRegionSouthernRoadWagonSceneBinding(
            shrineLayout,
            new OpeningRegionAnchor(road.Location, road.Position),
            wagon,
            WagonBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSouthernRoadWagonSceneBinding(
            shrineLayout,
            foreign.Anchors[8],
            foreign.Anchors[9],
            WagonBounds));
        Assert.Throws<ArgumentException>(() => new OpeningRegionSouthernRoadWagonSceneBinding(
            shrineLayout,
            wagon,
            road,
            WagonBounds));
    }

    [Fact]
    public void RejectsMalformedOutOfBoundsOverlappingAndTightWagonAreas()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionOldShrineLayout shrineLayout = ShrineLayout(projection);
        OpeningRegionAnchor road = projection.Anchors[8];
        OpeningRegionAnchor wagon = projection.Anchors[9];

        Assert.Throws<ArgumentOutOfRangeException>(() => Binding(
            shrineLayout,
            road,
            wagon,
            new NavigationWorldBounds(float.NaN, 744.0f, 792.0f, 876.0f)));
        Assert.Throws<ArgumentOutOfRangeException>(() => Binding(
            shrineLayout,
            road,
            wagon,
            new NavigationWorldBounds(648.0f, 744.0f, 792.0f, 901.0f)));
        Assert.Throws<ArgumentException>(() => Binding(
            shrineLayout,
            road,
            wagon,
            new NavigationWorldBounds(24.0f, 24.0f, 760.0f, 850.0f)));
        Assert.Throws<ArgumentException>(() => Binding(
            shrineLayout,
            road,
            wagon,
            new NavigationWorldBounds(700.0f, 800.0f, 740.0f, 840.0f)));
    }

    private static OpeningRegionSouthernRoadWagonSceneBinding Binding(
        OpeningRegionOldShrineLayout shrineLayout,
        OpeningRegionAnchor road,
        OpeningRegionAnchor wagon,
        NavigationWorldBounds wagonBounds) => new(shrineLayout, road, wagon, wagonBounds);

    internal static OpeningRegionOldShrineLayout ShrineLayout(OpeningRegionProjection projection)
    {
        OpeningRegionOldShrineSceneBinding binding = new(
            projection,
            projection.Anchors[7],
            new NavigationWorldBounds(24.0f, 24.0f, 216.0f, 136.0f));
        return new OpeningRegionOldShrineLayout(
            binding,
            48.0f,
            8.0f,
            new Vector2(192.0f, 136.0f));
    }
}
