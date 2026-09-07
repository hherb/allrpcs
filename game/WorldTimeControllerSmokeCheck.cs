using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs credential-free world-time controller checks when explicitly enabled.</summary>
public partial class WorldTimeControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_WORLD_TIME_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    private WorldTimeController controller = null!;
    private TemporaryWorldEventSource source = null!;
    private WorldEventLogController eventLogController = null!;
    private WorldObjectController worldObjectController = null!;
    private FocusedSearchActivityController focusedSearchController = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        controller = GetNode<WorldTimeController>("../WorldTimeController");
        source = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        eventLogController = GetNode<WorldEventLogController>("../WorldEventLogController");
        worldObjectController = GetNode<WorldObjectController>("../WorldObjectController");
        focusedSearchController = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        WorldTimeSnapshot initialTime = controller.WorldTime;
        WorldEventLogSnapshot expectedEventLog = eventLogController.EventLog;
        WorldObjectSnapshot? expectedWorldObjects = worldObjectController.WorldObjects;
        FocusedSearchActivitySnapshot expectedActivities = focusedSearchController.Activities;
        FocusedSearchOutcomeSnapshot expectedOutcomes = focusedSearchController.Outcomes;
        CharacterPerceptionSnapshot? expectedPerception = focusedSearchController.Perception;

        if (initialTime.CurrentEventId is not null
            || controller.TryAdvance(source.WorldEvents.Entries.Single())
            || !ReferenceEquals(controller.WorldTime, initialTime)
            || !HasExpectedControllerSurface())
        {
            Fail("World-time controller failed initial state or no-advance rejection.");
            return;
        }

        WorldEventId advancingId = new("patient-stabilized");
        WorldEvent advancingEvent = new(advancingId, WorldEventTimeEffect.Advance);
        if (!controller.TryAdvance(advancingEvent))
        {
            Fail("World-time controller rejected an eligible advancing event.");
            return;
        }

        WorldTimeSnapshot advancedTime = controller.WorldTime;
        WorldEvent duplicate = new(
            new WorldEventId("patient-stabilized"),
            WorldEventTimeEffect.Advance);
        if (ReferenceEquals(initialTime, advancedTime)
            || !ReferenceEquals(advancedTime.CurrentEventId, advancingId)
            || controller.TryAdvance(duplicate)
            || !ReferenceEquals(controller.WorldTime, advancedTime)
            || !RejectsNullAdvance())
        {
            Fail("World-time controller failed exact advance or rejection-state identity.");
            return;
        }

        if (!ReferenceEquals(eventLogController.EventLog, expectedEventLog)
            || !ReferenceEquals(worldObjectController.WorldObjects, expectedWorldObjects)
            || !ReferenceEquals(focusedSearchController.Activities, expectedActivities)
            || !ReferenceEquals(focusedSearchController.Outcomes, expectedOutcomes)
            || !ReferenceEquals(focusedSearchController.Perception, expectedPerception)
            || !ReferenceEquals(
                source.WorldEvents.Entries.Single().TimeEffect,
                WorldEventTimeEffect.NoAdvance))
        {
            Fail("World-time controller changed event-log, source, world, or discovery state.");
            return;
        }

        GD.Print(
            "World-time controller smoke passed: explicit initial state, no-advance rejection, "
            + "exact advancing-event retention, value-equal repeat and null rejection, minimal "
            + "surface, and unchanged event-log, source, world-object, perception, and "
            + "focused-search state were correct.");
        GetTree().Quit();
    }

    private static bool HasExpectedControllerSurface()
    {
        System.Reflection.PropertyInfo property = AssertSingle(
            typeof(WorldTimeController).GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly));
        System.Reflection.MethodInfo method = AssertSingle(
            typeof(WorldTimeController)
                .GetMethods(
                    System.Reflection.BindingFlags.Public
                        | System.Reflection.BindingFlags.Instance
                        | System.Reflection.BindingFlags.DeclaredOnly)
                .Where(candidate => !candidate.IsSpecialName)
                .ToArray());

        return property.Name == nameof(WorldTimeController.WorldTime)
            && property.PropertyType == typeof(WorldTimeSnapshot)
            && property.SetMethod is not null
            && property.SetMethod.IsPrivate
            && method.Name == nameof(WorldTimeController.TryAdvance)
            && method.ReturnType == typeof(bool)
            && method.GetParameters().Length == 1
            && method.GetParameters()[0].ParameterType == typeof(WorldEvent);
    }

    private bool RejectsNullAdvance()
    {
        try
        {
            controller.TryAdvance(null!);
        }
        catch (ArgumentNullException)
        {
            return true;
        }

        return false;
    }

    private static T AssertSingle<T>(IReadOnlyCollection<T> values) =>
        values.Count == 1
            ? values.Single()
            : throw new InvalidOperationException("Expected one reflected member.");

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
