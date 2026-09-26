using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionClosedForestPathSceneBindingTests
{
    [Fact]
    public void RetainsExactClearingAndClosedPathProvenance()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionWoodlandClearingLayout clearingLayout = ClearingLayout(projection);
        OpeningRegionAnchor closedPath = projection.Anchors[10];

        OpeningRegionClosedForestPathSceneBinding binding = new(
            clearingLayout,
            closedPath);

        Assert.Same(clearingLayout, binding.WoodlandClearingLayout);
        Assert.Same(projection, binding.Projection);
        Assert.Same(closedPath, binding.ClosedForestPathAnchor);
    }

    [Fact]
    public void RejectsMissingRebuiltForeignAndWrongRoleAnchors()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionWoodlandClearingLayout clearingLayout = ClearingLayout(projection);
        OpeningRegionAnchor closedPath = projection.Anchors[10];
        OpeningRegionProjection foreign = OpeningRegionOldShrineSceneBindingTests.Projection();

        Assert.Throws<ArgumentNullException>(() =>
            new OpeningRegionClosedForestPathSceneBinding(clearingLayout, null!));
        Assert.Throws<ArgumentException>(() => new OpeningRegionClosedForestPathSceneBinding(
            clearingLayout,
            new OpeningRegionAnchor(closedPath.Location, closedPath.Position)));
        Assert.Throws<ArgumentException>(() => new OpeningRegionClosedForestPathSceneBinding(
            clearingLayout,
            foreign.Anchors[10]));
        Assert.Throws<ArgumentException>(() => new OpeningRegionClosedForestPathSceneBinding(
            clearingLayout,
            projection.Anchors[9]));
    }

    [Fact]
    public void RejectsClearingAndAnchorFromDifferentProjections()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionProjection foreign = OpeningRegionOldShrineSceneBindingTests.Projection();

        Assert.Throws<ArgumentException>(() => new OpeningRegionClosedForestPathSceneBinding(
            ClearingLayout(projection),
            foreign.Anchors[10]));
    }

    internal static OpeningRegionWoodlandClearingLayout ClearingLayout(
        OpeningRegionProjection projection)
    {
        OpeningRegionWoodlandClearingSceneBinding binding = new(
            projection,
            projection.Anchors[6],
            new NavigationWorldBounds(240.0f, 24.0f, 536.0f, 120.0f));
        return new OpeningRegionWoodlandClearingLayout(
            binding,
            48.0f,
            8.0f,
            new System.Numerics.Vector2(497.0f, 120.0f),
            new System.Numerics.Vector2(438.0f, 120.0f));
    }
}
