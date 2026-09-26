using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Map;

public sealed class MapSaveStateCaptureProjectionTests
{
    [Fact]
    public void CapturesOnlyKnownMembershipAndNotesInCanonicalOrder()
    {
        MapInput input = CreateInput();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [input.Catalogue.Locations[2], input.Catalogue.Locations[0]],
            [input.Catalogue.Locations[4], input.Catalogue.Locations[1]]);
        SeekerMapNoteSnapshot notes = AddNote(
            input,
            map,
            AddNote(input, map, input.Notes, input.Catalogue.Locations[4], "Water level rose."),
            input.Catalogue.Locations[0],
            "Ask the healer about the patient.");

        MapSaveState state = MapSaveStateCaptureProjection.Capture(map, notes);

        Assert.Same(input.SeekerId, state.SeekerId);
        Assert.Equal(["sickhouse", "village-square"], Values(state.ReportedLocationIds));
        Assert.Equal(["healer-garden", "stream"], Values(state.VerifiedLocationIds));
        Assert.Equal(["sickhouse", "stream"], Values(state.Notes.Select(note => note.LocationId)));
        Assert.Equal(
            ["Ask the healer about the patient.", "Water level rose."],
            state.Notes.Select(note => note.Text));
    }

    [Fact]
    public void CapturesEmptyMapAndNotes()
    {
        MapInput input = CreateInput();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [],
            []);

        MapSaveState state = MapSaveStateCaptureProjection.Capture(map, input.Notes);

        Assert.Empty(state.ReportedLocationIds);
        Assert.Empty(state.VerifiedLocationIds);
        Assert.Empty(state.Notes);
    }

    [Fact]
    public void ReturnsFreshIndependentImmutablePersistenceValues()
    {
        MapInput input = CreateInput();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [input.Catalogue.Locations[0]],
            []);
        SeekerMapNoteSnapshot notes = AddNote(
            input,
            map,
            input.Notes,
            input.Catalogue.Locations[0],
            "Return before dusk.");

        MapSaveState first = MapSaveStateCaptureProjection.Capture(map, notes);
        MapSaveState second = MapSaveStateCaptureProjection.Capture(map, notes);

        Assert.NotSame(first, second);
        Assert.NotSame(first.ReportedLocationIds, second.ReportedLocationIds);
        Assert.NotSame(first.VerifiedLocationIds, second.VerifiedLocationIds);
        Assert.NotSame(first.Notes, second.Notes);
        Assert.NotSame(first.Notes[0], second.Notes[0]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<LocationId>)first.ReportedLocationIds).Clear());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<MapNoteSaveEntry>)first.Notes).Clear());
    }

    [Fact]
    public void RejectsForeignCatalogueOwnerAndHiddenNoteWithoutPartialOutput()
    {
        MapInput input = CreateInput();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [input.Catalogue.Locations[0]],
            []);
        OpeningRegionMapLocationCatalogue foreignCatalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        SeekerMapNoteSnapshot foreignCatalogueNotes =
            SeekerMapNoteSnapshotProjection.CreateEmpty(foreignCatalogue, input.SeekerId);
        SeekerMapNoteSnapshot foreignOwnerNotes =
            SeekerMapNoteSnapshotProjection.CreateEmpty(input.Catalogue, new CharacterId("seeker"));
        SeekerMapSnapshot mapWithSecondLocation = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [input.Catalogue.Locations[0], input.Catalogue.Locations[1]],
            []);
        SeekerMapNoteSnapshot hiddenNote = AddNote(
            input,
            mapWithSecondLocation,
            input.Notes,
            input.Catalogue.Locations[1],
            "Still hidden.");

        Assert.Throws<ArgumentException>(() =>
            MapSaveStateCaptureProjection.Capture(map, foreignCatalogueNotes));
        Assert.Throws<ArgumentException>(() =>
            MapSaveStateCaptureProjection.Capture(map, foreignOwnerNotes));
        Assert.Throws<ArgumentException>(() =>
            MapSaveStateCaptureProjection.Capture(map, hiddenNote));
        Assert.Single(map.ReportedLocations);
        Assert.Empty(input.Notes.Notes);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        MapInput input = CreateInput();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            input.Catalogue,
            input.SeekerId,
            [],
            []);

        Assert.Throws<ArgumentNullException>(() =>
            MapSaveStateCaptureProjection.Capture(null!, input.Notes));
        Assert.Throws<ArgumentNullException>(() =>
            MapSaveStateCaptureProjection.Capture(map, null!));
    }

    [Fact]
    public void ExposesOnlyClosedVisibilitySafePersistenceSurfaces()
    {
        Assert.Equal(
            ["Notes", "ReportedLocationIds", "SeekerId", "VerifiedLocationIds"],
            typeof(MapSaveState)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(MapSaveState).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.DoesNotContain(
            typeof(MapSaveState).GetProperties(),
            property => property.PropertyType.Namespace?.StartsWith(
                "Allrpcs.Application",
                StringComparison.Ordinal) == true);
        Assert.Equal(
            ["LocationId", "Text"],
            typeof(MapNoteSaveEntry)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(MapNoteSaveEntry).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    private static SeekerMapNoteSnapshot AddNote(
        MapInput input,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes,
        MapLocationDefinition location,
        string text) => SeekerMapNoteSnapshotTransition.Set(
            input.Catalogue,
            map,
            notes,
            input.SeekerId,
            location,
            text);

    private static string[] Values(IEnumerable<LocationId> ids) =>
        ids.Select(id => id.Value).ToArray();

    private static MapInput CreateInput()
    {
        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        CharacterId seekerId = new("seeker");
        return new MapInput(
            catalogue,
            seekerId,
            SeekerMapNoteSnapshotProjection.CreateEmpty(catalogue, seekerId));
    }

    private sealed record MapInput(
        OpeningRegionMapLocationCatalogue Catalogue,
        CharacterId SeekerId,
        SeekerMapNoteSnapshot Notes);
}
