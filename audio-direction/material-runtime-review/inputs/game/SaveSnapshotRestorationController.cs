using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Coordinates atomic in-memory restoration of one eligible versioned save root.</summary>
public partial class SaveSnapshotRestorationController : Node
{
    /// <summary>Resets transient presentation only after a complete successful restore.</summary>
    public event Action? PresentationReset;

    private FocusedSearchActivityController discoveryController = null!;
    private FirstPatientStateController firstPatientController = null!;
    private FirstPatientPreparationController firstPatientPreparation = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator patientTestimony = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
    private TemporaryOrdinaryCharacterParticipantSource participants = null!;
    private CharacterMemoryController memoryController = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator routeConsumption = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        firstPatientController = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        firstPatientPreparation = GetNode<FirstPatientPreparationController>(
            "../FirstPatientPreparationController");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        patientTestimony = GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
            "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        sageConsultation = GetNode<OpeningRegionSageConsultationInputCoordinator>(
            "../OpeningRegionSageConsultationInputCoordinator");
        participants = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        journalController = GetNode<JournalController>("../JournalController");
        journalSelection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
        mapController = GetNode<SeekerMapController>("../SeekerMapController");
        shrineSource = GetNode<OpeningRegionOldShrineSceneSource>(
            "../OpeningRegionOldShrineSceneSource");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routeCompletions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
        routeConsumption = GetNode<OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
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
        FirstPatientRecoveryRestoration? restoredRecovery = null;
        JournalSaveStateRestoration? restoredJournal = null;
        MapSaveStateRestoration? restoredMap = null;
        InvestigationRouteSaveStateRestoration? restoredRoutes = null;
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

            try
            {
                restoredRecovery = FirstPatientRecoveryRestorationProjection.Restore(
                    snapshot.FirstPatientRecovery ?? FirstPatientRecoveryStage.NotHeard,
                    firstPatientContextSource.Context,
                    reboundFirstPatient.DiseaseTruth,
                    reboundFirstPatient.AdministrationExperiments,
                    reboundFirstPatient.AdministrationExperiments.Records.Count == 0
                        ? null
                        : reboundFirstPatient.AdministrationExperiments.Records[^1]
                            .Administration.WorldEvent,
                    snapshot.WorldEvents.EventLog,
                    participants.Participants[1],
                    discoveryController.SearcherCharacterId);
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

        if (snapshot.Journal is not null)
        {
            try
            {
                restoredJournal = JournalSaveStateRestorationProjection.Restore(snapshot.Journal);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        if (snapshot.Map is not null)
        {
            try
            {
                restoredMap = MapSaveStateRestorationProjection.Restore(snapshot.Map);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        if (snapshot.InvestigationRoutes is not null)
        {
            if (restoredMap is null)
            {
                return false;
            }

            try
            {
                restoredRoutes = InvestigationRouteSaveStateRestorationProjection.Restore(
                    snapshot.InvestigationRoutes,
                    restoredMap.Catalogue,
                    shrineSource.DiscoveryScenario);
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
            firstPatientPreparation.InstallRestored(reboundFirstPatient);
            patientTestimony.InstallRestored(restoredRecovery?.PatientTestimony);
            sageConsultation.InstallRestored(restoredRecovery?.SageConsultation);
        }
        if (restoredJournal is not null)
        {
            journalController.InstallRestored(restoredJournal.Journal);
            journalSelection.InstallRestored(restoredJournal.SelectedSection);
        }
        if (restoredMap is not null)
        {
            restoredMap.InstallInto(mapController);
        }
        if (restoredRoutes is not null)
        {
            routeProgress.InstallRestored(restoredRoutes.Progress);
            routeCompletions.InstallRestored(
                restoredRoutes.Progress,
                restoredRoutes.AcceptedCompletions);
            routeConsumption.InstallRestored(restoredRoutes.ConsumedCompletion);
        }
        if (!discoveryRestored || !worldEventsRestored)
        {
            throw new InvalidOperationException(
                "A preflighted save root could not be installed into its controllers.");
        }

        foreach (Delegate listener in PresentationReset?.GetInvocationList() ?? [])
        {
            try { ((Action)listener)(); }
            catch (Exception exception) { GD.PrintErr($"Restoration presentation failed: {exception.Message}"); }
        }
        return true;
    }
}
