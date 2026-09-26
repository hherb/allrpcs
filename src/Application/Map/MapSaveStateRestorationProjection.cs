using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Map;

/// <summary>Restores closed map persistence values against fresh canonical definitions.</summary>
public static class MapSaveStateRestorationProjection
{
    /// <summary>Returns fresh safe map and note snapshots after complete compatibility preflight.</summary>
    public static MapSaveStateRestoration Restore(MapSaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(state.SeekerId);

        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        Dictionary<LocationId, MapLocationDefinition> locationsById = catalogue.Locations
            .ToDictionary(location => location.Id);
        HashSet<LocationId> reported = ValidateMembership(
            state.ReportedLocationIds,
            locationsById);
        HashSet<LocationId> verified = ValidateMembership(
            state.VerifiedLocationIds,
            locationsById);
        if (reported.Overlaps(verified))
        {
            throw Incompatible();
        }

        Dictionary<LocationId, string> noteTextByLocation = ValidateNotes(
            state.Notes,
            locationsById,
            reported,
            verified);

        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            state.SeekerId,
            catalogue.Locations.Where(location => reported.Contains(location.Id)),
            catalogue.Locations.Where(location => verified.Contains(location.Id)));
        SeekerMapNoteSnapshot notes = new(
            catalogue,
            state.SeekerId,
            catalogue.Locations
                .Where(location => noteTextByLocation.ContainsKey(location.Id))
                .Select(location => new SeekerMapNote(
                    location,
                    noteTextByLocation[location.Id])));
        return new MapSaveStateRestoration(map, notes);
    }

    private static HashSet<LocationId> ValidateMembership(
        IEnumerable<LocationId> locationIds,
        Dictionary<LocationId, MapLocationDefinition> locationsById)
    {
        HashSet<LocationId> retained = [];
        foreach (LocationId? locationId in locationIds)
        {
            if (locationId is null
                || !locationsById.ContainsKey(locationId)
                || !retained.Add(locationId))
            {
                throw Incompatible();
            }
        }

        return retained;
    }

    private static Dictionary<LocationId, string> ValidateNotes(
        IEnumerable<MapNoteSaveEntry> notes,
        Dictionary<LocationId, MapLocationDefinition> locationsById,
        HashSet<LocationId> reported,
        HashSet<LocationId> verified)
    {
        Dictionary<LocationId, string> retained = [];
        foreach (MapNoteSaveEntry? note in notes)
        {
            if (note is null
                || note.LocationId is null
                || !locationsById.ContainsKey(note.LocationId)
                || (!reported.Contains(note.LocationId) && !verified.Contains(note.LocationId))
                || string.IsNullOrWhiteSpace(note.Text)
                || note.Text.Length > SeekerMapNote.MaximumTextLength
                || !retained.TryAdd(note.LocationId, note.Text))
            {
                throw Incompatible();
            }
        }

        return retained;
    }

    private static ArgumentException Incompatible() => new(
        "Saved map state must contain canonical, unique, same-owner visibility-safe values.",
        "state");
}
