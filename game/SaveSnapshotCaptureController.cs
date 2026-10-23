using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Captures the scene's exact initialized dynamic state as an in-memory save root.</summary>
public partial class SaveSnapshotCaptureController : Node
{
    private FocusedSearchActivityController discoveryController = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator patientTestimony = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
    private FirstPatientStateController firstPatientController = null!;
    private CharacterMemoryController memoryController = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator routeConsumption = null!;
    private WorldEventController worldEventController = null!;
    private SeekerCustomizationSelectionSceneSource customization = null!;
    private TemporaryOrdinaryCharacterParticipantSource participants = null!;
    private Node2D seeker = null!;
    private Node2D healer = null!;
    private InventoryController inventory = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        patientTestimony = GetNode<OpeningRegionFirstPatientStabilizationTestimonyCoordinator>(
            "../OpeningRegionFirstPatientStabilizationTestimonyCoordinator");
        sageConsultation = GetNode<OpeningRegionSageConsultationInputCoordinator>(
            "../OpeningRegionSageConsultationInputCoordinator");
        firstPatientController = GetNode<FirstPatientStateController>("../FirstPatientStateController");
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
        inventory = GetNode<InventoryController>("../InventoryController");
        customization = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../SeekerCustomizationSelectionSceneSource");
        participants = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        seeker = GetNode<Node2D>("../Seeker");
        healer = GetNode<Node2D>("../Healer");
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
        OpeningRegionMapLocationCatalogue? mapCatalogue = mapController.Catalogue;
        OpeningRegionInvestigationRouteProgressSnapshot? routes = routeProgress.Progress;
        // Before the map is first opened, none of its or the routes' state exists; that saves as null.
        bool mapUnopened = map is null
            && mapNotes is null
            && mapCatalogue is null
            && routes is null
            && routeProgress.Catalogue is null;
        if (perception is null
            || worldEvents is null
            || eventLog is null
            || worldTime is null
            || firstPatient is null
            || journal is null
            || journal.SeekerId != perception.CharacterId
            || (!mapUnopened
                && (map is null
                    || mapNotes is null
                    || mapCatalogue is null
                    || routes is null
                    || !ReferenceEquals(routeProgress.Catalogue, routes.Catalogue)
                    || !OpeningRegionInvestigationRouteValidationPolicy.IsValid(
                        routes.Catalogue,
                        mapCatalogue,
                        shrineSource.DiscoveryScenario)
                    || map.SeekerId != perception.CharacterId
                    || mapNotes.SeekerId != perception.CharacterId)))
        {
            return null;
        }

        // A live save always records the inventory; only a pre-inventory save migrates without one.
        if (inventory.Ownerships is not { } ownerships)
        {
            return null;
        }

        JournalSaveState journalState;
        MapSaveState? mapState = null;
        InvestigationRouteSaveState? routeState = null;
        FirstPatientRecoveryStage recoveryStage;
        try
        {
            journalState = JournalSaveStateCaptureProjection.Capture(
                journal,
                journalSelection.Selection);
            journalState = FirstPatientRecoveryJournalSaveStateProjection.Project(
                journalState,
                patientTestimony.Testimony,
                sageConsultation.Consultation);
            if (!mapUnopened)
            {
                mapState = MapSaveStateCaptureProjection.Capture(map!, mapNotes!);
                routeState = InvestigationRouteSaveStateCaptureProjection.Capture(
                    routes!,
                    routeCompletions.AcceptedCompletions,
                    routeConsumption.ConsumedCompletion);
            }
            recoveryStage = CaptureRecoveryStage();
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
        SaveContinuationState continuation = new(
            customization.CurrentState.SelectedProfile is { } profile
                ? SeekerCustomizationSaveState.From(profile)
                : null,
            [
                Position(perception.CharacterId, seeker),
                Position(participants.Participants[0].Identity.Id, healer),
            ]);
        return new SaveSnapshot(
            SaveSchemaVersion.OptionalMap,
            discovery,
            eventState,
            memory,
            firstPatient,
            journalState,
            mapState,
            routeState,
            recoveryStage,
            continuation,
            ownerships);
    }

    private static CharacterPositionSaveState Position(CharacterId characterId, Node2D body) =>
        new(characterId, body.GlobalPosition.X, body.GlobalPosition.Y);

    private FirstPatientRecoveryStage CaptureRecoveryStage()
    {
        if (sageConsultation.Consultation is not null)
        {
            if (patientTestimony.Testimony is null)
            {
                throw new InvalidOperationException(
                    "Sage consultation cannot precede the patient testimony.");
            }

            return FirstPatientRecoveryStage.SageConsulted;
        }

        return patientTestimony.Testimony is null
            ? FirstPatientRecoveryStage.NotHeard
            : FirstPatientRecoveryStage.PatientTestimonyHeard;
    }
}
