using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskArrivalOccurrenceTransitionTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void FinalWaypointReturnsOneCompleteAtomicResult()
    {
        Scenario scenario = CreateScenario();

        CharacterIndependentTaskArrivalOccurrenceResult result =
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                scenario.Candidate,
                scenario.Activities,
                scenario.WorldEvents,
                scenario.EventLog,
                scenario.WorldTime,
                scenario.Transcript);

        Assert.True(result.TravelProgress.IsComplete);
        Assert.Same(scenario.Plan, result.TravelProgress.Plan);
        Assert.Equal(2, result.TravelProgress.CompletedWaypointCount);
        Assert.Same(scenario.Candidate, result.TaskOccurrenceResult.Candidate);
        CharacterIndependentTaskActivity completed =
            Assert.Single(result.TaskOccurrenceResult.Activities.Activities);
        Assert.Same(scenario.Activity.Assignment, completed.Assignment);
        Assert.Same(CharacterIndependentTaskActivityState.Completed, completed.State);
        Assert.Collection(
            result.TaskOccurrenceResult.WorldEventResult.WorldEventResult.EventLog.Entries,
            eventId => Assert.Same(scenario.WorldEvent.EventId, eventId));
        Assert.Same(
            scenario.WorldEvent.EventId,
            result.TaskOccurrenceResult.WorldEventResult.WorldEventResult.WorldTime.CurrentEventId);
        Assert.Collection(
            result.TaskOccurrenceResult.WorldEventResult.Transcript.Entries,
            entry => Assert.Same(scenario.Candidate.Occurrence.TranscriptEntry, entry));
        Assert.Equal(1, scenario.Progress.CompletedWaypointCount);
        Assert.Same(scenario.Activity, scenario.Activities.Activities[0]);
        Assert.Empty(scenario.EventLog.Entries);
        Assert.Null(scenario.WorldTime.CurrentEventId);
        Assert.Empty(scenario.Transcript.Entries);
    }

    [Fact]
    public void NonFinalSkippedRepeatedAndDistinctPlanTravelAreRejected()
    {
        Scenario scenario = CreateScenario(progressAtStart: true);

        Assert.Throws<InvalidOperationException>(() => ArriveAndOccur(
            scenario,
            scenario.Plan.Waypoints[0]));
        Assert.Throws<InvalidOperationException>(() => ArriveAndOccur(
            scenario,
            scenario.Plan.Waypoints[1]));

        CharacterIndependentTaskTravelProgress completed = Complete(scenario.Plan);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                completed,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                scenario.Candidate,
                scenario.Activities,
                scenario.WorldEvents,
                scenario.EventLog,
                scenario.WorldTime,
                scenario.Transcript));

        CharacterIndependentTaskTravelPlan distinctPlan = Plan(scenario.Activity);
        CharacterIndependentTaskTravelProgress distinctProgress = AdvanceOnce(distinctPlan);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                distinctProgress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                scenario.Candidate,
                scenario.Activities,
                scenario.WorldEvents,
                scenario.EventLog,
                scenario.WorldTime,
                scenario.Transcript));
        AssertUnchanged(scenario, 0);
    }

    [Fact]
    public void DistinctActivityStaleTaskAndIneligibleEventAreRejected()
    {
        Scenario scenario = CreateScenario();
        CharacterIndependentTaskActivity distinctActivity = Activity();
        CharacterIndependentTaskWorldEventOccurrence distinctCandidate = Candidate(
            distinctActivity,
            scenario.WorldEvent);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                distinctCandidate,
                scenario.Activities,
                scenario.WorldEvents,
                scenario.EventLog,
                scenario.WorldTime,
                scenario.Transcript));

        CharacterIndependentTaskActivitySnapshot staleActivities = new([Activity("other-task")]);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                scenario.Candidate,
                staleActivities,
                scenario.WorldEvents,
                scenario.EventLog,
                scenario.WorldTime,
                scenario.Transcript));

        WorldEventLogSnapshot ineligibleEventLog = new([scenario.WorldEvent.EventId]);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                scenario.Candidate,
                scenario.Activities,
                scenario.WorldEvents,
                ineligibleEventLog,
                scenario.WorldTime,
                scenario.Transcript));
        AssertUnchanged(scenario, 1);
        Assert.Equal("other-task", staleActivities.Activities[0].Assignment.TaskId.Value);
        Assert.Collection(
            ineligibleEventLog.Entries,
            eventId => Assert.Same(scenario.WorldEvent.EventId, eventId));
    }

    [Fact]
    public void NullInputsAreRejected()
    {
        Scenario scenario = CreateScenario();

        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            null!,
            scenario.Plan,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            null!,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            null!,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            scenario.Candidate,
            null!,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            scenario.Candidate,
            scenario.Activities,
            null!,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            null!,
            scenario.WorldTime,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            null!,
            scenario.Transcript));
        Assert.Throws<ArgumentNullException>(() => Transition(
            scenario,
            scenario.Progress,
            scenario.Plan,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            null!));
    }

    private static CharacterIndependentTaskArrivalOccurrenceResult ArriveAndOccur(
        Scenario scenario,
        Vector2 reachedWaypoint) =>
        CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
            scenario.Progress,
            scenario.Plan,
            reachedWaypoint,
            scenario.Candidate,
            scenario.Activities,
            scenario.WorldEvents,
            scenario.EventLog,
            scenario.WorldTime,
            scenario.Transcript);

    private static CharacterIndependentTaskArrivalOccurrenceResult Transition(
        Scenario scenario,
        CharacterIndependentTaskTravelProgress progress,
        CharacterIndependentTaskTravelPlan plan,
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
            progress,
            plan,
            scenario.Plan.Waypoints[^1],
            candidate,
            activities,
            worldEvents,
            eventLog,
            worldTime,
            transcript);

    private static Scenario CreateScenario(bool progressAtStart = false)
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan plan = Plan(activity);
        CharacterIndependentTaskTravelProgress progress = progressAtStart
            ? new CharacterIndependentTaskTravelProgress(plan)
            : AdvanceOnce(plan);
        WorldEvent worldEvent = new(
            new WorldEventId("healer-examined-herb-patch"),
            WorldEventTimeEffect.Advance);
        return new Scenario(
            activity,
            plan,
            progress,
            worldEvent,
            Candidate(activity, worldEvent),
            new CharacterIndependentTaskActivitySnapshot([activity]),
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([]));
    }

    private static CharacterIndependentTaskTravelPlan Plan(
        CharacterIndependentTaskActivity activity) =>
        new(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(400.0f, 225.0f), new Vector2(800.0f, 450.0f)]);

    private static CharacterIndependentTaskTravelProgress AdvanceOnce(
        CharacterIndependentTaskTravelPlan plan) =>
        CharacterIndependentTaskTravelProgressTransition.Advance(
            new CharacterIndependentTaskTravelProgress(plan),
            plan,
            plan.Waypoints[0]);

    private static CharacterIndependentTaskTravelProgress Complete(
        CharacterIndependentTaskTravelPlan plan) =>
        CharacterIndependentTaskTravelProgressTransition.Advance(
            AdvanceOnce(plan),
            plan,
            plan.Waypoints[1]);

    private static CharacterIndependentTaskWorldEventOccurrence Candidate(
        CharacterIndependentTaskActivity activity,
        WorldEvent worldEvent) =>
        new(
            activity,
            new CharacterTurnWorldEventOccurrence(
                worldEvent,
                new CharacterTurnTranscriptEntry(
                    activity.Assignment.CharacterId,
                    activity.Assignment.Intention)));

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

    private static void AssertUnchanged(Scenario scenario, int completedWaypointCount)
    {
        Assert.Equal(completedWaypointCount, scenario.Progress.CompletedWaypointCount);
        Assert.Same(scenario.Activity, scenario.Activities.Activities[0]);
        Assert.Empty(scenario.EventLog.Entries);
        Assert.Null(scenario.WorldTime.CurrentEventId);
        Assert.Empty(scenario.Transcript.Entries);
    }

    private sealed record Scenario(
        CharacterIndependentTaskActivity Activity,
        CharacterIndependentTaskTravelPlan Plan,
        CharacterIndependentTaskTravelProgress Progress,
        WorldEvent WorldEvent,
        CharacterIndependentTaskWorldEventOccurrence Candidate,
        CharacterIndependentTaskActivitySnapshot Activities,
        WorldEventSnapshot WorldEvents,
        WorldEventLogSnapshot EventLog,
        WorldTimeSnapshot WorldTime,
        CharacterTurnTranscriptSnapshot Transcript);
}
