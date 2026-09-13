using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Godot;
using Vector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Runs credential-free scene-owned independent-task return-progress checks.</summary>
public partial class CharacterIndependentTaskReturnTravelControllerSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_INDEPENDENT_TASK_RETURN_TRAVEL_CONTROLLER_CHECK";
    private const string EnabledEnvironmentValue = "1";

    /// <inheritdoc />
    public override void _Ready()
    {
        if (OS.GetEnvironment(CheckEnvironmentVariable) != EnabledEnvironmentValue)
        {
            return;
        }

        CharacterIndependentTaskReturnTravelController controller = new();
        CharacterIndependentTaskReturnTravelPlan plan = Plan();

        if (!RejectsNullInitialization(controller)
            || !controller.TryInitialize(plan)
            || !ReferenceEquals(controller.Plan, plan)
            || controller.Progress is not { } initial
            || !ReferenceEquals(initial.Plan, plan)
            || initial.CompletedWaypointCount != 0
            || initial.IsComplete
            || controller.TryInitialize(Plan()))
        {
            Fail("Return-travel controller rejected exact one-time plan initialization.");
            return;
        }

        CharacterIndependentTaskReturnTravelPlan stalePlan = Plan();
        CharacterIndependentTaskReturnTravelProgress staleProgress = new(plan);
        if (controller.TryAdvance(stalePlan, initial, stalePlan.Waypoints[0])
            || controller.TryAdvance(plan, staleProgress, plan.Waypoints[0])
            || controller.TryAdvance(plan, initial, plan.Waypoints[1])
            || !HasExactState(controller, plan, initial))
        {
            Fail("Return-travel controller changed state after stale or skipped input.");
            return;
        }

        if (!controller.TryAdvance(plan, initial, plan.Waypoints[0])
            || controller.Progress is not { } first
            || ReferenceEquals(first, initial)
            || !ReferenceEquals(first.Plan, plan)
            || first.CompletedWaypointCount != 1
            || first.IsComplete
            || initial.CompletedWaypointCount != 0
            || controller.TryAdvance(plan, first, plan.Waypoints[0])
            || !HasExactState(controller, plan, first))
        {
            Fail("Return-travel controller failed immutable exact first-waypoint advancement.");
            return;
        }

        if (!controller.TryAdvance(plan, first, plan.Waypoints[1])
            || controller.Progress is not { } completed
            || ReferenceEquals(completed, first)
            || completed.CompletedWaypointCount != 2
            || !completed.IsComplete
            || first.CompletedWaypointCount != 1
            || controller.TryAdvance(plan, completed, plan.Waypoints[1])
            || !HasExactState(controller, plan, completed)
            || !RejectsNullAdvanceInputs(controller, plan, completed))
        {
            Fail("Return-travel controller failed exact completion or repeated-input rejection.");
            return;
        }

        GD.Print(
            "Independent-task return-travel controller smoke passed: exact one-time plan ownership, "
            + "compare-and-swap waypoint advancement, immutable completion, and fail-closed stale, "
            + "skipped, repeated, and null input.");
        controller.Free();
        GetTree().Quit();
    }

    private static CharacterIndependentTaskReturnTravelPlan Plan()
    {
        CharacterIndependentTaskActivity activity = new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId("inspect-herb-patch"),
                new CharacterId("travelling-healer"),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch"))),
            CharacterIndependentTaskActivityState.Active);
        CharacterIndependentTaskTravelPlan outboundPlan = new(
            activity,
            new NavigationWorldBounds(0.0f, 0.0f, 1600.0f, 900.0f),
            Vector2.Zero,
            [new Vector2(800.0f, 450.0f)]);
        WorldEvent worldEvent = new(
            new WorldEventId("healer-examined-herb-patch"),
            WorldEventTimeEffect.Advance);
        CharacterIndependentTaskArrivalOccurrenceResult arrival =
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                new CharacterIndependentTaskTravelProgress(outboundPlan),
                outboundPlan,
                outboundPlan.Waypoints[0],
                new CharacterIndependentTaskWorldEventOccurrence(
                    activity,
                    new CharacterTurnWorldEventOccurrence(
                        worldEvent,
                        new CharacterTurnTranscriptEntry(
                            activity.Assignment.CharacterId,
                            activity.Assignment.Intention))),
                new CharacterIndependentTaskActivitySnapshot([activity]),
                new WorldEventSnapshot([worldEvent]),
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null),
                new CharacterTurnTranscriptSnapshot([]));
        CharacterIndependentTaskActivity completed =
            arrival.TaskOccurrenceResult.Activities.Activities.Single();

        return new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            outboundPlan.WorldBounds,
            outboundPlan.Waypoints[^1],
            [new Vector2(400.0f, 225.0f), outboundPlan.Origin]);
    }

    private static bool HasExactState(
        CharacterIndependentTaskReturnTravelController controller,
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress) =>
        ReferenceEquals(controller.Plan, plan)
        && ReferenceEquals(controller.Progress, progress);

    private static bool RejectsNullInitialization(
        CharacterIndependentTaskReturnTravelController controller) =>
        ThrowsArgumentNull(() => controller.TryInitialize(null!))
        && controller.Plan is null
        && controller.Progress is null;

    private static bool RejectsNullAdvanceInputs(
        CharacterIndependentTaskReturnTravelController controller,
        CharacterIndependentTaskReturnTravelPlan plan,
        CharacterIndependentTaskReturnTravelProgress progress) =>
        ThrowsArgumentNull(() => controller.TryAdvance(null!, progress, Vector2.One))
        && ThrowsArgumentNull(() => controller.TryAdvance(plan, null!, Vector2.One));

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

    private void Fail(string message)
    {
        GD.PushError(message);
        GetTree().Quit(1);
    }
}
