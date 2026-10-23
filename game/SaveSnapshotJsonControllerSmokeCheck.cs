using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free serialized in-memory save round-trip checks when enabled.</summary>
public partial class SaveSnapshotJsonControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_SAVE_SNAPSHOT_JSON_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private SaveSnapshotJsonController controller = null!;
    private FirstPatientStateController firstPatientController = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private CharacterMemoryController memoryController = null!;
    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private SaveSnapshotRestorationController restorationController = null!;
    private FocusedSearchActivityController discoveryController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
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

        controller = GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
        firstPatientController = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
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
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldObjectSnapshot worldObjects = worldObjectController.WorldObjects
            ?? throw new InvalidOperationException("World objects were not initialized.");
        WorldEventSnapshot definitions = worldEventController.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;
        CharacterMemorySnapshot populatedMemory =
            CharacterMemoryControllerSmokeCheck.CreateMemory("healer");
        memoryController.Install(populatedMemory);
        FirstPatientTreatmentContext firstPatientContext = firstPatientContextSource.Context;
        if (!firstPatientController.TryRestore(
                CreatePopulatedFirstPatientState(firstPatientContext, definitions.Entries[0]),
                firstPatientContext,
                definitions))
        {
            Fail("Serialized save smoke could not install populated first-patient state.");
            return;
        }
        if (!journalComposition.TryComposeAndInitialize()
            || !journalSelection.TrySelect(
                journalController.Journal!,
                journalController.Journal!.Catalogue.Sections[4]))
        {
            Fail("Serialized save smoke could not install populated journal state.");
            return;
        }

        MapSaveStateRestorationProjection.Restore(PopulatedMapState()).InstallInto(mapController);
        if (!SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(
                routeProgress,
                mapController,
                shrineSource))
        {
            Fail("Serialized save smoke could not initialize exact route state.");
            return;
        }
        OpeningRegionMapLocationCatalogue initialMapCatalogue = mapController.Catalogue!;

        string initial = controller.CaptureSerialized()
            ?? throw new InvalidOperationException("Initialized scene state was not serialized.");
        SaveSnapshot initialSnapshot = SaveSnapshotJsonDeserializer.Deserialize(initial);
        string legacy = SaveSnapshotJsonSerializer.Serialize(new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            initialSnapshot.Discovery,
            initialSnapshot.WorldEvents));
        SaveSnapshot migrated = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            legacy,
            InitialFirstPatientState(firstPatientContext));
        // A legacy save restores no bodies and the authored inventory, so the capture keeps the
        // live continuation and inventory.
        string expectedMigratedCapture = SaveSnapshotJsonSerializer.Serialize(new SaveSnapshot(
            migrated.SchemaVersion,
            migrated.Discovery,
            migrated.WorldEvents,
            migrated.CharacterMemory,
            migrated.FirstPatient,
            migrated.Journal,
            migrated.Map,
            migrated.InvestigationRoutes,
            migrated.FirstPatientRecovery,
            initialSnapshot.Continuation,
            initialSnapshot.Inventory));
        SaveSnapshot alternate = CreateAlternateState(worldObjects, definitions);
        if (!restorationController.TryRestore(alternate))
        {
            Fail("Serialized save smoke could not install its valid alternate state.");
            return;
        }

        string alternateSerialized = controller.CaptureSerialized()
            ?? throw new InvalidOperationException("Alternate scene state was not serialized.");
        string missingEvent = initial.Replace(
            definitions.Entries[0].EventId.Value,
            "missing-event",
            StringComparison.Ordinal);
        string futureVersion = initial.Replace(
            "\"schemaVersion\":11",
            "\"schemaVersion\":27",
            StringComparison.Ordinal);
        string unknownMapLocation = initial.Replace(
            "\"reportedLocationIds\":[\"stream\"]",
            "\"reportedLocationIds\":[\"missing-location\"]",
            StringComparison.Ordinal);
        string hiddenMapNote = initial.Replace(
            "\"locationId\":\"old-shrine\",\"text\":\"Inspect the layered marks.\"",
            "\"locationId\":\"village-square\",\"text\":\"Inspect the layered marks.\"",
            StringComparison.Ordinal);

        if (controller.TryRestoreSerialized("{")
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(futureVersion)
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(missingEvent)
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(unknownMapLocation)
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(hiddenMapNote)
            || !HasSerializedState(alternateSerialized)
            || !ThrowsArgumentNull(() => controller.TryRestoreSerialized(null!)))
        {
            Fail("Serialized save smoke failed malformed, future, scenario, or null rejection.");
            return;
        }

        if (!controller.TryRestoreSerialized(legacy)
            || !HasSerializedState(expectedMigratedCapture)
            || memoryController.Memory.Characters.Count != 0
            || firstPatientController.PreparedBatches?.Batches.Count != 0
            || firstPatientController.AdministrationExperiments?.Records.Count != 0
            || firstPatientController.Experiments?.Records.Count != 0
            || journalController.Journal?.Observations.Entries.Count != 0
            || journalController.Journal.Testimony?.Entries.Count != 0
            || journalController.Journal.Hypotheses?.Entries.Count != 0
            || journalController.Journal.Experiments?.Entries.Count != 0
            || journalController.Journal.OpenQuestions?.Entries.Count != 0
            || journalSelection.Selection is not null
            || mapController.Map?.ReportedLocations.Count != 0
            || mapController.Map.VerifiedLocations.Count != 0
            || mapController.Notes?.Notes.Count != 0)
        {
            Fail("Serialized save smoke failed legacy migration to explicit empty memory.");
            return;
        }

        if (!controller.TryRestoreSerialized(initial)
            || !HasSerializedState(initial)
            || memoryController.Memory.Characters.Count != 1
            || firstPatientController.PreparedBatches?.Batches.Count != 1
            || firstPatientController.AdministrationExperiments?.Records.Count != 1
            || firstPatientController.Experiments?.Records.Count != 1
            || journalController.Journal?.PendingSections.Count != 0
            || journalSelection.Selection?.Key != "open-questions"
            || mapController.Map?.ReportedLocations.Single().Id.Value != "stream"
            || mapController.Map.VerifiedLocations.Single().Id.Value != "old-shrine"
            || mapController.Notes?.Notes.Single().Text != "Inspect the layered marks."
            || ReferenceEquals(mapController.Catalogue, initialMapCatalogue)
            || !ReferenceEquals(
                mapController.Map.VerifiedLocations[0],
                mapController.Catalogue!.Locations.Single(location =>
                    location.Id.Value == "old-shrine"))
            || !ReferenceEquals(
                firstPatientController.AdministrationExperiments!.Records[0]
                    .Administration.WorldEvent,
                definitions.Entries[0])
            || !ReferenceEquals(
                firstPatientController.Experiments!.Records[0].WorldEvent,
                definitions.Entries[0])
            || !ReferenceEquals(worldObjectController.WorldObjects, worldObjects)
            || !ReferenceEquals(worldEventController.WorldEvents, definitions)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Serialized save smoke failed exact round-trip restoration or preservation.");
            return;
        }

        GD.Print(
            "Serialized save controller smoke passed: canonical populated-memory capture, "
            + "complete prepared-batch and dual-journal first-patient restoration, materially "
            + "different live state, safe journal, selection, populated map, and route-lifecycle "
            + "round trip with fresh canonical definitions, version-1 migration to explicit empty "
            + "memory, patient, journal, map, and route state, fail-closed malformed, future version, "
            + "unknown scenario "
            + "event, unknown map location, hidden map note, and null "
            + "input, and unchanged authored world, movement, input, and unrelated presentation "
            + "were correct.");
        GetTree().Quit();
    }

    private bool HasSerializedState(string expected) =>
        string.Equals(controller.CaptureSerialized(), expected, StringComparison.Ordinal);

    private static SaveSnapshot CreateAlternateState(
        WorldObjectSnapshot worldObjects,
        WorldEventSnapshot definitions)
    {
        CharacterId owner = new("seeker");
        ObjectId target = new(worldObjects.Entries[0].Identity.Id.Value);
        WorldEventId firstEvent = new(definitions.Entries[0].EventId.Value);
        return new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(owner, []),
                new FocusedSearchActivitySnapshot(
                    [new FocusedSearchActivity(new FocusedSearchRequest(owner, target))]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([firstEvent]),
                new WorldTimeSnapshot(null)));
    }

    private static MapSaveState PopulatedMapState() => new(
        new CharacterId("seeker"),
        [new LocationId("stream")],
        [new LocationId("old-shrine")],
        [new MapNoteSaveEntry(new LocationId("old-shrine"), "Inspect the layered marks.")]);

    private static FirstPatientSaveState InitialFirstPatientState(
        FirstPatientTreatmentContext context) => new(
        context.Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));

    private static FirstPatientSaveState CreatePopulatedFirstPatientState(
        FirstPatientTreatmentContext context,
        WorldEvent worldEvent)
    {
        CharacterDiseaseState infection = context.Scenario.DiseaseTruth.Infections.Single();
        DiseaseTruthSnapshot truth = new(
            context.Scenario.DiseaseTruth.Definitions,
            [
                new CharacterDiseaseState(
                    infection.CharacterId,
                    infection.Disease,
                    infection.Stage,
                    worldEvent.EventId,
                    DiseaseCareState.Comforted),
            ]);
        IngredientPreparationRecipe recipe = context.Scenario.Remedy.Recipe;
        IngredientPreparationActivity activity = new(
            recipe,
            new IngredientPreparationProgress(recipe.Plan, recipe.Plan.Steps.Count));
        IngredientPreparationOutcome preparation = new(
            activity,
            recipe.ResultDefinition,
            recipe.ResultQuantity);
        IngredientQuantity dosageQuantity = new(25, recipe.ResultQuantity.Unit);
        IngredientDosage dosage = new(preparation, dosageQuantity);
        PreparedIngredientBatch batch = PreparedIngredientBatchConsumptionTransition.Consume(
            dosage,
            new PreparedIngredientBatch(preparation))!;
        IngredientAdministrationOutcome administration = new(
            new CharacterId("healer"),
            context.Scenario.PatientId,
            dosage,
            worldEvent);
        ExperimentId experimentId = new("serialized-first-patient-dose");

        return new FirstPatientSaveState(
            truth,
            new PreparedIngredientBatchSnapshot([batch]),
            new IngredientExperimentJournalSnapshot(
                [
                    new IngredientAdministrationExperimentRecord(
                        experimentId,
                        administration,
                        new CharacterConditionSet([])),
                ]),
            new ExperimentJournalSnapshot(
                [
                    new ExperimentJournalRecord(
                        experimentId,
                        worldEvent,
                        "Administer one serialized first-patient dose.",
                        "The complete first-patient state remains observable."),
                ]));
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
