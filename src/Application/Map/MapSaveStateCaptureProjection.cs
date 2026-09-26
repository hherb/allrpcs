using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Map;

/// <summary>Captures exact same-Seeker safe map state as closed persistence values.</summary>
public static class MapSaveStateCaptureProjection
{
    /// <summary>Returns one fresh immutable save state from exact current map snapshots.</summary>
    public static MapSaveState Capture(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(notes);
        RequireExactSafeState(map, notes);

        return new MapSaveState(
            map.SeekerId,
            map.ReportedLocations.Select(location => location.Id),
            map.VerifiedLocations.Select(location => location.Id),
            notes.Notes.Select(note => new MapNoteSaveEntry(note.Location.Id, note.Text)));
    }

    private static void RequireExactSafeState(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        if (!ReferenceEquals(map.Catalogue, notes.Catalogue)
            || !ReferenceEquals(map.SeekerId, notes.SeekerId)
            || !HasExactUniqueMembership(map.Catalogue, map.ReportedLocations)
            || !HasExactUniqueMembership(map.Catalogue, map.VerifiedLocations)
            || map.ReportedLocations.Any(reported =>
                map.VerifiedLocations.Any(verified => ReferenceEquals(reported, verified)))
            || !HasExactSafeNotes(map, notes))
        {
            throw new ArgumentException(
                "Map persistence requires exact same-Seeker safe map and note state.",
                nameof(notes));
        }
    }

    private static bool HasExactUniqueMembership(
        OpeningRegionMapLocationCatalogue catalogue,
        IEnumerable<MapLocationDefinition> locations)
    {
        HashSet<MapLocationDefinition> retained = new(ReferenceEqualityComparer.Instance);
        return locations.All(location =>
            location is not null
            && catalogue.Locations.Any(candidate => ReferenceEquals(candidate, location))
            && retained.Add(location));
    }

    private static bool HasExactSafeNotes(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        HashSet<MapLocationDefinition> retained = new(ReferenceEqualityComparer.Instance);
        return notes.Notes.All(note =>
            note is not null
            && note.Location is not null
            && retained.Add(note.Location)
            && IsKnown(map, note.Location)
            && !string.IsNullOrWhiteSpace(note.Text)
            && note.Text.Length <= SeekerMapNote.MaximumTextLength);
    }

    private static bool IsKnown(
        SeekerMapSnapshot map,
        MapLocationDefinition location) =>
        map.ReportedLocations.Any(candidate => ReferenceEquals(candidate, location))
        || map.VerifiedLocations.Any(candidate => ReferenceEquals(candidate, location));
}
