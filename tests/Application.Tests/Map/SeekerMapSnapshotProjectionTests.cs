using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapSnapshotProjectionTests
{
    [Fact]
    public void ProjectsEmptyReportedAndVerifiedSafeStatesInCanonicalOrder()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");

        SeekerMapSnapshot empty = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        SeekerMapSnapshot populated = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[8], catalogue.Locations[1]],
            [catalogue.Locations[7], catalogue.Locations[0]]);

        Assert.Same(seekerId, empty.SeekerId);
        Assert.Empty(empty.ReportedLocations);
        Assert.Empty(empty.VerifiedLocations);
        Assert.Equal(
            [catalogue.Locations[1], catalogue.Locations[8]],
            populated.ReportedLocations);
        Assert.Equal(
            [catalogue.Locations[0], catalogue.Locations[7]],
            populated.VerifiedLocations);
    }

    [Fact]
    public void ReturnsIndependentImmutableCollectionsWithoutHiddenCatalogueExposure()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");

        SeekerMapSnapshot first = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[4]],
            []);
        SeekerMapSnapshot second = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[4]],
            []);

        Assert.NotSame(first, second);
        Assert.NotSame(first.ReportedLocations, second.ReportedLocations);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<MapLocationDefinition>)first.ReportedLocations).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<MapLocationDefinition>)first.VerifiedLocations).Clear());
        Assert.Equal(
            ["ReportedLocations", "SeekerId", "VerifiedLocations"],
            typeof(SeekerMapSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void RejectsRebuiltForeignDuplicateOverlappingAndNullInput()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        OpeningRegionMapLocationCatalogue foreign = Catalogue();
        CharacterId seekerId = new("seeker");
        MapLocationDefinition rebuilt = new(
            catalogue.Locations[0].Id,
            catalogue.Locations[0].DisplayLabel);

        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [rebuilt],
            []));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [foreign.Locations[0]],
            []));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0], catalogue.Locations[0]],
            []));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            [catalogue.Locations[0]]));
        Assert.Throws<ArgumentException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [null!],
            []));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotProjection.Project(
            null!,
            seekerId,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            null!,
            [],
            []));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            null!,
            []));
        Assert.Throws<ArgumentNullException>(() => SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            null!));
    }

    private static OpeningRegionMapLocationCatalogue Catalogue() =>
        RepresentativeOpeningRegionMapLocationSource.Create();
}
