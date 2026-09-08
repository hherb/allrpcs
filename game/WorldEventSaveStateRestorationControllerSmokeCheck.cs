using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Runs credential-free world-event save-state controller restoration checks when enabled.
/// </summary>
public partial class WorldEventSaveStateRestorationControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_WORLD_EVENT_SAVE_STATE_RESTORATION_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private WorldEventController controller = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
    private CharacterBody2D seeker = null!;
    private Line2D activityIndicator = null!;
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

        controller = GetNode<WorldEventController>("../WorldEventController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        activityIndicator = GetNode<Line2D>(
            "../Seeker/FocusedSearchActivityIndicator/Indicator");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldEventSnapshot definitions = controller.WorldEvents
            ?? throw new InvalidOperationException("World events were not initialized.");
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        PassiveNoticingCandidateSnapshot expectedPassiveCandidates =
            focusedSearchController.PassiveNoticingCandidates;
        ObservationReinterpretationCandidateSnapshot expectedReinterpretationCandidates =
            focusedSearchController.ObservationReinterpretationCandidates;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        bool expectedActivityIndicator = activityIndicator.Visible;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        WorldEvent firstEvent = definitions.Entries[0];
        WorldEvent advancingEvent = definitions.Entries[1];
        WorldEventLogSnapshot eventLog = new([firstEvent.EventId, advancingEvent.EventId]);
        WorldTimeSnapshot worldTime = new(new WorldEventId(advancingEvent.EventId.Value));
        WorldEventSaveState state = new(eventLog, worldTime);

        if (!controller.TryRestoreWorldEventState(state)
            || !HasExactState(state, definitions)
            || !controller.TryRestoreWorldEventState(state)
            || !HasExactState(state, definitions))
        {
            Fail("World-event save-state controller smoke failed exact or repeated restoration.");
            return;
        }

        WorldEventId missingId = new("missing-event");
        WorldEventSaveState missingEvent = new(
            new WorldEventLogSnapshot([firstEvent.EventId, missingId]),
            new WorldTimeSnapshot(null));
        WorldEventSaveState staleTime = new(
            eventLog,
            new WorldTimeSnapshot(null));
        WorldEventController uninitialized = new();
        AddChild(uninitialized);

        if (controller.TryRestoreWorldEventState(missingEvent)
            || !HasExactState(state, definitions)
            || controller.TryRestoreWorldEventState(staleTime)
            || !HasExactState(state, definitions)
            || uninitialized.TryRestoreWorldEventState(state)
            || uninitialized.WorldEvents is not null
            || uninitialized.EventLog is not null
            || uninitialized.WorldTime is not null
            || !ThrowsArgumentNull(() => controller.TryRestoreWorldEventState(null!))
            || !HasExactState(state, definitions))
        {
            Fail("World-event save-state controller smoke failed rejection or state preservation.");
            return;
        }

        if (!ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(
                focusedSearchController.PassiveNoticingCandidates,
                expectedPassiveCandidates)
            || !ReferenceEquals(
                focusedSearchController.ObservationReinterpretationCandidates,
                expectedReinterpretationCandidates)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || activityIndicator.Visible != expectedActivityIndicator
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("World-event restoration changed world, discovery, movement, input, or presentation state.");
            return;
        }

        GD.Print(
            "World-event save-state controller smoke passed: exact atomic event-log and "
            + "qualitative-time installation, retained authored definitions, repeated restoration, "
            + "fail-closed missing event, stale time, uninitialized, and null handling, and unchanged "
            + "world, discovery, movement, input, and presentation state were correct.");
        GetTree().Quit();
    }

    private bool HasExactState(WorldEventSaveState state, WorldEventSnapshot definitions) =>
        ReferenceEquals(controller.WorldEvents, definitions)
        && ReferenceEquals(controller.EventLog, state.EventLog)
        && ReferenceEquals(controller.WorldTime, state.WorldTime);

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
