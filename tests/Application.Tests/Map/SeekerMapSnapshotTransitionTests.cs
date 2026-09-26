using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapSnapshotTransitionTests
{
    [Fact]
    public void ReportsAndDirectlyVerifiesExactCanonicalLocations()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot empty = Empty(catalogue, seekerId);

        SeekerMapSnapshot reported = SeekerMapSnapshotTransition.Report(
            catalogue,
            empty,
            seekerId,
            catalogue.Locations[4]);
        SeekerMapSnapshot verified = SeekerMapSnapshotTransition.Verify(
            catalogue,
            reported,
            seekerId,
            catalogue.Locations[0]);

        Assert.Equal([catalogue.Locations[4]], reported.ReportedLocations);
        Assert.Empty(reported.VerifiedLocations);
        Assert.Equal([catalogue.Locations[4]], verified.ReportedLocations);
        Assert.Equal([catalogue.Locations[0]], verified.VerifiedLocations);
        Assert.Empty(empty.ReportedLocations);
        Assert.Empty(empty.VerifiedLocations);
    }

    [Fact]
    public void PromotesReportedLocationToVerifiedAndRetainsCanonicalOrder()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot snapshot = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[8], catalogue.Locations[1]],
            [catalogue.Locations[7]]);

        SeekerMapSnapshot result = SeekerMapSnapshotTransition.Verify(
            catalogue,
            snapshot,
            seekerId,
            catalogue.Locations[1]);

        Assert.Equal([catalogue.Locations[8]], result.ReportedLocations);
        Assert.Equal(
            [catalogue.Locations[1], catalogue.Locations[7]],
            result.VerifiedLocations);
        Assert.Equal(
            [catalogue.Locations[1], catalogue.Locations[8]],
            snapshot.ReportedLocations);
        Assert.Equal([catalogue.Locations[7]], snapshot.VerifiedLocations);
    }

    [Fact]
    public void RejectsRebuiltForeignCrossOwnerRepeatedAndNullInputWithoutChangingState()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        OpeningRegionMapLocationCatalogue foreign = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot snapshot = Empty(catalogue, seekerId);
        MapLocationDefinition rebuilt = new(
            catalogue.Locations[0].Id,
            catalogue.Locations[0].DisplayLabel);

        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            snapshot,
            new CharacterId("seeker"),
            catalogue.Locations[0]));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotTransition.Report(
            foreign,
            snapshot,
            seekerId,
            foreign.Locations[0]));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            snapshot,
            seekerId,
            rebuilt));
        SeekerMapSnapshot reported = SeekerMapSnapshotTransition.Report(
            catalogue,
            snapshot,
            seekerId,
            catalogue.Locations[0]);
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            reported,
            seekerId,
            catalogue.Locations[0]));
        SeekerMapSnapshot verified = SeekerMapSnapshotTransition.Verify(
            catalogue,
            reported,
            seekerId,
            catalogue.Locations[0]);
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotTransition.Verify(
            catalogue,
            verified,
            seekerId,
            catalogue.Locations[0]));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotTransition.Report(
            null!,
            snapshot,
            seekerId,
            catalogue.Locations[0]));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            null!,
            seekerId,
            catalogue.Locations[0]));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            snapshot,
            null!,
            catalogue.Locations[0]));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotTransition.Report(
            catalogue,
            snapshot,
            seekerId,
            null!));
        Assert.Empty(snapshot.ReportedLocations);
        Assert.Empty(snapshot.VerifiedLocations);
    }

    private static OpeningRegionMapLocationCatalogue Catalogue() =>
        RepresentativeOpeningRegionMapLocationSource.Create();

    private static SeekerMapSnapshot Empty(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId) => SeekerMapSnapshotProjection.Project(catalogue, seekerId, [], []);
}
