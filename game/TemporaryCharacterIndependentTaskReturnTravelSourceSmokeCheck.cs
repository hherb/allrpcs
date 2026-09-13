using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one exact authored scene return route after independent-task completion.</summary>
public partial class TemporaryCharacterIndependentTaskReturnTravelSourceSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_INDEPENDENT_TASK_RETURN_TRAVEL_SOURCE_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        SetProcess(OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue);
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        SetProcess(false);

        TemporaryCharacterIndependentTaskReturnTravelSource source =
            GetNode<TemporaryCharacterIndependentTaskReturnTravelSource>(
                "../TemporaryCharacterIndependentTaskReturnTravelSource");
        TemporaryCharacterIndependentTaskOccurrenceSource occurrenceSource =
            GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>(
                "../TemporaryCharacterIndependentTaskOccurrenceSource");
        CharacterIndependentTaskDepartureController taskController =
            GetNode<CharacterIndependentTaskDepartureController>(
                "../CharacterIndependentTaskDepartureController");
        CharacterIndependentTaskTravelController outboundController =
            GetNode<CharacterIndependentTaskTravelController>(
                "../CharacterIndependentTaskTravelController");
        CharacterIndependentTaskReturnTravelController returnController =
            GetNode<CharacterIndependentTaskReturnTravelController>(
                "../CharacterIndependentTaskReturnTravelController");
        WorldEventController worldController =
            GetNode<WorldEventController>("../WorldEventController");
        CharacterTurnTranscriptController transcriptController =
            GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");

        CharacterIndependentTaskActivity active = taskController.ActiveActivity!;
        CharacterIndependentTaskTravelPlan outboundPlan = outboundController.Plan!;
        CharacterIndependentTaskTravelProgress outboundProgress = outboundController.Progress!;
        CharacterIndependentTaskActivitySnapshot activities = taskController.Activities!;
        WorldEventLogSnapshot eventLog = worldController.EventLog!;
        WorldTimeSnapshot worldTime = worldController.WorldTime!;
        CharacterTurnTranscriptSnapshot transcript = transcriptController.Transcript!;
        CharacterIndependentTaskArrivalOccurrenceResult arrival =
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                outboundProgress,
                outboundPlan,
                outboundPlan.Waypoints.Single(),
                occurrenceSource.CreateOccurrence(active),
                activities,
                worldController.WorldEvents!,
                eventLog,
                worldTime,
                transcript);
        CharacterIndependentTaskActivity completed =
            arrival.TaskOccurrenceResult.Activities.Activities.Single();

        CharacterIndependentTaskReturnTravelPlan plan = source.CreatePlan(arrival, completed);

        if (!ReferenceEquals(plan.ArrivalOccurrenceResult, arrival)
            || !ReferenceEquals(plan.CompletedActivity, completed)
            || plan.WorldBounds != outboundPlan.WorldBounds
            || plan.Origin != outboundPlan.Waypoints[^1]
            || plan.Waypoints.Count != 1
            || plan.Waypoints[0] != outboundPlan.Origin
            || returnController.Plan is not null
            || returnController.Progress is not null
            || !ReferenceEquals(taskController.ActiveActivity, active)
            || !ReferenceEquals(taskController.Activities, activities)
            || !ReferenceEquals(outboundController.Plan, outboundPlan)
            || !ReferenceEquals(outboundController.Progress, outboundProgress)
            || !ReferenceEquals(worldController.EventLog, eventLog)
            || !ReferenceEquals(worldController.WorldTime, worldTime)
            || !ReferenceEquals(transcriptController.Transcript, transcript))
        {
            Fail("The authored return route lost exact values or changed scene-owned state.");
            return;
        }

        CharacterIndependentTaskReturnTravelPlan second = source.CreatePlan(arrival, completed);
        CharacterIndependentTaskActivity distinctCompleted = new(
            completed.Assignment,
            CharacterIndependentTaskActivityState.Completed);
        CharacterIndependentTaskAssignment otherAssignment = new(
            new CharacterIndependentTaskId("other-task"),
            active.Assignment.CharacterId,
            active.Assignment.Intention);
        CharacterIndependentTaskActivity otherCompleted = new(
            otherAssignment,
            CharacterIndependentTaskActivityState.Completed);
        if (ReferenceEquals(second, plan)
            || !ReferenceEquals(second.ArrivalOccurrenceResult, arrival)
            || !ReferenceEquals(second.CompletedActivity, completed)
            || !ThrowsArgumentNull(() => source.CreatePlan(null!, completed))
            || !ThrowsArgumentNull(() => source.CreatePlan(arrival, null!))
            || !ThrowsArgument(() => source.CreatePlan(arrival, distinctCompleted))
            || !ThrowsInvalidOperation(() => source.CreatePlan(arrival, otherCompleted))
            || !HasClosedSourceSurface())
        {
            Fail("The temporary return-route source accepted invalid input or exposed extra policy.");
            return;
        }

        GD.Print(
            "Temporary independent-task return-travel source smoke passed: the exact completed "
            + "healer task retains its successful arrival, herb-patch origin, and departure "
            + "destination in a fresh route without scene initialization or advancement.");
        GetTree().Quit();
    }

    private static bool HasClosedSourceSurface()
    {
        Type type = typeof(TemporaryCharacterIndependentTaskReturnTravelSource);
        System.Reflection.MethodInfo[] methods = type.GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly)
            .Where(method => !method.IsSpecialName)
            .OrderBy(method => method.Name, StringComparer.Ordinal)
            .ToArray();

        return methods.Length == 2
            && methods[0].Name == nameof(TemporaryCharacterIndependentTaskReturnTravelSource.CreatePlan)
            && methods[1].Name == nameof(_Ready)
            && type.GetProperties(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Instance
                    | System.Reflection.BindingFlags.DeclaredOnly).Length == 0;
    }

    private static bool ThrowsArgument(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentException)
        {
            return true;
        }
    }

    private static bool ThrowsArgumentNull(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (ArgumentNullException)
        {
            return true;
        }
    }

    private static bool ThrowsInvalidOperation(Action action)
    {
        try
        {
            action();
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
