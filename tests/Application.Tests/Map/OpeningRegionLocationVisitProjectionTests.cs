using System.Numerics;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class OpeningRegionLocationVisitProjectionTests
{
    [Fact]
    public void ProjectsSafeExactVisitWithinBoundedAnchorDistance()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionAnchor anchor = projection.Anchors[7];
        CharacterId seekerId = new("seeker");

        OpeningRegionLocationVisit? visit = OpeningRegionLocationVisitProjection.Project(
            projection,
            seekerId,
            anchor,
            anchor.Position + new Vector2(10.0f, -10.0f),
            20.0f);

        Assert.NotNull(visit);
        Assert.Same(seekerId, visit.SeekerId);
        Assert.Same(anchor.Location.Identity.Id, visit.LocationId);
        Assert.Equal(
            ["LocationId", "SeekerId"],
            typeof(OpeningRegionLocationVisit)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void ReturnsNoVisitOutsideTheBoundWithoutExposingCoordinates()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionAnchor anchor = projection.Anchors[7];

        OpeningRegionLocationVisit? visit = OpeningRegionLocationVisitProjection.Project(
            projection,
            new CharacterId("seeker"),
            anchor,
            anchor.Position + new Vector2(20.0f, 0.0f),
            10.0f);

        Assert.Null(visit);
    }

    [Fact]
    public void RejectsForeignAnchorNonFinitePositionInvalidBoundAndNullInput()
    {
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionProjection foreign = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionAnchor anchor = projection.Anchors[7];
        CharacterId seekerId = new("seeker");

        Assert.Throws<ArgumentException>(() => OpeningRegionLocationVisitProjection.Project(
            projection,
            seekerId,
            foreign.Anchors[7],
            anchor.Position,
            20.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                new Vector2(float.NaN, 0.0f),
                20.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                anchor.Position,
                0.0f));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                anchor.Position,
                float.PositiveInfinity));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            OpeningRegionLocationVisitProjection.Project(
                projection,
                seekerId,
                anchor,
                anchor.Position,
                projection.MinimumLandmarkClearancePixels / 2.0f));
        Assert.Throws<ArgumentNullException>(() => OpeningRegionLocationVisitProjection.Project(
            null!, seekerId, anchor, anchor.Position, 20.0f));
        Assert.Throws<ArgumentNullException>(() => OpeningRegionLocationVisitProjection.Project(
            projection, null!, anchor, anchor.Position, 20.0f));
        Assert.Throws<ArgumentNullException>(() => OpeningRegionLocationVisitProjection.Project(
            projection, seekerId, null!, anchor.Position, 20.0f));
    }
}
