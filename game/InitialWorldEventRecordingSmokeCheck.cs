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
    private WorldEventController worldEventController = null!;
    private WorldEventLogController eventLogController = null!;
    private WorldTimeController worldTimeController = null!;
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
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
        eventLogController = GetNode<WorldEventLogController>("../WorldEventLogController");
        worldTimeController = GetNode<WorldTimeController>("../WorldTimeController");
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

        WorldEventSnapshot expectedWorldEvents = source.WorldEvents;
        WorldEvent expectedEvent = source.InitialWorldEvent;
        WorldEventId expectedEventId = expectedEvent.EventId;
        WorldEvent untriggeredAdvancingEvent = expectedWorldEvents.Entries[1];
        WorldEventSnapshot? expectedControllerEvents = worldEventController.WorldEvents;
        WorldEventLogSnapshot? expectedEventLog = worldEventController.EventLog;
        WorldTimeSnapshot? expectedWorldTime = worldEventController.WorldTime;
        WorldEventLogSnapshot expectedStandaloneEventLog = eventLogController.EventLog;
        WorldTimeSnapshot expectedStandaloneWorldTime = worldTimeController.WorldTime;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        if (!ReferenceEquals(expectedControllerEvents, expectedWorldEvents)
            || !ReferenceEquals(worldEventController.FindWorldEvent(expectedEventId), expectedEvent)
            || expectedEventLog is null
            || expectedWorldTime is null
            || expectedEventLog.Entries.Count != 1
            || !ReferenceEquals(expectedEventLog.Entries[0], expectedEventId)
            || ReferenceEquals(expectedEventLog.Entries[0], untriggeredAdvancingEvent.EventId)
            || expectedWorldTime.CurrentEventId is not null
            || worldEventController.TryOccur(new WorldEventId(expectedEventId.Value))
            || !ReferenceEquals(worldEventController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldEventController.WorldTime, expectedWorldTime))
        {
            Fail("Initial world-event recording failed exact one-time authoritative recording.");
            return;
        }

        if (!ReferenceEquals(source.WorldEvents, expectedWorldEvents)
            || expectedWorldEvents.Entries.Count != 5
            || !ReferenceEquals(expectedWorldEvents.Entries[0], expectedEvent)
            || !ReferenceEquals(source.InitialWorldEvent, expectedEvent)
            || !ReferenceEquals(worldEventController.WorldEvents, expectedControllerEvents)
            || !ReferenceEquals(eventLogController.EventLog, expectedStandaloneEventLog)
            || !ReferenceEquals(worldTimeController.WorldTime, expectedStandaloneWorldTime)
            || !ReferenceEquals(expectedEvent.EventId, expectedEventId)
            || !ReferenceEquals(expectedEvent.TimeEffect, WorldEventTimeEffect.NoAdvance)
            || !ReferenceEquals(
                untriggeredAdvancingEvent.TimeEffect,
                WorldEventTimeEffect.Advance)
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
            "Initial world-event recording smoke passed: the source identifier resolved to the "
            + "exact authoritative event and was recorded once during startup, a value-equal "
            + "duplicate was rejected, the authored advancing event remained untriggered, and source, "
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
