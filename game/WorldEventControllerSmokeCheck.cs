using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free world-event controller checks when explicitly enabled.</summary>
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
        WorldEvent sourceEvent = sourceSnapshot.Entries.Single();
        WorldEventLogSnapshot expectedEventLog = eventLogController.EventLog;
        WorldTimeSnapshot expectedWorldTime = worldTimeController.WorldTime;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;
        Vector2 expectedPosition = seeker.Position;
        Vector2 expectedVelocity = seeker.Velocity;
        string expectedFocusStatus = focusStatus.Text;
        string expectedFocusedSearchResult = focusedSearchResult.Text;

        WorldEventSnapshot? initializedSnapshot = controller.WorldEvents;
        WorldEventController uninitializedController = new();
        AddChild(uninitializedController);
        if (initializedSnapshot is null
            || initializedSnapshot.Entries.Count != 1
            || !ReferenceEquals(initializedSnapshot.Entries[0], sourceEvent)
            || !ReferenceEquals(initializedSnapshot, sourceSnapshot)
            || !ReferenceEquals(controller.FindWorldEvent(sourceEvent.EventId), sourceEvent)
            || uninitializedController.FindWorldEvent(sourceEvent.EventId) is not null
            || !HasExpectedControllerSurface())
        {
            Fail("World-event controller failed its exact scene initialization boundary.");
            return;
        }

        WorldEvent secondEvent = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        WorldEventSnapshot separateSnapshot = new([sourceEvent, secondEvent]);
        if (!uninitializedController.TryInitializeWorldEvents(separateSnapshot)
            || !ReferenceEquals(uninitializedController.WorldEvents, separateSnapshot))
        {
            Fail("World-event controller rejected exact one-time snapshot initialization.");
            return;
        }

        WorldEventId valueEqualSecondId = new(secondEvent.EventId.Value);
        if (!ReferenceEquals(controller.FindWorldEvent(sourceEvent.EventId), sourceEvent)
            || !ReferenceEquals(uninitializedController.FindWorldEvent(valueEqualSecondId), secondEvent)
            || controller.FindWorldEvent(new WorldEventId("absent-event")) is not null
            || controller.TryInitializeWorldEvents(new WorldEventSnapshot([]))
            || !ReferenceEquals(controller.WorldEvents, initializedSnapshot)
            || !RejectsNullInputs())
        {
            Fail("World-event controller failed lookup, rejection, or state preservation.");
            return;
        }

        if (!ReferenceEquals(source.WorldEvents, sourceSnapshot)
            || !ReferenceEquals(sourceSnapshot.Entries.Single(), sourceEvent)
            || !ReferenceEquals(eventLogController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldTimeController.WorldTime, expectedWorldTime)
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
            Fail("World-event controller changed source, world, gameplay, or presentation state.");
            return;
        }

        GD.Print(
            "World-event controller smoke passed: optional minimal ownership, exact scene and "
            + "one-time initialization, exact and value-equal lookup, absent and null rejection, and "
            + "unchanged source, identifier log, world time, world objects, perception, "
            + "focused search, movement, input, and presentation state were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo[] properties = typeof(WorldEventController)
            .GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly);
        System.Reflection.MethodInfo[] methods = typeof(WorldEventController)
            .GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .ToArray();

        return properties.Length == 1
            && properties[0].Name == nameof(WorldEventController.WorldEvents)
            && properties[0].PropertyType == typeof(WorldEventSnapshot)
            && properties[0].CanRead
            && properties[0].SetMethod is not null
            && properties[0].SetMethod!.IsPrivate
            && methods.Length == 2
            && methods.Any(
                method => method.Name == nameof(WorldEventController.TryInitializeWorldEvents)
                    && method.ReturnType == typeof(bool)
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(WorldEventSnapshot))
            && methods.Any(
                method => method.Name == nameof(WorldEventController.FindWorldEvent)
                    && method.ReturnType == typeof(WorldEvent)
                    && method.GetParameters().Length == 1
                    && method.GetParameters()[0].ParameterType == typeof(WorldEventId));
    }

    private bool RejectsNullInputs()
    {
        try
        {
            controller.TryInitializeWorldEvents(null!);
        }
        catch (ArgumentNullException)
        {
            try
            {
                controller.FindWorldEvent(null!);
            }
            catch (ArgumentNullException)
            {
                return true;
            }
        }

        return false;
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
