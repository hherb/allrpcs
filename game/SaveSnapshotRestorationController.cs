using Allrpcs.Application.Characters;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Characters;
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

    internal IReadOnlyList<string> RegisteredOwners { get; private set; } = [];

    private FocusedSearchActivityController discoveryController = null!;
    private FirstPatientStateController firstPatientController = null!;
    private FirstPatientPreparationController firstPatientPreparation = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private OpeningRegionFirstPatientStabilizationTestimonyCoordinator patientTestimony = null!;
    private OpeningRegionSageConsultationInputCoordinator sageConsultation = null!;
    private TemporaryOrdinaryCharacterParticipantSource participants = null!;
    private InventoryController inventory = null!;
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
    private SeekerCustomizationSelectionSceneSource customization = null!;
    private SeekerCustomizationApplicationCoordinator customizationApplication = null!;
    private TemporarySeekerCharacterSource seekerCharacter = null!;
    private CharacterBody2D seeker = null!;
    private CharacterBody2D healer = null!;

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
        inventory = GetNode<InventoryController>("../InventoryController");
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
        customization = GetNode<SeekerCustomizationSelectionSceneSource>(
            "../SeekerCustomizationSelectionSceneSource");
        customizationApplication = GetNode<SeekerCustomizationApplicationCoordinator>(
            "../SeekerCustomizationApplicationCoordinator");
        seekerCharacter = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        healer = GetNode<CharacterBody2D>("../Healer");
    }

    /// <summary>Restores all exact state compositions after one complete root preflight.</summary>
    public bool TryRestore(SaveSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);

        WorldObjectSnapshot? worldObjects = worldObjectController.WorldObjects;
        WorldEventSnapshot? worldEvents = worldEventController.DefinitionsFor(snapshot.WorldEvents);
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

        SeekerCustomizationInstallation? customizationInstallation = null;
        List<(CharacterBody2D Body, Vector2 Position)> positions = [];
        if (snapshot.Continuation is { } continuation)
        {
            try
            {
                if (!SeekerCustomizationRestorationPolicy.TryDecide(
                        customization.CurrentState,
                        continuation.SeekerProfile,
                        seekerCharacter.Character.Identity.Id,
                        out SeekerCustomizationSelectionState? customizationToApply))
                {
                    GD.PushError(customization.CurrentState.SelectedProfile is null
                        ? $"Load rejected: the saved Seeker names appearance '{continuation.SeekerProfile!.AppearanceKey}' or background '{continuation.SeekerProfile.BackgroundKey}', which this build does not offer."
                        : "Load rejected: the save belongs to a different Seeker than the one customized this session.");
                    return false;
                }

                // Every scene check runs here, so the commit below only installs.
                if (customizationToApply is not null)
                {
                    customizationInstallation = customizationApplication.Prepare(
                        customization.CurrentState,
                        customizationToApply);
                    if (customizationInstallation is null)
                    {
                        GD.PushError("Load rejected: the saved Seeker profile cannot be installed in this scene.");
                        return false;
                    }
                }
            }
            catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
            {
                GD.PushError($"Load rejected: the saved Seeker profile is invalid: {exception}");
                return false;
            }

            foreach (CharacterPositionSaveState position in continuation.Positions)
            {
                CharacterBody2D? body = position.CharacterId == discoveryController.SearcherCharacterId ? seeker
                    : position.CharacterId == participants.Participants[0].Identity.Id ? healer
                    : null;
                if (body is null)
                {
                    GD.PushError($"Load rejected: no body for saved position of '{position.CharacterId.Value}'.");
                    return false;
                }
                positions.Add((body, new Vector2(position.X, position.Y)));
            }
        }

        RestoreParticipantRegistry registry = new();
        void Register(string owner, Func<PreparedRestore> prepare) =>
            registry.Register(new RestoreParticipant(owner, _ => prepare()));
        Register("discovery", () => new(
            () => discoveryController.InstallPreparedRestoration(snapshot.Discovery),
            discoveryController.PresentRestoredActivity));
        Register("world-events", () => new(
            () => worldEventController.InstallPreparedRestoration(snapshot.WorldEvents, worldEvents)));
        Register("character-memory", () => new(() => memoryController.Install(snapshot.CharacterMemory)));
        Register("first-patient", () => new(() =>
        {
            if (reboundFirstPatient is not null) { firstPatientController.Install(reboundFirstPatient); }
        }));
        Register("patient-testimony", () => new(() =>
        {
            if (reboundFirstPatient is not null)
            {
                patientTestimony.InstallPreparedRestoration(restoredRecovery?.PatientTestimony);
            }
        }, () =>
        {
            if (reboundFirstPatient is not null) { patientTestimony.PresentRestoredState(); }
        }));
        Register("sage-consultation", () => new(() =>
        {
            if (reboundFirstPatient is not null)
            {
                sageConsultation.InstallPreparedRestoration(restoredRecovery?.SageConsultation);
            }
        }, () =>
        {
            if (reboundFirstPatient is not null) { sageConsultation.PresentRestoredState(); }
        }));
        Register("journal", () => new(() =>
        {
            if (restoredJournal is not null) { journalController.InstallRestored(restoredJournal.Journal); }
        }));
        Register("journal-selection", () => new(() =>
        {
            if (restoredJournal is not null) { journalSelection.InstallRestored(restoredJournal.SelectedSection); }
        }));
        Register("map", () => new(() =>
        {
            if (restoredMap is not null) { mapController.InstallPreparedRestoration(restoredMap); }
        }));
        Register("route-progress", () => new(() =>
        {
            if (restoredRoutes is not null) { routeProgress.InstallPreparedRestoration(restoredRoutes.Progress); }
        }));
        Register("route-completions", () => new(() =>
        {
            if (restoredRoutes is not null)
            {
                routeCompletions.InstallPreparedRestoration(restoredRoutes.AcceptedCompletions);
            }
        }));
        Register("route-consumption", () => new(() =>
        {
            if (restoredRoutes is not null)
            {
                routeConsumption.InstallPreparedRestoration(restoredRoutes.ConsumedCompletion);
            }
        }));
        Register("seeker-customization", () => new(() =>
        {
            if (customizationInstallation is not null) { customizationApplication.Install(customizationInstallation); }
        }));
        Register("character-positions", () => new(() =>
        {
            foreach ((CharacterBody2D body, Vector2 position) in positions)
            {
                body.GlobalPosition = position;
                body.Velocity = Vector2.Zero;
            }
        }));
        Register("inventory", () => new(inventory.PrepareRestoration(snapshot.Inventory)));
        GetParent<Main>().RegisterSessionRestoreParticipants(registry);
        // Deliberately last: authored preparation can reject an otherwise valid projected root.
        Register("first-patient-preparation", () => new(reboundFirstPatient is null
            ? () => { }
        : firstPatientPreparation.PrepareRestoration(reboundFirstPatient)));
        RegisteredOwners = registry.Owners;
        if (!registry.TryRestore(snapshot)) { return false; }

        foreach (Delegate listener in PresentationReset?.GetInvocationList() ?? [])
        {
            try { ((Action)listener)(); }
            catch (Exception exception) { GD.PrintErr($"Restoration presentation failed: {exception.Message}"); }
        }
        return true;
    }
}
