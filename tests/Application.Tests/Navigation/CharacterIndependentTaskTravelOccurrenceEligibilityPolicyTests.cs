using System.Numerics;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Tests.Navigation;

public sealed class CharacterIndependentTaskTravelOccurrenceEligibilityPolicyTests
{
    private static readonly NavigationWorldBounds TestWorldBounds =
        new(0.0f, 0.0f, 1600.0f, 900.0f);

    [Fact]
    public void CompletedExactPlanAndActiveTaskAreEligible()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan plan = Plan(activity);
        CharacterIndependentTaskTravelProgress progress = Complete(plan);

        Assert.True(CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
            plan,
            progress,
            activity));
    }

    [Fact]
    public void IncompleteOrDistinctTravelStateIsIneligible()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan plan = Plan(activity);
        CharacterIndependentTaskTravelPlan distinctPlan = Plan(activity);

        Assert.False(CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
            plan,
            new CharacterIndependentTaskTravelProgress(plan),
            activity));
        Assert.False(CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
            plan,
            Complete(distinctPlan),
            activity));
    }

    [Fact]
    public void DistinctOrValueEqualActivityIsIneligible()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan plan = Plan(activity);
        CharacterIndependentTaskTravelProgress progress = Complete(plan);
        CharacterIndependentTaskActivity distinctActivity = new(
            activity.Assignment,
            CharacterIndependentTaskActivityState.Active);
        CharacterIndependentTaskActivity valueEqualActivity = Activity();

        Assert.False(CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
            plan,
            progress,
            distinctActivity));
        Assert.False(CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
            plan,
            progress,
            valueEqualActivity));
    }

    [Fact]
    public void NullInputsAreRejected()
    {
        CharacterIndependentTaskActivity activity = Activity();
        CharacterIndependentTaskTravelPlan plan = Plan(activity);
        CharacterIndependentTaskTravelProgress progress = Complete(plan);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
                null!,
                progress,
                activity));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
                plan,
                null!,
                activity));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskTravelOccurrenceEligibilityPolicy.IsEligible(
                plan,
                progress,
                null!));
    }

    private static CharacterIndependentTaskTravelPlan Plan(
        CharacterIndependentTaskActivity activity) =>
        new(
            activity,
            TestWorldBounds,
            Vector2.Zero,
            [new Vector2(800.0f, 450.0f)]);

    private static CharacterIndependentTaskTravelProgress Complete(
        CharacterIndependentTaskTravelPlan plan) =>
        CharacterIndependentTaskTravelProgressTransition.Advance(
            new CharacterIndependentTaskTravelProgress(plan),
            plan,
            plan.Waypoints[0]);

    private static CharacterIndependentTaskActivity Activity() =>
        new(
            new CharacterIndependentTaskAssignment(
                new CharacterIndependentTaskId("inspect-herb-patch"),
                new CharacterId("travelling-healer"),
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    new ObjectId("herb-patch"))),
            CharacterIndependentTaskActivityState.Active);
}
