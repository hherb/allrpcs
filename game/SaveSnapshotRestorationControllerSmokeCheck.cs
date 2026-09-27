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
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free complete in-memory save restoration checks when enabled.</summary>
public partial class SaveSnapshotRestorationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_SAVE_SNAPSHOT_RESTORATION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private SaveSnapshotRestorationController controller = null!;
    private FirstPatientStateController firstPatientController = null!;
    private TemporaryFirstPatientTreatmentContextSource firstPatientContextSource = null!;
    private CharacterMemoryController memoryController = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator routeCompletions = null!;
    private OpeningRegionInvestigationRouteCompletionConsumptionCoordinator routeConsumption = null!;
    private FocusedSearchActivityController discoveryController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
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

        controller = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        firstPatientController = GetNode<FirstPatientStateController>(
            "../FirstPatientStateController");
        firstPatientContextSource = GetNode<TemporaryFirstPatientTreatmentContextSource>(
            "../TemporaryFirstPatientTreatmentContextSource");
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
        journalController = GetNode<JournalController>("../JournalController");
        journalSelection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
        mapController = GetNode<SeekerMapController>("../SeekerMapController");
        routeProgress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
        routeCompletions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
        routeConsumption = GetNode<OpeningRegionInvestigationRouteCompletionConsumptionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionConsumptionCoordinator");
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
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

        WorldObjectSnapshot worldObjects = worldObjectController.WorldObjects
            ?? throw new InvalidOperationException("World objects were not initialized.");
        WorldEventSnapshot worldEvents = worldEventController.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        CharacterId owner = new(discoveryController.SearcherCharacterId.Value);
        ObjectId firstTarget = new(worldObjects.Entries[0].Identity.Id.Value);
        ObjectId secondTarget = new(worldObjects.Entries[1].Identity.Id.Value);
        DiscoverySaveState discovery = CreateDiscovery(owner, firstTarget, secondTarget);
        WorldEventSaveState eventState = CreateEventState(worldEvents);
        CharacterMemorySnapshot memory = CharacterMemoryControllerSmokeCheck.CreateMemory("healer");
        FirstPatientSaveState firstPatient = CreateFirstPatientState(
            firstPatientContextSource.Context);
        JournalSaveState journal = CreateJournalState(owner);
        MapSaveState map = CreateMapState(owner);
        SaveSnapshot snapshot = new(
            SaveSchemaVersion.InvestigationRoutes,
            discovery,
            eventState,
            memory,
            firstPatient,
            journal,
            map,
            CreateRouteState());

        if (!controller.TryRestore(snapshot)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !HasRestoredFirstPatient(firstPatientContextSource.Context, firstPatient)
            || !HasRestoredJournal(journal)
            || !HasRestoredMap(map)
            || !HasRestoredRoutes()
            || !activityIndicator.Visible
            || !controller.TryRestore(snapshot)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !HasRestoredJournal(journal)
            || !HasRestoredMap(map)
            || !HasRestoredRoutes())
        {
            Fail("Save restoration controller smoke failed exact or repeated root restoration.");
            return;
        }

        DiseaseTruthSnapshot restoredDiseaseTruth = firstPatientController.DiseaseTruth!;
        PreparedIngredientBatchSnapshot restoredPreparedBatches =
            firstPatientController.PreparedBatches!;
        IngredientExperimentJournalSnapshot restoredAdministrationExperiments =
            firstPatientController.AdministrationExperiments!;
        ExperimentJournalSnapshot restoredExperiments = firstPatientController.Experiments!;
        JournalSnapshot restoredJournal = journalController.Journal!;
        JournalSectionDefinition restoredSelection = journalSelection.Selection!;
        OpeningRegionMapLocationCatalogue restoredMapCatalogue = mapController.Catalogue!;
        SeekerMapSnapshot restoredMap = mapController.Map!;
        SeekerMapNoteSnapshot restoredMapNotes = mapController.Notes!;
        OpeningRegionInvestigationRouteProgressSnapshot restoredRouteProgress =
            routeProgress.Progress!;
        OpeningRegionInvestigationRouteCompletion restoredCompletion =
            routeCompletions.AcceptedCompletions.Single();

        SaveSnapshot futureVersion = new(new SaveSchemaVersion(27), discovery, eventState);
        SaveSnapshot missingTarget = new(
            SaveSchemaVersion.FirstSupported,
            CreateDiscovery(owner, firstTarget, new ObjectId("missing-object")),
            eventState);
        WorldEventSaveState missingEventState = new(
            new WorldEventLogSnapshot([new WorldEventId("missing-event")]),
            new WorldTimeSnapshot(null));
        SaveSnapshot missingEvent = new(
            SaveSchemaVersion.FirstSupported,
            discovery,
            missingEventState);
        JournalSaveState duplicateJournal = new(
            journal.SeekerId,
            [journal.Observations[0], journal.Observations[0]],
            journal.Testimony,
            journal.Hypotheses,
            journal.Experiments,
            journal.OpenQuestions,
            journal.SelectedSectionKey);
        SaveSnapshot invalidJournal = new(
            SaveSchemaVersion.Journal,
            discovery,
            eventState,
            memory,
            firstPatient,
            duplicateJournal);
        MapSaveState hiddenNoteMap = new(
            owner,
            [new LocationId("stream")],
            [],
            [new MapNoteSaveEntry(new LocationId("old-shrine"), "Still hidden.")]);
        SaveSnapshot invalidMap = new(
            SaveSchemaVersion.Map,
            discovery,
            eventState,
            memory,
            firstPatient,
            journal,
            hiddenNoteMap);
        InvestigationRouteSaveState unknownRouteState = new(
            [
                new InvestigationRouteProgressSaveEntry("unknown", [], []),
                new InvestigationRouteProgressSaveEntry("shrine", [], []),
                new InvestigationRouteProgressSaveEntry("road-wagon", [], []),
            ],
            [],
            null);
        SaveSnapshot invalidRoutes = new(
            SaveSchemaVersion.InvestigationRoutes,
            discovery,
            eventState,
            memory,
            firstPatient,
            journal,
            map,
            unknownRouteState);

        if (controller.TryRestore(futureVersion)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(missingTarget)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(missingEvent)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(invalidJournal)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(invalidMap)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(invalidRoutes)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !ThrowsArgumentNull(() => controller.TryRestore(null!))
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !activityIndicator.Visible)
        {
            Fail("Save restoration controller smoke failed preflight rejection or preservation.");
            return;
        }

        if (!ReferenceEquals(firstPatientController.DiseaseTruth, restoredDiseaseTruth)
            || !ReferenceEquals(firstPatientController.PreparedBatches, restoredPreparedBatches)
            || !ReferenceEquals(
                firstPatientController.AdministrationExperiments,
                restoredAdministrationExperiments)
            || !ReferenceEquals(firstPatientController.Experiments, restoredExperiments))
        {
            Fail("Save restoration changed first-patient state after rejected root input.");
            return;
        }

        if (!ReferenceEquals(journalController.Journal, restoredJournal)
            || !ReferenceEquals(journalSelection.Selection, restoredSelection))
        {
            Fail("Save restoration changed journal state after rejected root input.");
            return;
        }

        if (!ReferenceEquals(mapController.Catalogue, restoredMapCatalogue)
            || !ReferenceEquals(mapController.Map, restoredMap)
            || !ReferenceEquals(mapController.Notes, restoredMapNotes))
        {
            Fail("Save restoration changed map state after rejected root input.");
            return;
        }

        if (!ReferenceEquals(routeProgress.Progress, restoredRouteProgress)
            || !ReferenceEquals(routeCompletions.AcceptedCompletions.Single(), restoredCompletion)
            || !ReferenceEquals(routeConsumption.ConsumedCompletion, restoredCompletion))
        {
            Fail("Save restoration changed route lifecycle after rejected root input.");
            return;
        }

        if (seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Save restoration changed movement, input, or presentation outside activity.");
            return;
        }

        GD.Print(
            "Save restoration controller smoke passed: complete versioned-root preflight, exact "
            + "atomic discovery, world-event, character-memory, and rebound first-patient state "
            + "installation plus fresh safe journal, exact selection, populated map, and route "
            + "lifecycle with fresh canonical definitions, repeated "
            + "restoration, fail-closed future version, missing target, missing event, duplicate "
            + "journal, hidden-note map, and null handling, and "
            + "unchanged authoritative definitions, movement, input, and other presentation state "
            + "were correct.");
        GetTree().Quit();
    }

    private bool HasExactState(
        SaveSnapshot snapshot,
        WorldObjectSnapshot worldObjects,
        WorldEventSnapshot worldEvents) =>
        ReferenceEquals(worldObjectController.WorldObjects, worldObjects)
        && ReferenceEquals(worldEventController.WorldEvents, worldEvents)
        && ReferenceEquals(discoveryController.Perception, snapshot.Discovery.Perception)
        && ReferenceEquals(
            discoveryController.Activities,
            snapshot.Discovery.FocusedSearchActivities)
        && ReferenceEquals(discoveryController.Outcomes, snapshot.Discovery.FocusedSearchOutcomes)
        && ReferenceEquals(
            discoveryController.PassiveNoticingCandidates,
            snapshot.Discovery.PassiveNoticingCandidates)
        && ReferenceEquals(
            discoveryController.ObservationReinterpretationCandidates,
            snapshot.Discovery.ObservationReinterpretationCandidates)
        && ReferenceEquals(worldEventController.EventLog, snapshot.WorldEvents.EventLog)
        && ReferenceEquals(worldEventController.WorldTime, snapshot.WorldEvents.WorldTime)
        && ReferenceEquals(memoryController.Memory, snapshot.CharacterMemory);

    private bool HasRestoredJournal(JournalSaveState saved)
    {
        JournalSnapshot? journal = journalController.Journal;
        JournalSectionDefinition? selected = journalSelection.Selection;
        return journal is not null
            && !ReferenceEquals(journal, saved)
            && journal.SeekerId == saved.SeekerId
            && journal.PendingSections.Count == 0
            && journal.Observations.Entries.Count == 1
            && journal.Observations.Entries[0].ObjectId == saved.Observations[0].ObjectId
            && journal.Testimony?.Entries.Count == 0
            && journal.Hypotheses?.Entries.Count == 0
            && journal.Experiments?.Entries.Count == 0
            && journal.OpenQuestions?.Entries.Count == 0
            && selected is not null
            && ReferenceEquals(selected, journal.Catalogue.Sections[2]);
    }

    private bool HasRestoredMap(MapSaveState saved)
    {
        OpeningRegionMapLocationCatalogue? catalogue = mapController.Catalogue;
        SeekerMapSnapshot? map = mapController.Map;
        SeekerMapNoteSnapshot? notes = mapController.Notes;
        return catalogue is not null
            && map is not null
            && notes is not null
            && map.SeekerId == saved.SeekerId
            && map.ReportedLocations.Single().Id == saved.ReportedLocationIds.Single()
            && map.VerifiedLocations.Single().Id == saved.VerifiedLocationIds.Single()
            && notes.Notes.Single().Location.Id == saved.Notes.Single().LocationId
            && notes.Notes.Single().Text == saved.Notes.Single().Text
            && ReferenceEquals(
                map.VerifiedLocations[0],
                catalogue.Locations.Single(location => location.Id.Value == "old-shrine"))
            && ReferenceEquals(notes.Notes[0].Location, map.VerifiedLocations[0]);
    }

    private bool HasRestoredRoutes()
    {
        OpeningRegionInvestigationRouteProgressSnapshot? progress = routeProgress.Progress;
        OpeningRegionInvestigationRouteCompletion? completion =
            routeCompletions.AcceptedCompletions.SingleOrDefault();
        return progress is not null
            && ReferenceEquals(routeProgress.Catalogue, progress.Catalogue)
            && progress.Entries[0].RecordedLocationIds.Count == 2
            && progress.Entries[0].RecordedEvidenceIds.Count == 1
            && progress.Entries[1].RecordedLocationIds.Count == 1
            && progress.Entries[1].RecordedEvidenceIds.Count == 0
            && progress.Entries[2].RecordedLocationIds.Count == 0
            && progress.Entries[2].RecordedEvidenceIds.Count == 0
            && completion is not null
            && ReferenceEquals(completion.ProgressSnapshot, progress)
            && ReferenceEquals(completion.Route, progress.Catalogue.Routes[0])
            && ReferenceEquals(routeConsumption.ConsumedCompletion, completion);
    }

    private bool HasRestoredFirstPatient(
        FirstPatientTreatmentContext context,
        FirstPatientSaveState saved)
    {
        DiseaseTruthSnapshot? truth = firstPatientController.DiseaseTruth;
        CharacterDiseaseState? infection = truth?.Infections.Count == 1
            ? truth.Infections[0]
            : null;
        return truth is not null
            && !ReferenceEquals(truth, saved.DiseaseTruth)
            && infection is not null
            && ReferenceEquals(infection.CharacterId, context.Scenario.PatientId)
            && ReferenceEquals(infection.Disease, context.Scenario.Remedy.Disease)
            && ReferenceEquals(infection.CareState, DiseaseCareState.Comforted)
            && firstPatientController.PreparedBatches?.Batches.Count == 0
            && firstPatientController.AdministrationExperiments?.Records.Count == 0
            && firstPatientController.Experiments?.Records.Count == 0;
    }

    private static FirstPatientSaveState CreateFirstPatientState(
        FirstPatientTreatmentContext context)
    {
        CharacterDiseaseState initial = context.Scenario.DiseaseTruth.Infections.Single();
        DiseaseDefinition savedDisease = new(new DiseaseId(initial.Disease.DiseaseId.Value));
        return new FirstPatientSaveState(
            new DiseaseTruthSnapshot(
                new DiseaseDefinitionSnapshot([savedDisease]),
                [
                    new CharacterDiseaseState(
                        new CharacterId(initial.CharacterId.Value),
                        savedDisease,
                        initial.Stage,
                        null,
                        DiseaseCareState.Comforted),
                ]),
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
    }

    private static DiscoverySaveState CreateDiscovery(
        CharacterId owner,
        ObjectId firstTarget,
        ObjectId secondTarget) => new(
            new CharacterPerceptionSnapshot(
                owner,
                [
                    new WorldObjectVisibility(firstTarget, VisibilityState.Noticed),
                    new WorldObjectVisibility(secondTarget, VisibilityState.Identified),
                ]),
            new FocusedSearchActivitySnapshot(
                [new FocusedSearchActivity(new FocusedSearchRequest(owner, firstTarget))]),
            new FocusedSearchOutcomeSnapshot(
                [
                    new FocusedSearchOutcome(
                        new FocusedSearchRequest(owner, secondTarget),
                        VisibilityState.Understood),
                ]),
            new PassiveNoticingCandidateSnapshot(
                [new PassiveNoticingCandidate(owner, firstTarget)]),
            new ObservationReinterpretationCandidateSnapshot(
                [
                    new ObservationReinterpretationCandidate(
                        owner,
                        secondTarget,
                        new KnowledgeId("stone-marker-meaning")),
                ]));

    private static WorldEventSaveState CreateEventState(WorldEventSnapshot worldEvents)
    {
        WorldEvent firstEvent = worldEvents.Entries[0];
        WorldEvent advancingEvent = worldEvents.Entries[1];
        return new WorldEventSaveState(
            new WorldEventLogSnapshot([firstEvent.EventId, advancingEvent.EventId]),
            new WorldTimeSnapshot(new WorldEventId(advancingEvent.EventId.Value)));
    }

    private static JournalSaveState CreateJournalState(CharacterId owner) => new(
        owner,
        [
            new JournalObservationSaveEntry(
                new ObjectId("old-shrine"),
                "Old Shrine",
                "Layered marks cross the weathered stone."),
        ],
        [],
        [],
        [],
        [],
        "hypotheses");

    private static MapSaveState CreateMapState(CharacterId owner) => new(
        owner,
        [new LocationId("stream")],
        [new LocationId("old-shrine")],
        [new MapNoteSaveEntry(new LocationId("old-shrine"), "Inspect the layered marks.")]);

    private static InvestigationRouteSaveState CreateRouteState() => new(
        [
            new InvestigationRouteProgressSaveEntry(
                "stream-laundry",
                [new LocationId("stream"), new LocationId("abandoned-laundry")],
                [new EvidenceId("residue")]),
            new InvestigationRouteProgressSaveEntry(
                "shrine",
                [new LocationId("old-shrine")],
                []),
            new InvestigationRouteProgressSaveEntry("road-wagon", [], []),
        ],
        ["stream-laundry"],
        "stream-laundry");

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
