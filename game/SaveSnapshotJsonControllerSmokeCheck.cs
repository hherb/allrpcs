using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
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
    private CharacterMemoryController memoryController = null!;
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
        memoryController = GetNode<CharacterMemoryController>("../CharacterMemoryController");
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

        string initial = controller.CaptureSerialized()
            ?? throw new InvalidOperationException("Initialized scene state was not serialized.");
        SaveSnapshot initialSnapshot = SaveSnapshotJsonDeserializer.Deserialize(initial);
        string legacy = SaveSnapshotJsonSerializer.Serialize(new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            initialSnapshot.Discovery,
            initialSnapshot.WorldEvents));
        string expectedMigrated = SaveSnapshotJsonSerializer.Serialize(
            SaveSnapshotJsonDeserializer.DeserializeCurrent(legacy));
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
            "\"schemaVersion\":2",
            "\"schemaVersion\":27",
            StringComparison.Ordinal);

        if (controller.TryRestoreSerialized("{")
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(futureVersion)
            || !HasSerializedState(alternateSerialized)
            || controller.TryRestoreSerialized(missingEvent)
            || !HasSerializedState(alternateSerialized)
            || !ThrowsArgumentNull(() => controller.TryRestoreSerialized(null!)))
        {
            Fail("Serialized save smoke failed malformed, future, scenario, or null rejection.");
            return;
        }

        if (!controller.TryRestoreSerialized(legacy)
            || !HasSerializedState(expectedMigrated)
            || memoryController.Memory.Characters.Count != 0)
        {
            Fail("Serialized save smoke failed legacy migration to explicit empty memory.");
            return;
        }

        if (!controller.TryRestoreSerialized(initial)
            || !HasSerializedState(initial)
            || memoryController.Memory.Characters.Count != 1
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
            + "materially different live state, current-schema restoration, version-1 migration "
            + "to explicit empty memory, fail-closed malformed, future version, unknown scenario "
            + "event, and null input, and unchanged authored world, movement, input, and unrelated "
            + "presentation were correct.");
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
