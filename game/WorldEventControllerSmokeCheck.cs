using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free atomic world-event controller checks when explicitly enabled.</summary>
public partial class WorldEventControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_EVENT_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private WorldEventController controller = null!;
    private TemporaryWorldEventSource source = null!;
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

        controller = GetNode<WorldEventController>("../WorldEventController");
        source = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
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

        WorldEventSnapshot sourceSnapshot = source.WorldEvents;
        WorldEvent sourceEvent = source.InitialWorldEvent;
        WorldEvent untriggeredAdvancingEvent = sourceSnapshot.Entries[1];
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

        WorldEventSnapshot? initializedEvents = controller.WorldEvents;
        WorldEventLogSnapshot? startupLog = controller.EventLog;
        WorldTimeSnapshot? startupTime = controller.WorldTime;
        if (!ReferenceEquals(initializedEvents, sourceSnapshot)
            || startupLog is null
            || startupTime is null
            || startupLog.Entries.Count != 1
            || !ReferenceEquals(startupLog.Entries[0], sourceEvent.EventId)
            || ReferenceEquals(startupLog.Entries[0], untriggeredAdvancingEvent.EventId)
            || startupTime.CurrentEventId is not null
            || !ReferenceEquals(controller.FindWorldEvent(sourceEvent.EventId), sourceEvent)
            || controller.TryOccur(new WorldEventId(sourceEvent.EventId.Value))
            || !ReferenceEquals(controller.EventLog, startupLog)
            || !ReferenceEquals(controller.WorldTime, startupTime)
            || !HasExpectedControllerSurface())
        {
            Fail("World-event controller failed exact atomic startup occurrence.");
            return;
        }

        if (!ProvesInitializationAndAtomicOccurrences(sourceEvent)
            || !ProvesRejectedOccurrencesPreserveEverySnapshot(sourceEvent))
        {
            return;
        }

        if (!ReferenceEquals(source.WorldEvents, sourceSnapshot)
            || sourceSnapshot.Entries.Count != 6
            || !ReferenceEquals(sourceSnapshot.Entries[0], sourceEvent)
            || !ReferenceEquals(source.InitialWorldEvent, sourceEvent)
            || !ReferenceEquals(
                untriggeredAdvancingEvent.TimeEffect,
                WorldEventTimeEffect.Advance)
            || !ReferenceEquals(controller.WorldEvents, initializedEvents)
            || !ReferenceEquals(controller.EventLog, startupLog)
            || !ReferenceEquals(controller.WorldTime, startupTime)
            || !ReferenceEquals(eventLogController.EventLog, expectedStandaloneEventLog)
            || !ReferenceEquals(worldTimeController.WorldTime, expectedStandaloneWorldTime)
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
            Fail("World-event occurrence changed source, world, gameplay, or presentation state.");
            return;
        }

        GD.Print(
            "World-event controller smoke passed: exact one-time state initialization, atomic "
            + "no-advance and advancing occurrences, authoritative identifier retention, fail-closed "
            + "uninitialized, undefined, duplicate, repeated-current, and null handling, and unchanged "
            + "source, world, discovery, movement, input, and presentation state were correct.");
        GetTree().Quit();
    }

    private bool ProvesInitializationAndAtomicOccurrences(WorldEvent noAdvanceEvent)
    {
        WorldEvent advancingEvent = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        WorldEventSnapshot worldEvents = new([noAdvanceEvent, advancingEvent]);
        WorldEventLogSnapshot initialLog = new([]);
        WorldTimeSnapshot initialTime = new(null);
        WorldEventController separateController = new();
        AddChild(separateController);

        if (!separateController.TryInitialize(worldEvents, initialLog, initialTime)
            || !ReferenceEquals(separateController.WorldEvents, worldEvents)
            || !ReferenceEquals(separateController.EventLog, initialLog)
            || !ReferenceEquals(separateController.WorldTime, initialTime)
            || !separateController.TryOccur(new WorldEventId(noAdvanceEvent.EventId.Value)))
        {
            Fail("World-event controller rejected exact initialization or a no-advance occurrence.");
            return false;
        }

        WorldEventLogSnapshot noAdvanceLog = separateController.EventLog!;
        if (noAdvanceLog.Entries.Count != 1
            || !ReferenceEquals(noAdvanceLog.Entries[0], noAdvanceEvent.EventId)
            || !ReferenceEquals(separateController.WorldTime, initialTime)
            || !separateController.TryOccur(new WorldEventId(advancingEvent.EventId.Value)))
        {
            Fail("World-event controller failed no-advance state preservation or advancing occurrence.");
            return false;
        }

        WorldEventLogSnapshot advancedLog = separateController.EventLog!;
        WorldTimeSnapshot advancedTime = separateController.WorldTime!;
        if (advancedLog.Entries.Count != 2
            || !ReferenceEquals(advancedLog.Entries[0], noAdvanceEvent.EventId)
            || !ReferenceEquals(advancedLog.Entries[1], advancingEvent.EventId)
            || ReferenceEquals(advancedLog, noAdvanceLog)
            || ReferenceEquals(advancedTime, initialTime)
            || !ReferenceEquals(advancedTime.CurrentEventId, advancingEvent.EventId)
            || separateController.TryInitialize(new WorldEventSnapshot([]), new([]), new(null))
            || !ReferenceEquals(separateController.WorldEvents, worldEvents)
            || !ReferenceEquals(separateController.EventLog, advancedLog)
            || !ReferenceEquals(separateController.WorldTime, advancedTime))
        {
            Fail("World-event controller failed exact atomic result installation or one-time initialization.");
            return false;
        }

        return true;
    }

    private bool ProvesRejectedOccurrencesPreserveEverySnapshot(WorldEvent sourceEvent)
    {
        WorldEventController uninitialized = new();
        AddChild(uninitialized);
        if (uninitialized.TryOccur(sourceEvent.EventId)
            || uninitialized.FindWorldEvent(sourceEvent.EventId) is not null
            || uninitialized.WorldEvents is not null
            || uninitialized.EventLog is not null
            || uninitialized.WorldTime is not null)
        {
            Fail("World-event controller changed uninitialized state after a rejected occurrence.");
            return false;
        }

        WorldEvent advancingEvent = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        WorldEventSnapshot events = new([advancingEvent]);
        WorldEventLogSnapshot log = new([]);
        WorldTimeSnapshot repeatedTime = new(new WorldEventId(advancingEvent.EventId.Value));
        WorldEventController repeated = new();
        AddChild(repeated);
        if (!repeated.TryInitialize(events, log, repeatedTime)
            || repeated.TryOccur(advancingEvent.EventId)
            || repeated.TryOccur(new WorldEventId("absent-event"))
            || repeated.FindWorldEvent(new WorldEventId("absent-event")) is not null
            || !ReferenceEquals(repeated.WorldEvents, events)
            || !ReferenceEquals(repeated.EventLog, log)
            || !ReferenceEquals(repeated.WorldTime, repeatedTime)
            || !RejectsNullInputs(repeated, events, log, repeatedTime))
        {
            Fail("World-event controller failed rejection or exact snapshot preservation.");
            return false;
        }

        return true;
    }

    private static bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(WorldEventController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .OrderBy(property => property.Name)
            .ToArray();
        System.Reflection.MethodInfo[] methods = typeof(WorldEventController)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name)
            .ToArray();

        return properties.Length == 3
            && IsPrivateSetProperty(properties[0], "EventLog", typeof(WorldEventLogSnapshot))
            && IsPrivateSetProperty(properties[1], "WorldEvents", typeof(WorldEventSnapshot))
            && IsPrivateSetProperty(properties[2], "WorldTime", typeof(WorldTimeSnapshot))
            && methods.Length == 4
            && methods[0].Name == nameof(WorldEventController.FindWorldEvent)
            && methods[0].ReturnType == typeof(WorldEvent)
            && HasParameters(methods[0], typeof(WorldEventId))
            && methods[1].Name == nameof(WorldEventController.TryInitialize)
            && methods[1].ReturnType == typeof(bool)
            && HasParameters(
                methods[1],
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot))
            && methods[2].Name == nameof(WorldEventController.TryOccur)
            && methods[2].ReturnType == typeof(bool)
            && HasParameters(methods[2], typeof(WorldEventId))
            && methods[3].Name == nameof(WorldEventController.TryRestoreWorldEventState)
            && methods[3].ReturnType == typeof(bool)
            && HasParameters(methods[3], typeof(Allrpcs.Domain.Persistence.WorldEventSaveState));
    }

    private static bool IsPrivateSetProperty(
        System.Reflection.PropertyInfo property,
        string name,
        Type propertyType) =>
        property.Name == name
        && property.PropertyType == propertyType
        && property.CanRead
        && property.SetMethod is not null
        && property.SetMethod.IsPrivate;

    private static bool HasParameters(
        System.Reflection.MethodInfo method,
        params Type[] parameterTypes) =>
        method.GetParameters().Select(parameter => parameter.ParameterType).SequenceEqual(parameterTypes);

    private static bool RejectsNullInputs(
        WorldEventController target,
        WorldEventSnapshot events,
        WorldEventLogSnapshot log,
        WorldTimeSnapshot time) =>
        ThrowsArgumentNull(() => target.FindWorldEvent(null!))
        && ThrowsArgumentNull(() => target.TryOccur(null!))
        && ThrowsArgumentNull(() => target.TryInitialize(null!, log, time))
        && ThrowsArgumentNull(() => target.TryInitialize(events, null!, time))
        && ThrowsArgumentNull(() => target.TryInitialize(events, log, null!))
        && ReferenceEquals(target.WorldEvents, events)
        && ReferenceEquals(target.EventLog, log)
        && ReferenceEquals(target.WorldTime, time);

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
