using System.Numerics;
using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class OpeningRegionClosedForestPathLayoutTests
{
    private const float ApproachHalfWidthPixels = 24.0f;
    private const float BoundaryThicknessPixels = 8.0f;
    private const float ApproachLengthPixels = 16.0f;
    private static readonly Vector2 GateCenter = new(438.0f, 120.0f);

    [Fact]
    public void CreatesMinimumClearanceApproachAndClosedGate()
    {
        OpeningRegionClosedForestPathSceneBinding binding = Binding();

        OpeningRegionClosedForestPathLayout layout = Layout(binding);

        Assert.Same(binding, layout.SceneBinding);
        Assert.Equal(ApproachHalfWidthPixels, layout.ApproachHalfWidthPixels);
        Assert.Equal(BoundaryThicknessPixels, layout.BoundaryThicknessPixels);
        Assert.Equal(ApproachLengthPixels, layout.ApproachLengthPixels);
        Assert.Equal(GateCenter, layout.GateCenter);
        Assert.True(layout.IsGateClosed);
        Assert.Equal(2, layout.ApproachBoundarySegments.Count);
        Assert.Equal(48.0f, Vector2.Distance(
            layout.GateBoundary.Start,
            layout.GateBoundary.End));
        Assert.Equal(
            binding.Projection.MinimumPathClearancePixels,
            Vector2.Distance(layout.GateApproachPosition, layout.GateCenter),
            precision: 3);
    }

    [Fact]
    public void RejectsMalformedInsufficientAndOverlappingApproachGeometry()
    {
        OpeningRegionClosedForestPathSceneBinding binding = Binding();

        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            binding,
            float.NaN,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            GateCenter));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            binding,
            23.0f,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            GateCenter));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            binding,
            25.0f,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            GateCenter));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            binding,
            ApproachHalfWidthPixels,
            ApproachHalfWidthPixels,
            ApproachLengthPixels,
            GateCenter));
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(
            binding,
            ApproachHalfWidthPixels,
            BoundaryThicknessPixels,
            0.0f,
            GateCenter));
        Assert.Throws<ArgumentException>(() => Create(
            binding,
            ApproachHalfWidthPixels,
            BoundaryThicknessPixels,
            80.0f,
            GateCenter));
    }

    [Fact]
    public void RejectsMisalignedAndOutOfBoundsGateCenters()
    {
        OpeningRegionClosedForestPathSceneBinding binding = Binding();

        Assert.Throws<ArgumentException>(() => Create(
            binding,
            ApproachHalfWidthPixels,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            GateCenter + Vector2.UnitX));
        Assert.Throws<ArgumentException>(() => Create(
            binding,
            ApproachHalfWidthPixels,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            new Vector2(438.0f, 901.0f)));
    }

    [Fact]
    public void RoutePolicyAcceptsApproachAndRejectsSideGateAndWorldCrossings()
    {
        OpeningRegionClosedForestPathLayout layout = Layout(Binding());
        Vector2 clearing = layout.SceneBinding.WoodlandClearingLayout
            .SceneBinding.WoodlandClearingAnchor.Position;
        Vector2 sideCrossing = Vector2.Lerp(
            layout.ApproachBoundarySegments[0].Start,
            layout.ApproachBoundarySegments[0].End,
            0.5f);
        Vector2 sideDirection = Vector2.Normalize(
            layout.ApproachBoundarySegments[0].End
            - layout.ApproachBoundarySegments[0].Start);
        Vector2 sideNormal = new(-sideDirection.Y, sideDirection.X);
        Vector2 beyondGate = layout.GateCenter + Vector2.UnitY * 40.0f;

        Assert.True(OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(
            layout,
            [clearing, layout.GateApproachPosition]));
        Assert.False(OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(
            layout,
            [sideCrossing - (sideNormal * 16.0f), sideCrossing + (sideNormal * 16.0f)]));
        Assert.False(OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(
            layout,
            [layout.GateApproachPosition, beyondGate]));
        Assert.False(OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(
            layout,
            [clearing, new Vector2(-1.0f, clearing.Y)]));
        Assert.False(OpeningRegionClosedForestPathRoutePolicy.IsAcceptable(layout, [clearing]));
    }

    private static OpeningRegionClosedForestPathLayout Layout(
        OpeningRegionClosedForestPathSceneBinding binding) => Create(
            binding,
            ApproachHalfWidthPixels,
            BoundaryThicknessPixels,
            ApproachLengthPixels,
            GateCenter);

    private static OpeningRegionClosedForestPathLayout Create(
        OpeningRegionClosedForestPathSceneBinding binding,
        float approachHalfWidthPixels,
        float boundaryThicknessPixels,
        float approachLengthPixels,
        Vector2 gateCenter) => new(
            binding,
            approachHalfWidthPixels,
            boundaryThicknessPixels,
            approachLengthPixels,
            gateCenter);

    private static OpeningRegionClosedForestPathSceneBinding Binding()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        return new OpeningRegionClosedForestPathSceneBinding(
            OpeningRegionClosedForestPathSceneBindingTests.ClearingLayout(projection),
            projection.Anchors[10]);
    }
}
