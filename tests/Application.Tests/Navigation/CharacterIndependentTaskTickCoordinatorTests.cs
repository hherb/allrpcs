using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskTickCoordinatorTests
{
    private static readonly NavigationWorldBounds TestWorldBounds = new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void OutboundStepOffersTheActiveTaskOccurrenceAtTheNextWaypointWithBoundedElapsed()
    {
        FakePorts ports = new();

        new CharacterIndependentTaskTickCoordinator(ports).Process(10.0d);

        Advance advance = Assert.Single(ports.Advances);
        Assert.Equal(TimeSpan.FromSeconds(3), advance.Elapsed);
        Assert.Equal(ports.OutboundPlan!.Waypoints[0], advance.ReachedWaypoint);
        Assert.Same(ports.ActiveActivity, Assert.Single(ports.OccurrenceRequests));
        Assert.Same(ports.Occurrence, advance.Candidate);
        Assert.Null(advance.ReturnPlan);
        Assert.Same(ports.Party, advance.Party);
        Assert.Empty(ports.Presentations);
    }

    [Fact]
    public void ShortFramesPassThroughUnbounded()
    {
        FakePorts ports = new();

        new CharacterIndependentTaskTickCoordinator(ports).Process(0.5d);

        Assert.Equal(TimeSpan.FromSeconds(0.5), Assert.Single(ports.Advances).Elapsed);
    }

    [Fact]
    public void ReturnStepTargetsTheNextReturnWaypointWithoutAnOccurrence()
    {
        FakePorts ports = new();
        ports.ReturnPlan = ReturnPlan();
        ports.ReturnProgress = new(ports.ReturnPlan);

        new CharacterIndependentTaskTickCoordinator(ports).Process(1.0d);

        Advance advance = Assert.Single(ports.Advances);
        Assert.Equal(ports.ReturnPlan.Waypoints[0], advance.ReachedWaypoint);
        Assert.Null(advance.Candidate);
        Assert.Same(ports.ReturnProgress, advance.ReturnProgress);
        Assert.Empty(ports.OccurrenceRequests);
    }

    [Fact]
    public void CompletedReturnDoesNothingEvenForInvalidFrameTime()
    {
        FakePorts ports = new();
        ports.ReturnPlan = ReturnPlan();
        CharacterIndependentTaskReturnTravelProgress progress = new(ports.ReturnPlan);
        foreach (Vector2 waypoint in ports.ReturnPlan.Waypoints)
        {
            progress = CharacterIndependentTaskReturnTravelProgressTransition.Advance(
                progress, ports.ReturnPlan, waypoint);
        }

        ports.ReturnProgress = progress;
        ports.ActiveActivity = null;

        new CharacterIndependentTaskTickCoordinator(ports).Process(double.NaN);

        Assert.Empty(ports.Advances);
        Assert.Empty(ports.OccurrenceRequests);
    }

    [Fact]
    public void ChangedPartyIsPresentedOnceAndAPresentationFailureThrows()
    {
        FakePorts ports = new();
        CharacterPartySnapshot previous = ports.Party!;
        CharacterPartySnapshot separated = new([]);
        ports.PartyAfterAdvance = separated;

        new CharacterIndependentTaskTickCoordinator(ports).Process(1.0d);

        Assert.Equal([(previous, separated)], ports.Presentations);

        ports.Party = previous;
        ports.PresentResult = false;
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => new CharacterIndependentTaskTickCoordinator(ports).Process(1.0d));
        Assert.Equal(
            "The exact independent-task party change could not update body presentation.",
            failure.Message);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(-0.1d)]
    public void InvalidFrameTimeThrowsBeforeAdvancing(double delta)
    {
        FakePorts ports = new();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new CharacterIndependentTaskTickCoordinator(ports).Process(delta));
        Assert.Empty(ports.Advances);
    }

    [Fact]
    public void InvariantViolationsThrowWithTheirExactMessages()
    {
        AssertThrows(
            ports => ports.ReturnPlan = ReturnPlan(),
            "The independent-task return plan and progress must be jointly present.");
        AssertThrows(
            ports => ports.ActiveActivity = null,
            "Outbound independent-task travel requires its exact active activity.");
        AssertThrows(
            ports => ports.OutboundProgress = Complete(ports.OutboundPlan!),
            "Completed outbound travel did not initialize exact return travel.");
        AssertThrows(
            ports => ports.Cadence = null,
            "The independent-task step cadence was not initialized.");
        AssertThrows(
            ports => ports.Transcript = null,
            "Character-turn transcript state was not initialized.");
        AssertThrows(
            ports => ports.AdvanceResult = false,
            "The exact live independent-task elapsed step could not advance.");
    }

    private static void AssertThrows(Action<FakePorts> arrange, string message)
    {
        FakePorts ports = new();
        arrange(ports);
        InvalidOperationException failure = Assert.Throws<InvalidOperationException>(
            () => new CharacterIndependentTaskTickCoordinator(ports).Process(1.0d));
        Assert.Equal(message, failure.Message);
    }

    private static CharacterIndependentTaskActivity Activity() =>
        new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId("inspect-herb-patch"),
                new CharacterId("travelling-healer"),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch"))),
            CharacterIndependentTaskActivityState.Active);

    private static CharacterIndependentTaskTravelPlan OutboundPlan(
        CharacterIndependentTaskActivity activity) =>
        new(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(400.0f, 225.0f), new Vector2(800.0f, 450.0f)]);

    private static CharacterIndependentTaskWorldEventOccurrence Occurrence(
        CharacterIndependentTaskActivity activity) =>
        new(
            activity,
            new CharacterTurnWorldEventOccurrence(
                new WorldEvent(
                    new WorldEventId("healer-examined-herb-patch"),
                    WorldEventTimeEffect.Advance),
                new CharacterTurnTranscriptEntry(
                    activity.Assignment.CharacterId,
                    activity.Assignment.Intention)));

    private static CharacterIndependentTaskTravelProgress Complete(
        CharacterIndependentTaskTravelPlan plan)
    {
        CharacterIndependentTaskTravelProgress progress = new(plan);
        foreach (Vector2 waypoint in plan.Waypoints)
        {
            progress = CharacterIndependentTaskTravelProgressTransition.Advance(
                progress, plan, waypoint);
        }

        return progress;
    }

    private static CharacterIndependentTaskReturnTravelPlan ReturnPlan()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan outboundPlan = OutboundPlan(activity);
        CharacterIndependentTaskTravelProgress progress =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                new CharacterIndependentTaskTravelProgress(outboundPlan),
                outboundPlan,
                outboundPlan.Waypoints[0]);
        CharacterIndependentTaskWorldEventOccurrence candidate = Occurrence(activity);
        CharacterIndependentTaskArrivalOccurrenceResult arrival =
            CharacterIndependentTaskArrivalOccurrenceTransition.ArriveAndOccur(
                progress,
                outboundPlan,
                outboundPlan.Waypoints[^1],
                candidate,
                new CharacterIndependentTaskActivitySnapshot([activity]),
                new WorldEventSnapshot([candidate.Occurrence.WorldEvent]),
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null),
                new CharacterTurnTranscriptSnapshot([]));
        return new(
            arrival,
            Assert.Single(arrival.TaskOccurrenceResult.Activities.Activities),
            TestWorldBounds,
            outboundPlan.Waypoints[^1],
            [new Vector2(400.0f, 225.0f), outboundPlan.Origin]);
    }

    private sealed record Advance(
        TimeSpan Elapsed,
        CharacterIndependentTaskReturnTravelPlan? ReturnPlan,
        CharacterIndependentTaskReturnTravelProgress? ReturnProgress,
        Vector2 ReachedWaypoint,
        CharacterIndependentTaskWorldEventOccurrence? Candidate,
        CharacterPartySnapshot Party);

    private sealed class FakePorts : ICharacterIndependentTaskTickPorts
    {
        public FakePorts()
        {
            ActiveActivity = Activity();
            OutboundPlan = OutboundPlan(ActiveActivity);
            OutboundProgress = new(OutboundPlan);
            Activities = new([ActiveActivity]);
            Occurrence = Occurrence(ActiveActivity);
        }

        public CharacterIndependentTaskStepCadence? Cadence { get; set; } =
            new(TimeSpan.FromSeconds(3));

        public CharacterIndependentTaskStepClock? Clock { get; set; } =
            new(new CharacterIndependentTaskStepCadence(TimeSpan.FromSeconds(3)));

        public CharacterIndependentTaskTravelPlan? OutboundPlan { get; set; }

        public CharacterIndependentTaskTravelProgress? OutboundProgress { get; set; }

        public CharacterIndependentTaskReturnTravelPlan? ReturnPlan { get; set; }

        public CharacterIndependentTaskReturnTravelProgress? ReturnProgress { get; set; }

        public CharacterIndependentTaskActivity? ActiveActivity { get; set; }

        public CharacterIndependentTaskActivitySnapshot? Activities { get; set; }

        public CharacterPartySnapshot? Party { get; set; } =
            new([new CharacterId("travelling-healer")]);

        public CharacterPartySnapshot? PartyAfterAdvance { get; set; }

        public WorldEventLogSnapshot? EventLog { get; set; } = new([]);

        public WorldTimeSnapshot? WorldTime { get; set; } = new(null);

        public CharacterTurnTranscriptSnapshot? Transcript { get; set; } = new([]);

        public CharacterIndependentTaskWorldEventOccurrence Occurrence { get; }

        public bool AdvanceResult { get; set; } = true;

        public bool PresentResult { get; set; } = true;

        public List<Advance> Advances { get; } = [];

        public List<CharacterIndependentTaskActivity> OccurrenceRequests { get; } = [];

        public List<(CharacterPartySnapshot, CharacterPartySnapshot)> Presentations { get; } = [];

        public CharacterIndependentTaskWorldEventOccurrence CreateOccurrence(
            CharacterIndependentTaskActivity activity)
        {
            OccurrenceRequests.Add(activity);
            return Occurrence;
        }

        public bool TryAdvanceElapsed(
            CharacterIndependentTaskStepCadence cadence,
            CharacterIndependentTaskStepClock clock,
            TimeSpan elapsed,
            CharacterIndependentTaskTravelPlan outboundPlan,
            CharacterIndependentTaskTravelProgress outboundProgress,
            CharacterIndependentTaskReturnTravelPlan? returnPlan,
            CharacterIndependentTaskReturnTravelProgress? returnProgress,
            Vector2 reachedWaypoint,
            CharacterIndependentTaskWorldEventOccurrence? candidate,
            CharacterIndependentTaskActivitySnapshot activities,
            CharacterPartySnapshot party,
            WorldEventLogSnapshot eventLog,
            WorldTimeSnapshot worldTime,
            CharacterTurnTranscriptSnapshot transcript)
        {
            Advances.Add(new(elapsed, returnPlan, returnProgress, reachedWaypoint, candidate, party));
            if (PartyAfterAdvance is not null)
            {
                Party = PartyAfterAdvance;
            }

            return AdvanceResult;
        }

        public bool TryPresentPartyChange(
            CharacterPartySnapshot previous,
            CharacterPartySnapshot current)
        {
            Presentations.Add((previous, current));
            return PresentResult;
        }
    }
}
