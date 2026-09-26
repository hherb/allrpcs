namespace Allrpcs.Application.Map;

/// <summary>Projects exact current map and note state without exposing hidden locations.</summary>
public static class SeekerMapPresentationProjection
{
    /// <summary>Returns one safe presentation snapshot in canonical known-location order.</summary>
    public static SeekerMapPresentationSnapshot Project(
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(notes);

        if (!ReferenceEquals(map.Catalogue, notes.Catalogue)
            || !ReferenceEquals(map.SeekerId, notes.SeekerId))
        {
            throw new ArgumentException(
                "Map presentation requires exact shared catalogue and owner provenance.",
                nameof(notes));
        }

        Dictionary<MapLocationDefinition, SeekerMapNote> notesByLocation =
            new(ReferenceEqualityComparer.Instance);
        foreach (SeekerMapNote note in notes.Notes)
        {
            if (!IsKnown(map, note.Location) || !notesByLocation.TryAdd(note.Location, note))
            {
                throw new ArgumentException(
                    "Map presentation cannot retain notes for hidden or duplicate locations.",
                    nameof(notes));
            }
        }

        IEnumerable<SeekerMapPresentationEntry> locations = map.Catalogue.Locations
            .Where(location => IsKnown(map, location))
            .Select(location => new SeekerMapPresentationEntry(
                location,
                Contains(map.VerifiedLocations, location),
                notesByLocation.GetValueOrDefault(location)?.Text));
        return new SeekerMapPresentationSnapshot(map, notes, locations);
    }

    private static bool IsKnown(SeekerMapSnapshot map, MapLocationDefinition location) =>
        Contains(map.ReportedLocations, location)
        || Contains(map.VerifiedLocations, location);

    private static bool Contains(
        IEnumerable<MapLocationDefinition> locations,
        MapLocationDefinition location) => locations.Any(candidate => ReferenceEquals(candidate, location));
}
