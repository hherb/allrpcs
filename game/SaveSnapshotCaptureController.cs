using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Captures the scene's exact initialized dynamic state as an in-memory save root.</summary>
public partial class SaveSnapshotCaptureController : Node
{
    private FocusedSearchActivityController discoveryController = null!;
    private FirstPatientStateController firstPatientController = null!;
    private CharacterMemoryController memoryController = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private WorldEventController worldEventController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        firstPatientController = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        journalController = GetNode<JournalController>("../JournalController");
        journalSelection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
        mapController = GetNode<SeekerMapController>("../SeekerMapController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>
    /// Captures fresh save-state compositions retaining the exact initialized live snapshots.
    /// </summary>
    public SaveSnapshot? Capture()
    {
        CharacterPerceptionSnapshot? perception = discoveryController.Perception;
        CharacterMemorySnapshot memory = memoryController.Memory;
        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        FirstPatientSaveState? firstPatient = firstPatientController.Capture();
        JournalSnapshot? journal = journalController.Journal;
        SeekerMapSnapshot? map = mapController.Map;
        SeekerMapNoteSnapshot? mapNotes = mapController.Notes;
        if (perception is null
            || worldEvents is null
            || eventLog is null
            || worldTime is null
            || firstPatient is null
            || journal is null
            || journal.SeekerId != perception.CharacterId
            || map is null
            || mapNotes is null
            || map.SeekerId != perception.CharacterId
            || mapNotes.SeekerId != perception.CharacterId)
        {
            return null;
        }

        JournalSaveState journalState;
        MapSaveState mapState;
        try
        {
            journalState = JournalSaveStateCaptureProjection.Capture(
                journal,
                journalSelection.Selection);
            mapState = MapSaveStateCaptureProjection.Capture(map, mapNotes);
        }
        catch (ArgumentException)
        {
            return null;
        }

        DiscoverySaveState discovery = new(
            perception,
            discoveryController.Activities,
            discoveryController.Outcomes,
            discoveryController.PassiveNoticingCandidates,
            discoveryController.ObservationReinterpretationCandidates);
        WorldEventSaveState eventState = new(eventLog, worldTime);
        return new SaveSnapshot(
            SaveSchemaVersion.Map,
            discovery,
            eventState,
            memory,
            firstPatient,
            journalState,
            mapState);
    }
}
