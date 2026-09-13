using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskTravelTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void PlanRetainsExactActiveTaskAndStableCopiedRoute()
    {
        CharacterIndependentTaskActivity activity = Activity(CharacterIndependentTaskActivityState.Active);
        Vector2[] source = [new(800.0f, 450.0f), new(1600.0f, 900.0f)];

        CharacterIndependentTaskTravelPlan plan = new(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            source);

        source[0] = Vector2.One;

        Assert.Same(activity, plan.Activity);
        Assert.Equal(TestWorldBounds, plan.WorldBounds);
        Assert.Equal(Vector2.Zero, plan.Origin);
        Assert.Equal([new Vector2(800.0f, 450.0f), new Vector2(1600.0f, 900.0f)], plan.Waypoints);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<Vector2>)plan.Waypoints).Add(Vector2.One));
        Assert.All(
            typeof(CharacterIndependentTaskTravelPlan).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void PlanAcceptsMaximumRouteAndWorldBoundaryCoordinates()
    {
        Vector2[] waypoints = Enumerable
            .Range(0, CharacterIndependentTaskTravelPlan.MaximumWaypointCount)
            .Select(index => index % 2 == 0
                ? new Vector2(TestWorldBounds.Left, TestWorldBounds.Top)
                : new Vector2(TestWorldBounds.Right, TestWorldBounds.Bottom))
            .ToArray();

        CharacterIndependentTaskTravelPlan plan = new(
            Activity(CharacterIndependentTaskActivityState.Active),
            TestWorldBounds,
            new Vector2(TestWorldBounds.Right, TestWorldBounds.Bottom),
            waypoints);

        Assert.Equal(CharacterIndependentTaskTravelPlan.MaximumWaypointCount, plan.Waypoints.Count);
        Assert.Equal(waypoints, plan.Waypoints);
    }

    [Theory]
    [InlineData("assigned")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void PlanRejectsInactiveTasks(string stateValue)
    {
        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskTravelPlan(
            Activity(CharacterIndependentTaskActivityState.Parse(stateValue)),
            TestWorldBounds,
            Vector2.Zero,
            [Vector2.One]));
    }

    [Fact]
    public void PlanRejectsEmptyAndOversizedRoutes()
    {
        CharacterIndependentTaskActivity activity = Activity(CharacterIndependentTaskActivityState.Active);
        Vector2[] oversized = Enumerable
            .Repeat(Vector2.One, CharacterIndependentTaskTravelPlan.MaximumWaypointCount + 1)
            .ToArray();

        Assert.Throws<ArgumentException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            []));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            oversized));
    }

    [Fact]
    public void PlanRejectsInvalidBoundsOriginAndWaypoints()
    {
        CharacterIndependentTaskActivity activity = Activity(CharacterIndependentTaskActivityState.Active);

        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            new NavigationWorldBounds(0.0f, 0.0f, 0.0f, 900.0f),
            Vector2.Zero,
            [Vector2.One]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            TestWorldBounds,
            new Vector2(float.NaN, 0.0f),
            [Vector2.One]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(1600.01f, 450.0f)]));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterIndependentTaskTravelPlan(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(800.0f, float.PositiveInfinity)]));
    }

    [Fact]
    public void ProgressAdvancesOneExactWaypointAndCompletesOnlyAtTheEnd()
    {
        CharacterIndependentTaskTravelPlan plan = Plan();
        CharacterIndependentTaskTravelProgress initial = new(plan);

        CharacterIndependentTaskTravelProgress first =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[0]);
        CharacterIndependentTaskTravelProgress completed =
            CharacterIndependentTaskTravelProgressTransition.Advance(
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
    public void ProgressRejectsSkipsRepeatsCompletionAndDistinctPlanIdentity()
    {
        CharacterIndependentTaskTravelPlan plan = Plan();
        CharacterIndependentTaskTravelProgress initial = new(plan);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[1]));

        CharacterIndependentTaskTravelProgress first =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                initial,
                plan,
                plan.Waypoints[0]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(
                first,
                plan,
                plan.Waypoints[0]));

        CharacterIndependentTaskTravelProgress completed =
            CharacterIndependentTaskTravelProgressTransition.Advance(
                first,
                plan,
                plan.Waypoints[1]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(
                completed,
                plan,
                plan.Waypoints[1]));

        CharacterIndependentTaskTravelPlan distinctPlan = new(
            plan.Activity,
            plan.WorldBounds,
            plan.Origin,
            plan.Waypoints);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(
                initial,
                distinctPlan,
                distinctPlan.Waypoints[0]));
        Assert.Equal(0, initial.CompletedWaypointCount);
    }

    [Fact]
    public void ProgressContractsRejectNulls()
    {
        CharacterIndependentTaskTravelPlan plan = Plan();
        CharacterIndependentTaskTravelProgress progress = new(plan);

        Assert.Throws<ArgumentNullException>(() => new CharacterIndependentTaskTravelProgress(null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(null!, plan, Vector2.One));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskTravelProgressTransition.Advance(progress, null!, Vector2.One));
    }

    private static CharacterIndependentTaskTravelPlan Plan() => new(
        Activity(CharacterIndependentTaskActivityState.Active),
        TestWorldBounds,
        Vector2.Zero,
        [new Vector2(400.0f, 225.0f), new Vector2(800.0f, 450.0f)]);

    private static CharacterIndependentTaskActivity Activity(
        CharacterIndependentTaskActivityState state) =>
        new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId("inspect-herb-patch"),
                new CharacterId("travelling-healer"),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch"))),
            state);
}
