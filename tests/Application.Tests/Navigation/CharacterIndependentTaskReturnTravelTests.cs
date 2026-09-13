using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskReturnTravelTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void PlanRetainsExactCompletedArrivalAndStableCopiedReturnRoute()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);
        Vector2[] source = [new(400.0f, 225.0f), scenario.OutboundPlan.Origin];

        CharacterIndependentTaskReturnTravelPlan plan = new(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            source);

        source[0] = Vector2.One;

        Assert.Same(arrival, plan.ArrivalOccurrenceResult);
        Assert.Same(completed, plan.CompletedActivity);
        Assert.Equal(TestWorldBounds, plan.WorldBounds);
        Assert.Equal(scenario.OutboundPlan.Waypoints[^1], plan.Origin);
        Assert.Equal([new Vector2(400.0f, 225.0f), Vector2.Zero], plan.Waypoints);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Vector2>)plan.Waypoints).Add(Vector2.One));
        Assert.All(
            typeof(CharacterIndependentTaskReturnTravelPlan).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PlanAcceptsMaximumRouteAndWorldBoundaryCoordinates()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);
        Vector2[] waypoints = Enumerable
            .Range(0, CharacterIndependentTaskReturnTravelPlan.MaximumWaypointCount)
            .Select(index => index == CharacterIndependentTaskReturnTravelPlan.MaximumWaypointCount - 1
                ? scenario.OutboundPlan.Origin
                : index % 2 == 0
                    ? new Vector2(TestWorldBounds.Left, TestWorldBounds.Bottom)
                    : new Vector2(TestWorldBounds.Right, TestWorldBounds.Top))
            .ToArray();

        CharacterIndependentTaskReturnTravelPlan plan = new(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            waypoints);

        Assert.Equal(CharacterIndependentTaskReturnTravelPlan.MaximumWaypointCount, plan.Waypoints.Count);
        Assert.Equal(waypoints, plan.Waypoints);
    }

    [Fact]
    public void PlanRejectsIncompleteOrMismatchedOutboundTravel()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);
        CharacterIndependentTaskTravelPlan distinctPlan = OutboundPlan(Activity());
        CharacterIndependentTaskArrivalOccurrenceResult incomplete = new(
            new CharacterIndependentTaskTravelProgress(scenario.OutboundPlan),
            arrival.TaskOccurrenceResult);
        CharacterIndependentTaskArrivalOccurrenceResult mismatched = new(
            Complete(distinctPlan),
            arrival.TaskOccurrenceResult);

        Assert.Throws<ArgumentException>(() => ReturnPlan(incomplete, completed, scenario));
        Assert.Throws<ArgumentException>(() => ReturnPlan(mismatched, completed, scenario));
    }

    [Fact]
    public void PlanRejectsFailedOrMismatchedTaskResults()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);
        CharacterIndependentTaskActivity active = scenario.Activity;
        CharacterIndependentTaskActivity distinctCompleted = new(
            completed.Assignment,
            CharacterIndependentTaskActivityState.Completed);
        CharacterIndependentTaskWorldEventOccurrenceResult activeResult = new(
            arrival.TaskOccurrenceResult.Candidate,
            new CharacterIndependentTaskActivitySnapshot([active]),
            arrival.TaskOccurrenceResult.WorldEventResult);
        CharacterIndependentTaskArrivalOccurrenceResult failed = new(
            arrival.TravelProgress,
            activeResult);

        Assert.Throws<ArgumentException>(() => ReturnPlan(failed, active, scenario));
        Assert.Throws<ArgumentException>(() => ReturnPlan(arrival, distinctCompleted, scenario));

        CharacterIndependentTaskActivity otherActivity = Activity("other-task");
        CharacterIndependentTaskTravelPlan otherPlan = OutboundPlan(otherActivity);
        CharacterIndependentTaskArrivalOccurrenceResult mismatched = new(
            Complete(otherPlan),
            arrival.TaskOccurrenceResult);
        Assert.Throws<ArgumentException>(() => ReturnPlan(mismatched, completed, scenario));
    }

    [Fact]
    public void PlanRejectsInvalidOriginDestinationBoundsAndRouteSize()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);

        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            TestWorldBounds,
            Vector2.One,
            [scenario.OutboundPlan.Origin]));
        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            [Vector2.One]));
        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            Enumerable.Repeat(
                scenario.OutboundPlan.Origin,
                CharacterIndependentTaskReturnTravelPlan.MaximumWaypointCount + 1)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            new NavigationWorldBounds(0.0f, 0.0f, 0.0f, 900.0f),
            scenario.OutboundPlan.Waypoints[^1],
            [scenario.OutboundPlan.Origin]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            [new Vector2(float.NaN, 0.0f), scenario.OutboundPlan.Origin]));
    }

    [Fact]
    public void ProgressAdvancesOneExactWaypointAndCompletesOnlyAtTheEnd()
    {
        CharacterIndependentTaskReturnTravelPlan plan = ReturnPlan();
        CharacterIndependentTaskReturnTravelProgress initial = new(plan);

        CharacterIndependentTaskReturnTravelProgress first =
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[0]);
        CharacterIndependentTaskReturnTravelProgress completed =
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                first,
                plan,
                plan.Waypoints[1]);

        Assert.Same(plan, initial.Plan);
        Assert.Equal(0, initial.CompletedWaypointCount);
        Assert.False(initial.IsComplete);
        Assert.Same(plan, first.Plan);
        Assert.Equal(1, first.CompletedWaypointCount);
        Assert.False(first.IsComplete);
        Assert.Same(plan, completed.Plan);
        Assert.Equal(2, completed.CompletedWaypointCount);
        Assert.True(completed.IsComplete);
        Assert.Equal(0, initial.CompletedWaypointCount);
        Assert.Equal(1, first.CompletedWaypointCount);
    }

    [Fact]
    public void ProgressRejectsSkipsRepeatsCompletionDistinctPlansAndNulls()
    {
        CharacterIndependentTaskReturnTravelPlan plan = ReturnPlan();
        CharacterIndependentTaskReturnTravelProgress initial = new(plan);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[1]));
        CharacterIndependentTaskReturnTravelProgress first =
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[0]);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                first,
                plan,
                plan.Waypoints[0]));
        CharacterIndependentTaskReturnTravelProgress completed =
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                first,
                plan,
                plan.Waypoints[1]);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                completed,
                plan,
                plan.Waypoints[1]));

        CharacterIndependentTaskReturnTravelPlan distinctPlan = ReturnPlan();
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                initial,
                distinctPlan,
                distinctPlan.Waypoints[0]));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskReturnTravelProgress(null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(null!, plan, Vector2.One));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnTravelProgressTransition.Advance(initial, null!, Vector2.One));
        Assert.Equal(0, initial.CompletedWaypointCount);
    }

    [Fact]
    public void PlanRejectsNulls()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);

        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskReturnTravelPlan(
            null!, completed, TestWorldBounds, scenario.OutboundPlan.Waypoints[^1], [Vector2.Zero]));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival, null!, TestWorldBounds, scenario.OutboundPlan.Waypoints[^1], [Vector2.Zero]));
        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskReturnTravelPlan(
            arrival, completed, TestWorldBounds, scenario.OutboundPlan.Waypoints[^1], null!));
    }

    private static CharacterIndependentTaskReturnTravelPlan ReturnPlan()
    {
        ArrivalScenario scenario = CreateArrivalScenario();
        CharacterIndependentTaskArrivalOccurrenceResult arrival = ArriveAndOccur(scenario);
        CharacterIndependentTaskActivity completed = Assert.Single(
            arrival.TaskOccurrenceResult.Activities.Activities);
        return ReturnPlan(arrival, completed, scenario);
    }

    private static CharacterIndependentTaskReturnTravelPlan ReturnPlan(
        CharacterIndependentTaskArrivalOccurrenceResult arrival,
        CharacterIndependentTaskActivity completed,
        ArrivalScenario scenario) =>
        new(
            arrival,
            completed,
            TestWorldBounds,
            scenario.OutboundPlan.Waypoints[^1],
            [new Vector2(400.0f, 225.0f), scenario.OutboundPlan.Origin]);

    private static CharacterIndependentTaskArrivalOccurrenceResult ArriveAndOccur(
        ArrivalScenario scenario) =>
        CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
            scenario.Progress,
            scenario.OutboundPlan,
            scenario.OutboundPlan.Waypoints[^1],
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([]));

    private static ArrivalScenario CreateArrivalScenario()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan outboundPlan = OutboundPlan(activity);
        CharacterIndependentTaskTravelProgress progress =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                new CharacterIndependentTaskTravelProgress(outboundPlan),
                outboundPlan,
                outboundPlan.Waypoints[0]);
        WorldEvent worldEvent = new(
            new WorldEventId("healer-examined-herb-patch"),
            WorldEventTimeEffect.Advance);
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            new CharacterTurnWorldEventOccurrence(
                worldEvent,
                new CharacterTurnTranscriptEntry(
                    activity.Assignment.CharacterId,
                    activity.Assignment.Intention)));
        return new ArrivalScenario(
            activity,
            outboundPlan,
            progress,
            candidate,
            new CharacterIndependentTaskActivitySnapshot([activity]),
            new WorldEventSnapshot([worldEvent]));
    }

    private static CharacterIndependentTaskTravelPlan OutboundPlan(
        CharacterIndependentTaskActivity activity) =>
        new(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(400.0f, 225.0f), new Vector2(800.0f, 450.0f)]);

    private static CharacterIndependentTaskTravelProgress Complete(
        CharacterIndependentTaskTravelPlan plan)
    {
        CharacterIndependentTaskTravelProgress progress = new(plan);
        foreach (Vector2 waypoint in plan.Waypoints)
        {
            progress = CharacterIndependentTaskTravelProgressTransition.Advance(
                progress,
                plan,
                waypoint);
        }

        return progress;
    }

    private static CharacterIndependentTaskActivity Activity(
        string taskId = "inspect-herb-patch") =>
        new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId(taskId),
                new CharacterId("travelling-healer"),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch"))),
            CharacterIndependentTaskActivityState.Active);

    private sealed record ArrivalScenario(
        CharacterIndependentTaskActivity Activity,
        CharacterIndependentTaskTravelPlan OutboundPlan,
        CharacterIndependentTaskTravelProgress Progress,
        CharacterIndependentTaskWorldEventOccurrence Candidate,
        CharacterIndependentTaskActivitySnapshot Activities,
        WorldEventSnapshot WorldEvents);
}
