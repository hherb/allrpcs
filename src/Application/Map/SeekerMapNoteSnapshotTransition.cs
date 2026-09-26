using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Sets or removes bounded player notes for exact visible map locations.</summary>
public static class SeekerMapNoteSnapshotTransition
{
    /// <summary>Returns a fresh note snapshot with one known location's note added or replaced.</summary>
    public static SeekerMapNoteSnapshot Set(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes,
        CharacterId seekerId,
        MapLocationDefinition location,
        string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        ValidateProvenance(catalogue, map, notes, seekerId, location);
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException(
                "A player map note must contain non-whitespace text.",
                nameof(text));
        }

        if (text.Length > SeekerMapNote.MaximumTextLength)
        {
            throw new ArgumentOutOfRangeException(nameof(text));
        }

        Dictionary<MapLocationDefinition, SeekerMapNote> notesByLocation = CopyNotes(notes);
        notesByLocation[location] = new SeekerMapNote(location, text);
        return Compose(catalogue, seekerId, notesByLocation);
    }

    /// <summary>Returns a fresh note snapshot without one exact existing location note.</summary>
    public static SeekerMapNoteSnapshot Remove(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes,
        CharacterId seekerId,
        MapLocationDefinition location)
    {
        ValidateProvenance(catalogue, map, notes, seekerId, location);

        Dictionary<MapLocationDefinition, SeekerMapNote> notesByLocation = CopyNotes(notes);
        if (!notesByLocation.Remove(location))
        {
            throw new ArgumentException(
                "Only an existing exact map-location note may be removed.",
                nameof(location));
        }

        return Compose(catalogue, seekerId, notesByLocation);
    }

    private static SeekerMapNoteSnapshot Compose(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId,
        IReadOnlyDictionary<MapLocationDefinition, SeekerMapNote> notesByLocation) => new(
            catalogue,
            seekerId,
            catalogue.Locations
                .Where(notesByLocation.ContainsKey)
                .Select(location => notesByLocation[location]));

    private static Dictionary<MapLocationDefinition, SeekerMapNote> CopyNotes(
        SeekerMapNoteSnapshot notes)
    {
        Dictionary<MapLocationDefinition, SeekerMapNote> notesByLocation =
            new(ReferenceEqualityComparer.Instance);
        foreach (SeekerMapNote note in notes.Notes)
        {
            notesByLocation.Add(note.Location, note);
        }

        return notesByLocation;
    }

    private static void ValidateProvenance(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes,
        CharacterId seekerId,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(notes);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(location);

        bool knownLocation = IsKnown(map, location);
        bool retainedNotesKnown = notes.Notes.All(note =>
            catalogue.Locations.Any(candidate => ReferenceEquals(candidate, note.Location))
            && IsKnown(map, note.Location));
        if (!ReferenceEquals(map.Catalogue, catalogue)
            || !ReferenceEquals(notes.Catalogue, catalogue)
            || !ReferenceEquals(map.SeekerId, seekerId)
            || !ReferenceEquals(notes.SeekerId, seekerId)
            || !catalogue.Locations.Any(candidate => ReferenceEquals(candidate, location))
            || !knownLocation
            || !retainedNotesKnown)
        {
            throw new ArgumentException(
                "Map notes require exact catalogue, owner, and visible-location provenance.",
                nameof(location));
        }
    }

    private static bool IsKnown(
        SeekerMapSnapshot map,
        MapLocationDefinition location) =>
        map.ReportedLocations.Any(candidate => ReferenceEquals(candidate, location))
        || map.VerifiedLocations.Any(candidate => ReferenceEquals(candidate, location));
}
