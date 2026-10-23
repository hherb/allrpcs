using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact opening-region catalogue and one Seeker's safe in-memory map state.</summary>
public partial class SeekerMapController : Node, IMapSaveStateRestorationTarget
{
    /// <summary>Gets the exact initialized complete catalogue, or null before initialization.</summary>
    public OpeningRegionMapLocationCatalogue? Catalogue { get; private set; }

    /// <summary>Gets the exact current visibility-safe Seeker map, or null before initialization.</summary>
    public SeekerMapSnapshot? Map { get; private set; }

    /// <summary>Gets the exact current player-authored map notes, or null before initialization.</summary>
    public SeekerMapNoteSnapshot? Notes { get; private set; }

    /// <summary>Atomically initializes one fresh catalogue, empty map, and empty notes once.</summary>
    public bool TryInitialize(CharacterId seekerId)
    {
        ArgumentNullException.ThrowIfNull(seekerId);

        if (Catalogue is not null || Map is not null || Notes is not null)
        {
            return false;
        }

        OpeningRegionMapLocationCatalogue catalogue =
            RepresentativeOpeningRegionMapLocationSource.Create();
        SeekerMapSnapshot map = SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            [],
            []);
        SeekerMapNoteSnapshot notes = SeekerMapNoteSnapshotProjection.CreateEmpty(
            catalogue,
            seekerId);

        Catalogue = catalogue;
        Map = map;
        Notes = notes;
        return true;
    }

    void IMapSaveStateRestorationTarget.InstallRestored(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        SeekerMapNoteSnapshot notes)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(notes);

        _ = MapSaveStateCaptureProjection.Capture(map, notes);
        bool hasExactCatalogue = map.ReportedLocations
            .Concat(map.VerifiedLocations)
            .All(location => catalogue.Locations.Any(candidate => ReferenceEquals(candidate, location)))
            && notes.Notes.All(note =>
                catalogue.Locations.Any(candidate => ReferenceEquals(candidate, note.Location)));
        if (!hasExactCatalogue)
        {
            throw new ArgumentException(
                "Restored map definitions must belong to the supplied exact catalogue.",
                nameof(catalogue));
        }

        Catalogue = catalogue;
        Map = map;
        Notes = notes;
    }

    /// <summary>Returns to the never-opened state a load of a save taken before the map was opened needs.</summary>
    internal void ResetToUnopened()
    {
        Catalogue = null;
        Map = null;
        Notes = null;
    }

    internal void InstallPreparedRestoration(MapSaveStateRestoration restored)
    {
        Catalogue = restored.Catalogue;
        Map = restored.Map;
        Notes = restored.Notes;
    }

    /// <summary>Reports one exact location from exact current map and already-safe testimony.</summary>
    public bool TryReport(
        SeekerMapSnapshot expectedMap,
        JournalTestimonySectionSnapshot testimony,
        JournalTestimonyAuthoredCopy sourceEntry,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(testimony);
        ArgumentNullException.ThrowIfNull(sourceEntry);
        ArgumentNullException.ThrowIfNull(location);

        if (Catalogue is not { } catalogue
            || Notes is null
            || !ReferenceEquals(Map, expectedMap))
        {
            return false;
        }

        SeekerMapSnapshot result;
        try
        {
            result = SeekerMapTestimonyReportProjection.Project(
                catalogue,
                expectedMap,
                testimony,
                expectedMap.SeekerId,
                sourceEntry,
                location);
        }
        catch (ArgumentException)
        {
            return false;
        }

        Map = result;
        return true;
    }

    /// <summary>Verifies one exact location from an exact current map and coordinate-free visit.</summary>
    public bool TryVerifyVisit(
        SeekerMapSnapshot expectedMap,
        OpeningRegionProjection projection,
        OpeningRegionLocationVisit visit,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(visit);
        ArgumentNullException.ThrowIfNull(location);

        if (Catalogue is not { } catalogue
            || Notes is null
            || !ReferenceEquals(Map, expectedMap))
        {
            return false;
        }

        SeekerMapSnapshot result;
        try
        {
            result = SeekerMapVisitVerificationProjection.Project(
                catalogue,
                expectedMap,
                projection,
                visit,
                location);
        }
        catch (ArgumentException)
        {
            return false;
        }

        Map = result;
        return true;
    }

    /// <summary>Sets one bounded note only from exact current map and note snapshots.</summary>
    public bool TrySetNote(
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes,
        MapLocationDefinition location,
        string text)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedNotes);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(text);

        if (Catalogue is not { } catalogue
            || !ReferenceEquals(Map, expectedMap)
            || !ReferenceEquals(Notes, expectedNotes))
        {
            return false;
        }

        SeekerMapNoteSnapshot result;
        try
        {
            result = SeekerMapNoteSnapshotTransition.Set(
                catalogue,
                expectedMap,
                expectedNotes,
                expectedMap.SeekerId,
                location,
                text);
        }
        catch (ArgumentException)
        {
            return false;
        }

        Notes = result;
        return true;
    }

    /// <summary>Removes one exact existing note only from exact current map and note snapshots.</summary>
    public bool TryRemoveNote(
        SeekerMapSnapshot expectedMap,
        SeekerMapNoteSnapshot expectedNotes,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(expectedNotes);
        ArgumentNullException.ThrowIfNull(location);

        if (Catalogue is not { } catalogue
            || !ReferenceEquals(Map, expectedMap)
            || !ReferenceEquals(Notes, expectedNotes))
        {
            return false;
        }

        SeekerMapNoteSnapshot result;
        try
        {
            result = SeekerMapNoteSnapshotTransition.Remove(
                catalogue,
                expectedMap,
                expectedNotes,
                expectedMap.SeekerId,
                location);
        }
        catch (ArgumentException)
        {
            return false;
        }

        Notes = result;
        return true;
    }
}
