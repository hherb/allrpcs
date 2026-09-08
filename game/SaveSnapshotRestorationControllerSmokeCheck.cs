using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
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
        SaveSnapshot snapshot = new(
            SaveSchemaVersion.FirstSupported,
            discovery,
            eventState);

        if (!controller.TryRestore(snapshot)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !activityIndicator.Visible
            || !controller.TryRestore(snapshot)
            || !HasExactState(snapshot, worldObjects, worldEvents))
        {
            Fail("Save restoration controller smoke failed exact or repeated root restoration.");
            return;
        }

        SaveSnapshot futureVersion = new(new SaveSchemaVersion(2), discovery, eventState);
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

        if (controller.TryRestore(futureVersion)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(missingTarget)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || controller.TryRestore(missingEvent)
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !ThrowsArgumentNull(() => controller.TryRestore(null!))
            || !HasExactState(snapshot, worldObjects, worldEvents)
            || !activityIndicator.Visible)
        {
            Fail("Save restoration controller smoke failed preflight rejection or preservation.");
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
            + "atomic discovery and world-event snapshot installation, repeated restoration, "
            + "fail-closed future version, missing target, missing event, and null handling, and "
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
        && ReferenceEquals(worldEventController.WorldTime, snapshot.WorldEvents.WorldTime);

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
