using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskReturnRejoinTransitionTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void FinalReturnWaypointRejoinsExactCharacterAtomically()
    {
        Scenario scenario = CreateScenario();
        CharacterId sage = new("sage");
        CharacterPartySnapshot party = new([sage]);

        CharacterIndependentTaskReturnRejoinResult result =
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                party);

        Assert.True(result.ReturnProgress.IsComplete);
        Assert.Same(scenario.Plan, result.ReturnProgress.Plan);
        Assert.Equal(2, result.ReturnProgress.CompletedWaypointCount);
        Assert.Collection(
            result.Party.MemberCharacterIds,
            member => Assert.Same(sage, member),
            member => Assert.Same(
                scenario.CompletedActivity.Assignment.CharacterId,
                member));
        Assert.Equal(1, scenario.Progress.CompletedWaypointCount);
        Assert.Collection(party.MemberCharacterIds, member => Assert.Same(sage, member));
    }

    [Fact]
    public void NonFinalSkippedRepeatedAndDistinctPlanReturnAreRejected()
    {
        Scenario scenario = CreateScenario(progressAtStart: true);
        CharacterPartySnapshot party = new([]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[0],
                party));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[1],
                party));

        CharacterIndependentTaskReturnTravelProgress completed = Complete(scenario.Plan);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                completed,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                party));

        Scenario distinct = CreateScenario();
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                distinct.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                party));
        Assert.Equal(0, scenario.Progress.CompletedWaypointCount);
        Assert.Empty(party.MemberCharacterIds);
    }

    [Fact]
    public void ExistingMemberAndFullPartyAreIneligibleWithoutPartialState()
    {
        Scenario scenario = CreateScenario();
        CharacterPartySnapshot containing = new(
            [new CharacterId(scenario.CompletedActivity.Assignment.CharacterId.Value)]);
        CharacterPartySnapshot full = new(Enumerable
            .Range(0, CharacterPartySnapshot.MaximumMemberCount)
            .Select(index => new CharacterId($"character-{index}")));

        Assert.False(CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
            scenario.Plan,
            Complete(scenario.Plan),
            containing));
        Assert.False(CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
            scenario.Plan,
            Complete(scenario.Plan),
            full));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                containing));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress,
                scenario.Plan,
                scenario.Plan.Waypoints[^1],
                full));
        Assert.Equal(1, scenario.Progress.CompletedWaypointCount);
        Assert.Single(containing.MemberCharacterIds);
        Assert.Equal(CharacterPartySnapshot.MaximumMemberCount, full.MemberCharacterIds.Count);
    }

    [Fact]
    public void ExactCompletedReturnEligibilityRejectsDistinctStateAndNulls()
    {
        Scenario scenario = CreateScenario();
        CharacterIndependentTaskReturnTravelProgress completed = Complete(scenario.Plan);
        Scenario distinct = CreateScenario();
        CharacterPartySnapshot party = new([]);

        Assert.True(CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
            scenario.Plan,
            completed,
            party));
        Assert.False(CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
            scenario.Plan,
            scenario.Progress,
            party));
        Assert.False(CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
            scenario.Plan,
            Complete(distinct.Plan),
            party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
                null!,
                completed,
                party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
                scenario.Plan,
                null!,
                party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinEligibilityPolicy.IsEligible(
                scenario.Plan,
                completed,
                null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                null!, scenario.Plan, scenario.Plan.Waypoints[^1], party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress, null!, scenario.Plan.Waypoints[^1], party));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskReturnRejoinTransition.ArriveAndRejoin(
                scenario.Progress, scenario.Plan, scenario.Plan.Waypoints[^1], null!));
    }

    private static Scenario CreateScenario(bool progressAtStart = false)
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
            TestWorldBounds,
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
        CharacterIndependentTaskActivity completedActivity =
            arrival.TaskOccurrenceResult.Activities.Activities.Single();
        CharacterIndependentTaskReturnTravelPlan plan = new(
            arrival,
            completedActivity,
            TestWorldBounds,
            outboundPlan.Waypoints[^1],
            [new Vector2(400.0f, 225.0f), outboundPlan.Origin]);
        CharacterIndependentTaskReturnTravelProgress progress = progressAtStart
            ? new CharacterIndependentTaskReturnTravelProgress(plan)
            : CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                new CharacterIndependentTaskReturnTravelProgress(plan),
                plan,
                plan.Waypoints[0]);
        return new Scenario(completedActivity, plan, progress);
    }

    private static CharacterIndependentTaskReturnTravelProgress Complete(
        CharacterIndependentTaskReturnTravelPlan plan)
    {
        CharacterIndependentTaskReturnTravelProgress progress = new(plan);
        foreach (Vector2 waypoint in plan.Waypoints)
        {
            progress = CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                progress,
                plan,
                waypoint);
        }

        return progress;
    }

    private sealed record Scenario(
        CharacterIndependentTaskActivity CompletedActivity,
        CharacterIndependentTaskReturnTravelPlan Plan,
        CharacterIndependentTaskReturnTravelProgress Progress);
}
