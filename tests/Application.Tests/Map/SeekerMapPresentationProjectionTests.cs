using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Map;

public sealed class SeekerMapPresentationProjectionTests
{
    [Fact]
    public void ProjectsOnlyKnownLocationsWithStatusAndNotesInCanonicalOrder()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[8], catalogue.Locations[4]],
            [catalogue.Locations[7], catalogue.Locations[1]]);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            notes,
            seekerId,
            catalogue.Locations[8],
            "Ask where the road leads.");
        notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            notes,
            seekerId,
            catalogue.Locations[1],
            "Return for cultivated herbs.");

        SeekerMapPresentationSnapshot result = SeekerMapPresentationProjection.Project(map, notes);

        Assert.Same(seekerId, result.SeekerId);
        Assert.Collection(
            result.Locations,
            entry => AssertEntry(entry, catalogue.Locations[1], isVerified: true, "Return for cultivated herbs."),
            entry => AssertEntry(entry, catalogue.Locations[4], isVerified: false, null),
            entry => AssertEntry(entry, catalogue.Locations[7], isVerified: true, null),
            entry => AssertEntry(entry, catalogue.Locations[8], isVerified: false, "Ask where the road leads."));
        Assert.Equal([catalogue.Locations[4], catalogue.Locations[8]], map.ReportedLocations);
        Assert.Equal([catalogue.Locations[1], catalogue.Locations[7]], map.VerifiedLocations);
    }

    [Fact]
    public void ProjectsAClosedImmutableEmptySafeSurface()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(catalogue, seekerId, [], []);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);

        SeekerMapPresentationSnapshot result = SeekerMapPresentationProjection.Project(map, notes);

        Assert.Empty(result.Locations);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<SeekerMapPresentationEntry>)result.Locations).Clear());
        Assert.Equal(
            ["Locations", "SeekerId"],
            typeof(SeekerMapPresentationSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.Equal(
            ["IsVerified", "Location", "NoteText"],
            typeof(SeekerMapPresentationEntry)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(SeekerMapPresentationSnapshot).GetProperties(),
            property => Assert.Null(property.SetMethod));
        Assert.All(
            typeof(SeekerMapPresentationEntry).GetProperties(),
            property => Assert.Null(property.SetMethod));
    }

    [Fact]
    public void RejectsForeignOwnerCatalogueHiddenNotesAndNullInputs()
    {
        OpeningRegionMapLocationCatalogue catalogue = Catalogue();
        OpeningRegionMapLocationCatalogue foreignCatalogue = Catalogue();
        CharacterId seekerId = new("seeker");
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0], catalogue.Locations[1]],
            []);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);
        notes = SeekerMapNoteSnapshotTransition.Set(
            catalogue,
            map,
            notes,
            seekerId,
            catalogue.Locations[1],
            "Retained note.");
        SeekerMapSnapshot locationHidden = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [catalogue.Locations[0]],
            []);
        SeekerMapNoteSnapshot foreignOwner = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            new CharacterId("seeker"));
        SeekerMapNoteSnapshot foreignCatalogueNotes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            foreignCatalogue,
            seekerId);

        Assert.Throws<ArgumentException>(() =>
            SeekerMapPresentationProjection.Project(map, foreignOwner));
        Assert.Throws<ArgumentException>(() =>
            SeekerMapPresentationProjection.Project(map, foreignCatalogueNotes));
        Assert.Throws<ArgumentException>(() =>
            SeekerMapPresentationProjection.Project(locationHidden, notes));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapPresentationProjection.Project(null!, notes));
        Assert.Throws<ArgumentNullException>(() =>
            SeekerMapPresentationProjection.Project(map, null!));
    }

    private static OpeningRegionMapLocationCatalogue Catalogue() =>
        RepresentativeOpeningRegionMapLocationSource.Create();

    private static void AssertEntry(
        SeekerMapPresentationEntry entry,
        MapLocationDefinition location,
        bool isVerified,
        string? noteText)
    {
        Assert.Same(location, entry.Location);
        Assert.Equal(isVerified, entry.IsVerified);
        Assert.Equal(noteText, entry.NoteText);
    }
}
