using Allrpcs.Application.Investigations;
using Allrpcs.Application.Journal;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks ordinary explicit save and load input against isolated local storage.</summary>
public partial class SaveLoadInputSmokeCheck : Node
{
    private const string EnabledEnvironmentValue = "1";
    private const string SaveAction = "save_current";
    private const string LoadAction = "load_current";

    private SaveLoadInputController input = null!;
    private SaveSnapshotJsonController jsonController = null!;
    private SaveSnapshotRestorationController restorationController = null!;
    private RepresentativeJournalCompositionCoordinator journalComposition = null!;
    private JournalController journalController = null!;
    private JournalSectionSelectionController journalSelection = null!;
    private SeekerMapController mapController = null!;
    private OpeningRegionOldShrineSceneSource shrineSource = null!;
    private OpeningRegionInvestigationRouteProgressController routeProgress = null!;
    private WorldEventController worldEventController = null!;
    private WorldObjectController worldObjectController = null!;
    private CharacterBody2D seeker = null!;
    private Button save = null!;
    private Button load = null!;
    private Label status = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(
            SaveLoadInputController.SmokeCheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        input = GetNode<SaveLoadInputController>("../SaveLoadInputController");
        jsonController = GetNode<SaveSnapshotJsonController>("../SaveSnapshotJsonController");
        restorationController = GetNode<SaveSnapshotRestorationController>(
            "../SaveSnapshotRestorationController");
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
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        save = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Save");
        load = GetNode<Button>("../CommandOverlay/SaveLoadPanel/Load");
        status = GetNode<Label>("../CommandOverlay/SaveLoadPanel/Status");

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
        WorldEventSnapshot worldEvents = worldEventController.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;

        input._UnhandledInput(Action("save_load_unrelated", pressed: true));
        input._UnhandledInput(Action(SaveAction, pressed: false));
        input._UnhandledInput(Action(LoadAction, pressed: false));
        if (!HasExpectedInitialSurface()
            || File.Exists(StoragePath())
            || !RejectsNull(() => input._UnhandledInput(null!)))
        {
            Fail("Save-load input changed state before an explicit pressed action.");
            return;
        }

        input._UnhandledInput(Action(SaveAction, pressed: true));
        if (status.Text != SaveLoadInputController.SaveFailedStatusText
            || File.Exists(StoragePath()))
        {
            Fail("Save-load input did not expose bounded incomplete-state save failure.");
            return;
        }

        input._UnhandledInput(Action(LoadAction, pressed: true));
        if (status.Text != SaveLoadInputController.MissingStatusText)
        {
            Fail("Save-load input did not expose the missing-document result.");
            return;
        }

        if (!journalComposition.TryComposeAndInitialize()
            || !journalSelection.TrySelect(
                journalController.Journal!,
                journalController.Journal!.Catalogue.Sections[1]))
        {
            Fail("Save-load input smoke could not initialize safe journal state.");
            return;
        }

        MapSaveStateRestorationProjection.Restore(PopulatedMapState()).InstallInto(mapController);
        if (!SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(
                routeProgress,
                mapController,
                shrineSource))
        {
            Fail("Save-load input smoke could not initialize exact route state.");
            return;
        }

        string initial = jsonController.CaptureSerialized()
            ?? throw new InvalidOperationException("Initialized scene state was not serialized.");
        save.EmitSignal(BaseButton.SignalName.Pressed);
        if (status.Text != SaveLoadInputController.SavedStatusText
            || !string.Equals(File.ReadAllText(StoragePath()), initial, StringComparison.Ordinal))
        {
            Fail("Visible save control did not persist the exact complete capture.");
            return;
        }

        if (!restorationController.TryRestore(CreateAlternateState(worldObjects, worldEvents)))
        {
            Fail("Save-load input smoke could not install changed live state.");
            return;
        }

        string alternate = jsonController.CaptureSerialized()
            ?? throw new InvalidOperationException("Changed live state was not serialized.");
        input._UnhandledInput(Action(LoadAction, pressed: true));
        if (status.Text != SaveLoadInputController.LoadedStatusText
            || !HasExpectedState(initial)
            || string.Equals(alternate, initial, StringComparison.Ordinal))
        {
            Fail("Ordinary load input did not restore the exact persisted state.");
            return;
        }

        File.WriteAllText(StoragePath(), "{");
        load.EmitSignal(BaseButton.SignalName.Pressed);
        if (status.Text != SaveLoadInputController.LoadFailedStatusText
            || !HasExpectedState(initial))
        {
            Fail("Visible load control changed live state for a malformed document.");
            return;
        }

        string futureVersion = initial.Replace(
            "\"schemaVersion\":8",
            "\"schemaVersion\":27",
            StringComparison.Ordinal);
        File.WriteAllText(StoragePath(), futureVersion);
        input._UnhandledInput(Action(LoadAction, pressed: true));
        if (status.Text != SaveLoadInputController.LoadFailedStatusText
            || !HasExpectedState(initial))
        {
            Fail("Ordinary load input changed live state for an incompatible document.");
            return;
        }

        File.Delete(StoragePath());
        load.EmitSignal(BaseButton.SignalName.Pressed);
        if (status.Text != SaveLoadInputController.MissingStatusText
            || !HasExpectedState(initial)
            || !ReferenceEquals(worldObjectController.WorldObjects, worldObjects)
            || !ReferenceEquals(worldEventController.WorldEvents, worldEvents)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !HasClosedSurface())
        {
            Fail("Save-load input failed missing-state or unrelated-scene isolation.");
            return;
        }

        GD.Print(
            "Save-load input smoke passed: visible Q-save and L-load controls, released and "
            + "unrelated input, bounded save, load, missing, and failure status, exact complete "
            + "capture, changed-state restoration, malformed and incompatible rejection, "
            + "process-isolated storage, and unrelated scene isolation were correct.");
        GetTree().Quit();
    }

    private bool HasExpectedInitialSurface() =>
        save.IsVisibleInTree()
        && load.IsVisibleInTree()
        && !save.Disabled
        && !load.Disabled
        && save.Text == "Q  SAVE"
        && load.Text == "L  LOAD"
        && status.Text == SaveLoadInputController.ReadyStatusText
        && HasOnlyPhysicalKey(SaveAction, Key.Q)
        && HasOnlyPhysicalKey(LoadAction, Key.L);

    private bool HasExpectedState(string expected) =>
        string.Equals(jsonController.CaptureSerialized(), expected, StringComparison.Ordinal);

    private static SaveSnapshot CreateAlternateState(
        WorldObjectSnapshot worldObjects,
        WorldEventSnapshot worldEvents)
    {
        CharacterId owner = new("seeker");
        ObjectId target = new(worldObjects.Entries[0].Identity.Id.Value);
        WorldEventId firstEvent = new(worldEvents.Entries[0].EventId.Value);
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

    private static InputEventAction Action(string action, bool pressed) => new()
    {
        Action = action,
        Pressed = pressed,
    };

    private static bool HasOnlyPhysicalKey(string actionName, Key expectedKey)
    {
        StringName action = new(actionName);
        Godot.Collections.Array<InputEvent> events = InputMap.ActionGetEvents(action);
        return InputMap.HasAction(action)
            && events.Count == 1
            && events[0] is InputEventKey keyEvent
            && keyEvent.PhysicalKeycode == expectedKey;
    }

    private static bool RejectsNull(Action action)
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

    private static bool HasClosedSurface()
    {
        Type type = typeof(SaveLoadInputController);
        string[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .Select(method => method.Name)
            .Order(StringComparer.Ordinal)
            .ToArray();
        return methods.SequenceEqual(["_Ready", "_UnhandledInput"])
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                | System.Reflection.BindingFlags.Instance
                | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static string StoragePath() => SaveDocumentStorageController.CurrentFilePath();

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
