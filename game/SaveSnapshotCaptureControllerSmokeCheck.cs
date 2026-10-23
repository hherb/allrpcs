using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free exact in-memory save capture checks when enabled.</summary>
public partial class SaveSnapshotCaptureControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SAVE_SNAPSHOT_CAPTURE_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private SaveSnapshotCaptureController controller = null!;
    private FirstPatientStateController firstPatientController = null!;
    private CharacterMemoryController memoryController = null!;
    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator routeConsumption = null!;
    private SaveSnapshotRestorationController restorationController = null!;
    private FocusedSearchActivityController discoveryController = null!;
    private WorldEventController worldEventController = null!;
    private Line2D activityIndicator = null!;
    private CharacterBody2D seeker = null!;
    private Label focusStatus = null!;
    private Label focusedSearchResult = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
        firstPatientController = GetNode<FirstPatientStateController>("../FirstPatientStateController");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        journalComposition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
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
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        activityIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        CharacterPerceptionSnapshot perception = discoveryController.Perception
            ?? throw new InvalidOperationException("Discovery perception was not initialized.");
        FocusedSearchActivitySnapshot activities = discoveryController.Activities;
        FocusedSearchOutcomeSnapshot outcomes = discoveryController.Outcomes;
        PassiveNoticingCandidateSnapshot passiveCandidates =
            discoveryController.PassiveNoticingCandidates;
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates =
            discoveryController.ObservationReinterpretationCandidates;
        WorldEventSnapshot definitions = worldEventController.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        WorldEventLogSnapshot eventLog = worldEventController.EventLog
            ?? throw new InvalidOperationException("World-event log was not initialized.");
        WorldTimeSnapshot worldTime = worldEventController.WorldTime
            ?? throw new InvalidOperationException("World time was not initialized.");
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        bool expectedActivityIndicator = activityIndicator.Visible;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;
        CharacterMemorySnapshot memory =
            CharacterMemoryControllerSmokeCheck.CreateMemory("healer");
        memoryController.Install(memory);
        if (!journalComposition.TryComposeAndInitialize()
            || !journalSelection.TrySelect(
                journalController.Journal!,
                journalController.Journal!.Catalogue.Sections[2]))
        {
            Fail("Save capture smoke could not initialize exact journal state.");
            return;
        }
        MapSaveStateRestorationProjection.Restore(PopulatedMapState()).InstallInto(mapController);
        if (!SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(
                routeProgress,
                mapController,
                shrineSource))
        {
            Fail("Save capture smoke could not initialize exact route state.");
            return;
        }

        SaveSnapshot first = controller.Capture()
            ?? throw new InvalidOperationException("Initialized state was not captured.");
        SaveSnapshot second = controller.Capture()
            ?? throw new InvalidOperationException("Repeated initialized state was not captured.");

        if (ReferenceEquals(first, second)
            || ReferenceEquals(first.Discovery, second.Discovery)
            || ReferenceEquals(first.WorldEvents, second.WorldEvents)
            || !ReferenceEquals(first.SchemaVersion, SaveSchemaVersion.OptionalMap)
            || !ReferenceEquals(second.SchemaVersion, SaveSchemaVersion.OptionalMap)
            || first.Inventory is null
            || !ReferenceEquals(first.Inventory, second.Inventory)
            || !ReferenceEquals(
                first.FirstPatientRecovery,
                FirstPatientRecoveryStage.NotHeard)
            || !ReferenceEquals(
                second.FirstPatientRecovery,
                FirstPatientRecoveryStage.NotHeard)
            || ReferenceEquals(first.FirstPatient, second.FirstPatient)
            || ReferenceEquals(first.Journal, second.Journal)
            || ReferenceEquals(first.Map, second.Map)
            || !HasExactNestedState(first)
            || !HasExactNestedState(second))
        {
            Fail("Save capture smoke failed exact-instance or repeated-root composition.");
            return;
        }

        if (!RejectsEveryUninitializedState(definitions, eventLog, worldTime)
            || !HasExactLiveState(
                perception,
                activities,
                outcomes,
                passiveCandidates,
                reinterpretationCandidates,
                definitions,
                eventLog,
                worldTime))
        {
            Fail("Save capture smoke failed uninitialized rejection or state preservation.");
            return;
        }

        if (!restorationController.TryRestore(first)
            || !HasExactNonFirstPatientState(first)
            || !HasRestoredFirstPatientState(first.FirstPatient!)
            || !HasExactLiveState(
                perception,
                activities,
                outcomes,
                passiveCandidates,
                reinterpretationCandidates,
                definitions,
                eventLog,
                worldTime)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || activityIndicator.Visible != expectedActivityIndicator
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Save capture smoke failed restoration or unrelated-state preservation.");
            return;
        }

        GD.Print(
            "Save capture controller smoke passed: exact live discovery, world-event, and character-"
            + "memory snapshots, fresh repeated current-schema roots, fail-closed uninitialized "
            + "discovery perception, authored events, event log, qualitative time, journal, and "
            + "map and route state, safe journal, exact selection, populated map, and exact empty "
            + "route-lifecycle capture, restoration "
            + "through the existing "
            + "coordinator, and unchanged controllers, movement, input, and presentation were "
            + "correct.");
        GetTree().Quit();
    }

    private bool HasExactNestedState(SaveSnapshot snapshot) =>
        HasExactNonFirstPatientState(snapshot)
        && snapshot.FirstPatient is not null
        && snapshot.Journal is not null
        && snapshot.Map is not null
        && snapshot.InvestigationRoutes is not null
        && snapshot.Journal.SeekerId == snapshot.Discovery.Perception.CharacterId
        && string.Equals(snapshot.Journal.SelectedSectionKey, "hypotheses", StringComparison.Ordinal)
        && snapshot.Journal.Observations.Count == journalController.Journal!.Observations.Entries.Count
        && snapshot.Journal.Testimony.Count == journalController.Journal.Testimony!.Entries.Count
        && snapshot.Journal.Hypotheses.Count == journalController.Journal.Hypotheses!.Entries.Count
        && snapshot.Journal.Experiments.Count == journalController.Journal.Experiments!.Entries.Count
        && snapshot.Journal.OpenQuestions.Count == journalController.Journal.OpenQuestions!.Entries.Count
        && snapshot.Map.SeekerId == snapshot.Discovery.Perception.CharacterId
        && snapshot.Map.ReportedLocationIds.Single().Value == "stream"
        && snapshot.Map.VerifiedLocationIds.Single().Value == "old-shrine"
        && snapshot.Map.Notes.Single().Text == "Inspect the layered marks."
        && ReferenceEquals(snapshot.FirstPatient.DiseaseTruth, firstPatientController.DiseaseTruth)
        && ReferenceEquals(snapshot.FirstPatient.PreparedBatches, firstPatientController.PreparedBatches)
        && ReferenceEquals(
            snapshot.FirstPatient.AdministrationExperiments,
            firstPatientController.AdministrationExperiments)
        && ReferenceEquals(snapshot.FirstPatient.Experiments, firstPatientController.Experiments);

    private bool HasExactNonFirstPatientState(SaveSnapshot snapshot) =>
        ReferenceEquals(snapshot.Discovery.Perception, discoveryController.Perception)
        && ReferenceEquals(
            snapshot.Discovery.FocusedSearchActivities,
            discoveryController.Activities)
        && ReferenceEquals(snapshot.Discovery.FocusedSearchOutcomes, discoveryController.Outcomes)
        && ReferenceEquals(
            snapshot.Discovery.PassiveNoticingCandidates,
            discoveryController.PassiveNoticingCandidates)
        && ReferenceEquals(
            snapshot.Discovery.ObservationReinterpretationCandidates,
            discoveryController.ObservationReinterpretationCandidates)
        && ReferenceEquals(snapshot.WorldEvents.EventLog, worldEventController.EventLog)
        && ReferenceEquals(snapshot.WorldEvents.WorldTime, worldEventController.WorldTime)
        && ReferenceEquals(snapshot.CharacterMemory, memoryController.Memory);

    private bool HasRestoredFirstPatientState(FirstPatientSaveState saved)
    {
        DiseaseTruthSnapshot? restoredTruth = firstPatientController.DiseaseTruth;
        CharacterDiseaseState? savedInfection = saved.DiseaseTruth.Infections.SingleOrDefault();
        CharacterDiseaseState? restoredInfection = restoredTruth?.Infections.SingleOrDefault();
        return restoredTruth is not null
            && savedInfection is not null
            && restoredInfection is not null
            && ReferenceEquals(restoredInfection.CharacterId, savedInfection.CharacterId)
            && ReferenceEquals(restoredInfection.Disease, savedInfection.Disease)
            && ReferenceEquals(restoredInfection.Stage, savedInfection.Stage)
            && ReferenceEquals(restoredInfection.CareState, savedInfection.CareState)
            && restoredInfection.LastProgressionEventId == savedInfection.LastProgressionEventId
            && firstPatientController.PreparedBatches?.Batches.Count
                == saved.PreparedBatches.Batches.Count
            && firstPatientController.AdministrationExperiments?.Records.Count
                == saved.AdministrationExperiments.Records.Count
            && firstPatientController.Experiments?.Records.Count == saved.Experiments.Records.Count;
    }

    private bool RejectsEveryUninitializedState(
        WorldEventSnapshot definitions,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime)
    {
        FocusedSearchActivityController missingPerception = new();
        WorldEventController missingDefinitions = CreateWorldEventController(
            worldEvents: null,
            eventLog,
            worldTime);
        WorldEventController missingEventLog = CreateWorldEventController(
            definitions,
            eventLog: null,
            worldTime);
        WorldEventController missingWorldTime = CreateWorldEventController(
            definitions,
            eventLog,
            worldTime: null);
        FirstPatientStateController missingFirstPatient = new();
        JournalController missingJournal = new();
        JournalSectionSelectionController noSelection = new();
        SeekerMapController missingMap = new();
        SaveSnapshotCaptureController discoveryCapture =
            CreateCaptureController(
                missingPerception,
                worldEventController,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                mapController);
        SaveSnapshotCaptureController definitionsCapture =
            CreateCaptureController(
                discoveryController,
                missingDefinitions,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                mapController);
        SaveSnapshotCaptureController eventLogCapture =
            CreateCaptureController(
                discoveryController,
                missingEventLog,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                mapController);
        SaveSnapshotCaptureController worldTimeCapture =
            CreateCaptureController(
                discoveryController,
                missingWorldTime,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                mapController);
        SaveSnapshotCaptureController firstPatientCapture =
            CreateCaptureController(
                discoveryController,
                worldEventController,
                memoryController,
                missingFirstPatient,
                journalController,
                journalSelection,
                mapController);
        SaveSnapshotCaptureController journalCapture =
            CreateCaptureController(
                discoveryController,
                worldEventController,
                memoryController,
                firstPatientController,
                missingJournal,
                noSelection,
                mapController);
        SaveSnapshotCaptureController mapCapture =
            CreateCaptureController(
                discoveryController,
                worldEventController,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                missingMap);
        OpeningRegionInvestigationRouteProgressController missingRoutes = new();
        SaveSnapshotCaptureController routeCapture =
            CreateCaptureController(
                discoveryController,
                worldEventController,
                memoryController,
                firstPatientController,
                journalController,
                journalSelection,
                mapController);
        SetField(routeCapture, "routeProgress", missingRoutes);

        bool rejected = discoveryCapture.Capture() is null
            && definitionsCapture.Capture() is null
            && eventLogCapture.Capture() is null
            && worldTimeCapture.Capture() is null
            && firstPatientCapture.Capture() is null
            && journalCapture.Capture() is null
            && mapCapture.Capture() is null
            && routeCapture.Capture() is null;

        discoveryCapture.Free();
        definitionsCapture.Free();
        eventLogCapture.Free();
        worldTimeCapture.Free();
        firstPatientCapture.Free();
        journalCapture.Free();
        mapCapture.Free();
        routeCapture.Free();
        missingPerception.Free();
        missingDefinitions.Free();
        missingEventLog.Free();
        missingWorldTime.Free();
        missingFirstPatient.Free();
        missingJournal.Free();
        noSelection.Free();
        missingMap.Free();
        missingRoutes.Free();
        return rejected;
    }

    private SaveSnapshotCaptureController CreateCaptureController(
        FocusedSearchActivityController discovery,
        WorldEventController worldEvents,
        CharacterMemoryController memory,
        FirstPatientStateController firstPatient,
        JournalController journal,
        JournalSectionSelectionController selection,
        SeekerMapController map)
    {
        SaveSnapshotCaptureController captureController = new();
        SetField(captureController, "discoveryController", discovery);
        SetField(captureController, "worldEventController", worldEvents);
        SetField(captureController, "memoryController", memory);
        SetField(captureController, "firstPatientController", firstPatient);
        SetField(captureController, "journalController", journal);
        SetField(captureController, "journalSelection", selection);
        SetField(captureController, "mapController", map);
        SetField(captureController, "shrineSource", shrineSource);
        SetField(captureController, "routeProgress", routeProgress);
        SetField(captureController, "routeCompletions", routeCompletions);
        SetField(captureController, "routeConsumption", routeConsumption);
        return captureController;
    }

    private static MapSaveState PopulatedMapState() => new(
        new CharacterId("seeker"),
        [new LocationId("stream")],
        [new LocationId("old-shrine")],
        [new MapNoteSaveEntry(new LocationId("old-shrine"), "Inspect the layered marks.")]);

    private static WorldEventController CreateWorldEventController(
        WorldEventSnapshot? worldEvents,
        WorldEventLogSnapshot? eventLog,
        WorldTimeSnapshot? worldTime)
    {
        WorldEventController eventController = new();
        SetProperty(eventController, nameof(WorldEventController.WorldEvents), worldEvents);
        SetProperty(eventController, nameof(WorldEventController.EventLog), eventLog);
        SetProperty(eventController, nameof(WorldEventController.WorldTime), worldTime);
        return eventController;
    }

    private bool HasExactLiveState(
        CharacterPerceptionSnapshot perception,
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes,
        PassiveNoticingCandidateSnapshot passiveCandidates,
        ObservationReinterpretationCandidateSnapshot reinterpretationCandidates,
        WorldEventSnapshot definitions,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime) =>
        ReferenceEquals(discoveryController.Perception, perception)
        && ReferenceEquals(discoveryController.Activities, activities)
        && ReferenceEquals(discoveryController.Outcomes, outcomes)
        && ReferenceEquals(discoveryController.PassiveNoticingCandidates, passiveCandidates)
        && ReferenceEquals(
            discoveryController.ObservationReinterpretationCandidates,
            reinterpretationCandidates)
        && ReferenceEquals(worldEventController.WorldEvents, definitions)
        && ReferenceEquals(worldEventController.EventLog, eventLog)
        && ReferenceEquals(worldEventController.WorldTime, worldTime);

    private static void SetField(object target, string fieldName, object value) =>
        (target.GetType()
            .GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException($"Missing field {fieldName}."))
        .SetValue(target, value);

    private static void SetProperty(object target, string propertyName, object? value) =>
        (target.GetType()
            .GetProperty(propertyName)
            ?? throw new InvalidOperationException($"Missing property {propertyName}."))
        .SetValue(target, value);

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
