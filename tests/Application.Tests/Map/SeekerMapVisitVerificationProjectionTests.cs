using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Application.Tests.Navigation;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapVisitVerificationProjectionTests
{
    [Fact]
    public void VerifiesExactVisitedLocationAndPromotesPriorReport()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot empty = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        SeekerMapSnapshot reported = SeekerMapSnapshotTransition.Report(
            catalogue,
            empty,
            seekerId,
            catalogue.Locations[7]);
        OpeningRegionLocationVisit visit = Visit(projection, seekerId, 7);

        SeekerMapSnapshot result = SeekerMapVisitVerificationProjection.Project(
            catalogue,
            reported,
            projection,
            visit,
            catalogue.Locations[7]);

        Assert.Empty(result.ReportedLocations);
        Assert.Equal([catalogue.Locations[7]], result.VerifiedLocations);
        Assert.Equal([catalogue.Locations[7]], reported.ReportedLocations);
        Assert.Empty(reported.VerifiedLocations);
    }

    [Fact]
    public void VerifiesPreviouslyHiddenExactVisitedLocation()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        OpeningRegionLocationVisit visit = Visit(projection, seekerId, 0);

        SeekerMapSnapshot result = SeekerMapVisitVerificationProjection.Project(
            catalogue,
            map,
            projection,
            visit,
            catalogue.Locations[0]);

        Assert.Empty(result.ReportedLocations);
        Assert.Equal([catalogue.Locations[0]], result.VerifiedLocations);
    }

    [Fact]
    public void RejectsForeignProjectionOwnerCatalogueLocationAndRepeatedVerification()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionMapLocationCatalogue foreignCatalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        OpeningRegionProjection foreignProjection =
            OpeningRegionOldShrineSceneBindingTests.Projection();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        OpeningRegionLocationVisit visit = Visit(projection, seekerId, 7);
        OpeningRegionLocationVisit foreignOwner = Visit(
            projection,
            new CharacterId("seeker"),
            7);

        Assert.Throws<ArgumentException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue,
            map,
            foreignProjection,
            visit,
            catalogue.Locations[7]));
        Assert.Throws<ArgumentException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue,
            map,
            projection,
            foreignOwner,
            catalogue.Locations[7]));
        Assert.Throws<ArgumentException>(() => SeekerMapVisitVerificationProjection.Project(
            foreignCatalogue,
            map,
            projection,
            visit,
            foreignCatalogue.Locations[7]));
        Assert.Throws<ArgumentException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue,
            map,
            projection,
            visit,
            catalogue.Locations[6]));

        SeekerMapSnapshot verified = SeekerMapVisitVerificationProjection.Project(
            catalogue,
            map,
            projection,
            visit,
            catalogue.Locations[7]);
        Assert.Throws<ArgumentException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue,
            verified,
            projection,
            visit,
            catalogue.Locations[7]));
        Assert.Empty(map.VerifiedLocations);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        OpeningRegionProjection projection = OpeningRegionOldShrineSceneBindingTests.Projection();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        OpeningRegionLocationVisit visit = Visit(projection, seekerId, 7);
        MapLocationDefinition location = catalogue.Locations[7];

        Assert.Throws<ArgumentNullException>(() => SeekerMapVisitVerificationProjection.Project(
            null!, map, projection, visit, location));
        Assert.Throws<ArgumentNullException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue, null!, projection, visit, location));
        Assert.Throws<ArgumentNullException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue, map, null!, visit, location));
        Assert.Throws<ArgumentNullException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue, map, projection, null!, location));
        Assert.Throws<ArgumentNullException>(() => SeekerMapVisitVerificationProjection.Project(
            catalogue, map, projection, visit, null!));
    }

    private static OpeningRegionLocationVisit Visit(
        OpeningRegionProjection projection,
        CharacterId seekerId,
        int anchorIndex) => OpeningRegionLocationVisitProjection.Project(
            projection,
            seekerId,
            projection.Anchors[anchorIndex],
            projection.Anchors[anchorIndex].Position,
            20.0f)!;
}
