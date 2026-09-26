using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Application.Persistence;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free local save-document storage checks when enabled.</summary>
public partial class SaveDocumentStorageControllerSmokeCheck : Node
{
    private const string EnabledEnvironmentValue = "1";

    private SaveDocumentStorageController controller = null!;
    private SaveSnapshotJsonController jsonController = null!;
    private SaveSnapshotRestorationController restorationController = null!;
    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private FocusedSearchActivityController discoveryController = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private CharacterBody2D seeker = null!;
    private Label focusStatus = null!;
    private Label focusedSearchResult = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(
            SaveDocumentStorageController.SmokeCheckEnvironmentVariable)
            == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<SaveDocumentStorageController>("../SaveDocumentStorageController");
        jsonController = GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
        journalComposition = GetNode<RepresentativeJournalCompositionCoordinator>(
            "../RepresentativeJournalCompositionCoordinator");
        journalController = GetNode<JournalController>("../JournalController");
        journalSelection = GetNode<JournalSectionSelectionController>(
            "../JournalSectionSelectionController");
        mapController = GetNode<SeekerMapController>("../SeekerMapController");
        discoveryController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");

        File.Delete(StoragePath());
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        try
        {
            RunCheck();
        }
        finally
        {
            File.Delete(StoragePath());
        }
    }

    private void RunCheck()
    {
        WorldObjectSnapshot worldObjects = worldObjectController.WorldObjects
            ?? throw new InvalidOperationException("World objects were not initialized.");
        WorldEventSnapshot definitions = worldEventController.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        if (!journalComposition.TryComposeAndInitialize()
            || !journalSelection.TrySelect(
                journalController.Journal!,
                journalController.Journal!.Catalogue.Sections[1]))
        {
            Fail("Save-document storage smoke could not initialize journal state.");
            return;
        }
        MapSaveStateRestorationProjection.Restore(PopulatedMapState()).InstallInto(mapController);
        string initial = jsonController.CaptureSerialized()
            ?? throw new InvalidOperationException("Initialized scene state was not serialized.");
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        if (controller.LoadCurrent() != SaveDocumentReadStatus.Missing
            || controller.SaveCurrent() != SaveDocumentWriteResult.Saved
            || !string.Equals(File.ReadAllText(StoragePath()), initial, StringComparison.Ordinal))
        {
            Fail("Save-document storage smoke failed missing or exact initial persistence.");
            return;
        }

        SaveSnapshot alternate = CreateAlternateState(worldObjects, definitions);
        if (!restorationController.TryRestore(alternate)
            || controller.LoadCurrent() != SaveDocumentReadStatus.Loaded
            || !HasExpectedState(initial))
        {
            Fail("Save-document storage smoke failed exact persisted restoration.");
            return;
        }

        File.WriteAllText(StoragePath(), "{");
        if (controller.LoadCurrent() != SaveDocumentReadStatus.Failed
            || !HasExpectedState(initial))
        {
            Fail("Save-document storage smoke failed malformed-document rejection.");
            return;
        }

        File.WriteAllBytes(StoragePath(), [0xc3, 0x28]);
        if (controller.LoadCurrent() != SaveDocumentReadStatus.Failed
            || !HasExpectedState(initial)
            || controller.SaveCurrent() != SaveDocumentWriteResult.Saved
            || controller.LoadCurrent() != SaveDocumentReadStatus.Loaded
            || !HasExpectedState(initial))
        {
            Fail("Save-document storage smoke failed read failure or replacement recovery.");
            return;
        }

        File.Delete(StoragePath());
        if (controller.LoadCurrent() != SaveDocumentReadStatus.Missing
            || !HasExpectedState(initial)
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
            Fail("Save-document storage smoke failed missing-state or scene preservation.");
            return;
        }

        GD.Print(
            "Save-document storage controller smoke passed: isolated local persistence, exact "
            + "serialized restoration, malformed and invalid UTF-8 failure, replacement "
            + "recovery, safe journal, exact selection, and populated map round trip, missing "
            + "document, and "
            + "unchanged authored world, movement, input, and unrelated presentation were "
            + "correct.");
        GetTree().Quit();
    }

    private bool HasExpectedState(string expected) =>
        string.Equals(jsonController.CaptureSerialized(), expected, StringComparison.Ordinal);

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

    private static string StoragePath() =>
        SaveDocumentStorageController.CurrentFilePath();

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
