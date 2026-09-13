using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskLifecycleTransitionTests
{
    [Theory]
    [InlineData("assigned", "active")]
    [InlineData("assigned", "cancelled")]
    [InlineData("active", "completed")]
    [InlineData("active", "cancelled")]
    public void ProducesFreshActivityRetainingExactAssignment(
        string currentValue,
        string nextValue)
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskLifecyclePolicyTests.Activity(currentValue);
        CharacterIndependentTaskActivityState next =
            CharacterIndependentTaskActivityState.Parse(nextValue);

        CharacterIndependentTaskActivity result =
            CharacterIndependentTaskLifecycleTransition.Transition(activity, next);

        Assert.NotSame(activity, result);
        Assert.Same(activity.Assignment, result.Assignment);
        Assert.Same(next, result.State);
        Assert.Same(CharacterIndependentTaskActivityState.Parse(currentValue), activity.State);
    }

    [Fact]
    public void RejectsIllegalTransitionsWithoutChangingInput()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskLifecyclePolicyTests.Activity("completed");

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskLifecycleTransition.Transition(
                activity,
                CharacterIndependentTaskActivityState.Active));
        Assert.Same(CharacterIndependentTaskActivityState.Completed, activity.State);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureTransition()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskLifecyclePolicyTests.Activity("assigned");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskLifecycleTransition.Transition(
                null!,
                CharacterIndependentTaskActivityState.Active));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskLifecycleTransition.Transition(activity, null!));

        Type transitionType = typeof(CharacterIndependentTaskLifecycleTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));
        Assert.Equal("Transition", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskActivity), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
