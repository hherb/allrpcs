using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free temporary world-event source checks when explicitly enabled.</summary>
public partial class TemporaryWorldEventSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_EVENT_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string GreyboxReadyEventIdText = "greybox-ready";

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

        WorldEventLogSnapshot expectedEventLog = eventLogController.EventLog;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;
        WorldEventSnapshot firstSnapshot = source.WorldEvents;
        WorldEventSnapshot secondSnapshot = source.WorldEvents;
        WorldEvent first = firstSnapshot.Entries.Single();

        if (!HasExpectedPropertySurface()
            || !ReferenceEquals(firstSnapshot, secondSnapshot)
            || firstSnapshot.Entries.Count != 1
            || !string.Equals(
                first.EventId.Value,
                GreyboxReadyEventIdText,
                StringComparison.Ordinal)
            || !ReferenceEquals(first.TimeEffect, WorldEventTimeEffect.NoAdvance))
        {
            Fail("Temporary world-event source failed canonical exact event retention.");
            return;
        }

        WorldEvent independent = CreateEvent();
        if (ReferenceEquals(independent, first)
            || independent != first
            || !string.Equals(
                independent.EventId.Value,
                GreyboxReadyEventIdText,
                StringComparison.Ordinal)
            || !ReferenceEquals(independent.TimeEffect, WorldEventTimeEffect.NoAdvance))
        {
            Fail("Temporary world-event source failed independent construction.");
            return;
        }

        if (!ReferenceEquals(eventLogController.EventLog, expectedEventLog)
            || expectedEventLog.Entries.Count != 1
            || !ReferenceEquals(expectedEventLog.Entries[0], first.EventId)
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
            Fail("Temporary world-event source changed log, world, gameplay, or presentation state.");
            return;
        }

        GD.Print(
            "Temporary world-event source smoke passed: one stable exact snapshot containing the "
            + "canonical greybox-ready no-advance event, minimal surface, independent construction, "
            + "exact ordinary startup recording, and unchanged event-log, "
            + "world-object, perception, focused-search, movement, input, and presentation state "
            + "were correct.");
        GetTree().Quit();
    }

    private static WorldEvent CreateEvent()
    {
        System.Reflection.MethodInfo method = typeof(TemporaryWorldEventSource).GetMethod(
            "CreateEvent",
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException(
                "Temporary world-event source factory was not found.");

        return (WorldEvent)(method.Invoke(null, null)
            ?? throw new InvalidOperationException(
                "Temporary world-event source returned no event."));
    }

    private static bool HasExpectedPropertySurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(TemporaryWorldEventSource)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);

        return properties.Length == 1
            && string.Equals(properties[0].Name, "WorldEvents", StringComparison.Ordinal)
            && properties[0].PropertyType == typeof(WorldEventSnapshot)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
