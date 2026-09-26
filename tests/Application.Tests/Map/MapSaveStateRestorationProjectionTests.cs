using Allrpcs.Application.Map;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Map;

public sealed class MapSaveStateRestorationProjectionTests
{
    [Fact]
    public void RestoresFreshCanonicalDefinitionsAndStableCanonicalOrder()
    {
        MapSaveState state = PopulatedState();

        MapSaveStateRestoration restored = MapSaveStateRestorationProjection.Restore(state);

        Assert.Same(state.SeekerId, restored.Map.SeekerId);
        Assert.Same(state.SeekerId, restored.Notes.SeekerId);
        Assert.Equal(["sickhouse", "village-square"], Values(restored.Map.ReportedLocations));
        Assert.Equal(["healer-garden", "stream"], Values(restored.Map.VerifiedLocations));
        Assert.Equal(["sickhouse", "stream"], Values(restored.Notes.Notes));
        Assert.Equal(
            ["Ask the healer about the patient.", "Water level rose."],
            restored.Notes.Notes.Select(note => note.Text));
        Assert.Same(restored.Map.ReportedLocations[0], restored.Notes.Notes[0].Location);
        Assert.Same(restored.Map.VerifiedLocations[1], restored.Notes.Notes[1].Location);
    }

    [Fact]
    public void RestoresEmptyState()
    {
        MapSaveStateRestoration restored =
            MapSaveStateRestorationProjection.Restore(EmptyState());

        Assert.Empty(restored.Map.ReportedLocations);
        Assert.Empty(restored.Map.VerifiedLocations);
        Assert.Empty(restored.Notes.Notes);
    }

    [Fact]
    public void RoundTripsEmptyAndPopulatedStateWithoutSharingRestoredObjects()
    {
        foreach (MapSaveState state in new[] { EmptyState(), PopulatedState() })
        {
            MapSaveStateRestoration first = MapSaveStateRestorationProjection.Restore(state);
            MapSaveStateRestoration second = MapSaveStateRestorationProjection.Restore(state);
            MapSaveState captured = MapSaveStateCaptureProjection.Capture(first.Map, first.Notes);

            Assert.Equal(Values(first.Map.ReportedLocations), Values(captured.ReportedLocationIds));
            Assert.Equal(Values(first.Map.VerifiedLocations), Values(captured.VerifiedLocationIds));
            Assert.Equal(
                first.Notes.Notes.Select(note => (note.Location.Id.Value, note.Text)),
                captured.Notes.Select(note => (note.LocationId.Value, note.Text)));
            Assert.NotSame(first, second);
            Assert.NotSame(first.Map, second.Map);
            Assert.NotSame(first.Notes, second.Notes);
            if (first.Map.ReportedLocations.Count > 0)
            {
                Assert.NotSame(first.Map.ReportedLocations[0], second.Map.ReportedLocations[0]);
                Assert.NotSame(first.Notes.Notes[0], second.Notes.Notes[0]);
            }
        }
    }

    [Fact]
    public void RejectsUnknownDuplicateOverlappingAndHiddenLocationStateAtomically()
    {
        MapSaveState baseline = PopulatedState();
        LocationId unknown = new("unknown-place");
        MapSaveState[] invalidStates =
        [
            Copy(baseline, reported: [unknown]),
            Copy(baseline, verified: [unknown]),
            Copy(baseline, notes: [new MapNoteSaveEntry(unknown, "Unknown.")]),
            Copy(baseline, reported: [baseline.ReportedLocationIds[0], baseline.ReportedLocationIds[0]]),
            Copy(baseline, verified: [baseline.VerifiedLocationIds[0], baseline.VerifiedLocationIds[0]]),
            Copy(
                baseline,
                reported: [baseline.ReportedLocationIds[0]],
                verified: [baseline.ReportedLocationIds[0]]),
            Copy(
                baseline,
                notes:
                [
                    baseline.Notes[0],
                    new MapNoteSaveEntry(baseline.Notes[0].LocationId, "Duplicate."),
                ]),
            Copy(
                baseline,
                notes: [new MapNoteSaveEntry(new LocationId("abandoned-wagon"), "Still hidden.")]),
        ];

        Assert.All(invalidStates, state =>
            Assert.Throws<ArgumentException>(() =>
                MapSaveStateRestorationProjection.Restore(state)));
        Assert.Equal(["village-square", "sickhouse"], Values(baseline.ReportedLocationIds));
        Assert.Equal(["stream", "healer-garden"], Values(baseline.VerifiedLocationIds));
        Assert.Equal(2, baseline.Notes.Count);
    }

    [Fact]
    public void RejectsInvalidNoteTextAtomically()
    {
        MapSaveState baseline = PopulatedState();
        MapSaveState blank = Copy(
            baseline,
            notes: [new MapNoteSaveEntry(baseline.ReportedLocationIds[0], " \t")]);
        MapSaveState oversized = Copy(
            baseline,
            notes:
            [
                new MapNoteSaveEntry(
                    baseline.ReportedLocationIds[0],
                    new string('n', SeekerMapNote.MaximumTextLength + 1)),
            ]);

        Assert.Throws<ArgumentException>(() =>
            MapSaveStateRestorationProjection.Restore(blank));
        Assert.Throws<ArgumentException>(() =>
            MapSaveStateRestorationProjection.Restore(oversized));
        Assert.Equal(2, baseline.Notes.Count);
    }

    [Fact]
    public void RejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() =>
            MapSaveStateRestorationProjection.Restore(null!));
    }

    [Fact]
    public void ExposesOnlyTheFreshSafeMapAndNotes()
    {
        Assert.Equal(
            ["Map", "Notes"],
            typeof(MapSaveStateRestoration)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(MapSaveStateRestoration).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.Equal(
            typeof(SeekerMapSnapshot),
            typeof(MapSaveStateRestoration).GetProperty("Map")!.PropertyType);
        Assert.Equal(
            typeof(SeekerMapNoteSnapshot),
            typeof(MapSaveStateRestoration).GetProperty("Notes")!.PropertyType);
    }

    [Fact]
    public void InstallsTheExactFreshCompositionOnlyThroughAnExplicitTarget()
    {
        MapSaveStateRestoration restored =
            MapSaveStateRestorationProjection.Restore(PopulatedState());
        RecordingTarget target = new();

        restored.InstallInto(target);

        Assert.Same(restored.Map, target.Map);
        Assert.Same(restored.Notes, target.Notes);
        Assert.NotNull(target.Catalogue);
        Assert.Contains(
            target.Catalogue.Locations,
            location => ReferenceEquals(location, restored.Map.ReportedLocations[0]));
        Assert.Throws<ArgumentNullException>(() => restored.InstallInto(null!));
    }

    private static MapSaveState PopulatedState() => new(
        new CharacterId("seeker"),
        [new LocationId("village-square"), new LocationId("sickhouse")],
        [new LocationId("stream"), new LocationId("healer-garden")],
        [
            new MapNoteSaveEntry(new LocationId("stream"), "Water level rose."),
            new MapNoteSaveEntry(
                new LocationId("sickhouse"),
                "Ask the healer about the patient."),
        ]);

    private static MapSaveState EmptyState() => new(
        new CharacterId("seeker"),
        [],
        [],
        []);

    private static MapSaveState Copy(
        MapSaveState state,
        IEnumerable<LocationId>? reported = null,
        IEnumerable<LocationId>? verified = null,
        IEnumerable<MapNoteSaveEntry>? notes = null) => new(
            state.SeekerId,
            reported ?? state.ReportedLocationIds,
            verified ?? state.VerifiedLocationIds,
            notes ?? state.Notes);

    private static string[] Values(IEnumerable<LocationId> ids) =>
        ids.Select(id => id.Value).ToArray();

    private static string[] Values(IEnumerable<MapLocationDefinition> locations) =>
        locations.Select(location => location.Id.Value).ToArray();

    private static string[] Values(IEnumerable<SeekerMapNote> notes) =>
        notes.Select(note => note.Location.Id.Value).ToArray();

    private sealed class RecordingTarget : IMapSaveStateRestorationTarget
    {
        public OpeningRegionMapLocationCatalogue? Catalogue { get; private set; }

        public SeekerMapSnapshot? Map { get; private set; }

        public SeekerMapNoteSnapshot? Notes { get; private set; }

        public void InstallRestored(
            OpeningRegionMapLocationCatalogue catalogue,
            SeekerMapSnapshot map,
            SeekerMapNoteSnapshot notes)
        {
            Catalogue = catalogue;
            Map = map;
            Notes = notes;
        }
    }
}
