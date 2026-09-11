using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates atomic in-memory restoration of one eligible versioned save root.</summary>
public partial class SaveSnapshotRestorationController : Node
{
    private FocusedSearchActivityController discoveryController = null!;
    private FirstPatientStateController firstPatientController = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private CharacterMemoryController memoryController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        firstPatientController = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
    }

    /// <summary>Restores all exact state compositions after one complete root preflight.</summary>
    public bool TryRestore(SaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        WorldObjectSnapshot? worldObjects = worldObjectController.WorldObjects;
        WorldEventSnapshot? worldEvents = worldEventController.WorldEvents;
        if (worldObjects is null
            || worldEvents is null
            || !SaveSnapshotRestorationPolicy.CanRestore(
                snapshot,
                discoveryController.SearcherCharacterId,
                worldObjects,
                worldEvents,
                firstPatientContextSource.Context))
        {
            return false;
        }

        FirstPatientSaveState? reboundFirstPatient = null;
        if (snapshot.FirstPatient is not null)
        {
            try
            {
                reboundFirstPatient = FirstPatientSaveStateRestorationTransition.Rebind(
                    snapshot.FirstPatient,
                    firstPatientContextSource.Context,
                    worldEvents);
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        bool discoveryRestored = discoveryController.TryRestoreDiscoveryState(
            snapshot.Discovery,
            worldObjects);
        bool worldEventsRestored = worldEventController.TryRestoreWorldEventState(
            snapshot.WorldEvents);
        memoryController.Install(snapshot.CharacterMemory);
        if (reboundFirstPatient is not null)
        {
            firstPatientController.Install(reboundFirstPatient);
        }
        if (!discoveryRestored || !worldEventsRestored)
        {
            throw new InvalidOperationException(
                "A preflighted save root could not be installed into its controllers.");
        }

        return true;
    }
}
