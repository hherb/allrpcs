using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free initial world-event recording checks when explicitly enabled.</summary>
public partial class InitialWorldEventRecordingSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_INITIAL_WORLD_EVENT_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private TemporaryWorldEventSource source = null!;
    private WorldEventLogController eventLogController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;
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

        source = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        eventLogController = GetNode<WorldEventLogController>("../WorldEventLogController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        focusStatus = GetNode<Label>("../CommandOverlay/Panel/FocusStatus");
        focusedSearchResult = GetNode<Label>("../CommandOverlay/Panel/FocusedSearchResult");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldEvent expectedEvent = source.Event;
        WorldEventId expectedEventId = expectedEvent.EventId;
        WorldEventLogSnapshot expectedEventLog = eventLogController.EventLog;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        if (expectedEventLog.Entries.Count != 1
            || !ReferenceEquals(expectedEventLog.Entries[0], expectedEventId)
            || eventLogController.TryAppend(new WorldEventId(expectedEventId.Value))
            || !ReferenceEquals(eventLogController.EventLog, expectedEventLog))
        {
            Fail("Initial world-event recording failed exact one-time append.");
            return;
        }

        if (!ReferenceEquals(source.Event, expectedEvent)
            || !ReferenceEquals(expectedEvent.EventId, expectedEventId)
            || !ReferenceEquals(expectedEvent.TimeEffect, WorldEventTimeEffect.NoAdvance)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || seeker.Position != expectedPosition
            || seeker.Velocity != expectedVelocity
            || !string.Equals(focusStatus.Text, expectedFocusStatus, StringComparison.Ordinal)
            || !string.Equals(
                focusedSearchResult.Text,
                expectedFocusedSearchResult,
                StringComparison.Ordinal))
        {
            Fail("Initial world-event recording changed source, world, gameplay, or presentation state.");
            return;
        }

        GD.Print(
            "Initial world-event recording smoke passed: the exact source identifier was "
            + "appended once during startup, a value-equal duplicate was rejected, and source, "
            + "world-object, perception, focused-search, movement, input, and presentation state "
            + "were unchanged.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
