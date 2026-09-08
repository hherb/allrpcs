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
    private WorldEventController worldEventController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <summary>
    /// Captures fresh save-state compositions retaining the exact initialized live snapshots.
    /// </summary>
    public SaveSnapshot? Capture()
    {
        CharacterPerceptionSnapshot? perception = discoveryController.Perception;
        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? eventLog = worldEventController.EventLog;
        WorldTimeSnapshot? worldTime = worldEventController.WorldTime;
        if (perception is null
            || worldEvents is null
            || eventLog is null
            || worldTime is null)
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
            SaveSchemaVersion.FirstSupported,
            discovery,
            eventState);
    }
}
