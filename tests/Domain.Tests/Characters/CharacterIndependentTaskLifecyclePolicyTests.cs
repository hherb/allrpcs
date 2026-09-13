using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskLifecyclePolicyTests
{
    [Theory]
    [InlineData("assigned", "active")]
    [InlineData("assigned", "cancelled")]
    [InlineData("active", "completed")]
    [InlineData("active", "cancelled")]
    public void AcceptsOnlyDefinedForwardTransitions(string currentValue, string nextValue)
    {
        CharacterIndependentTaskActivity activity = Activity(currentValue);

        Assert.True(CharacterIndependentTaskLifecyclePolicy.CanTransition(
            activity,
            CharacterIndependentTaskActivityState.Parse(nextValue)));
    }

    [Theory]
    [InlineData("assigned", "assigned")]
    [InlineData("assigned", "completed")]
    [InlineData("active", "assigned")]
    [InlineData("active", "active")]
    [InlineData("completed", "assigned")]
    [InlineData("completed", "active")]
    [InlineData("completed", "completed")]
    [InlineData("completed", "cancelled")]
    [InlineData("cancelled", "assigned")]
    [InlineData("cancelled", "active")]
    [InlineData("cancelled", "completed")]
    [InlineData("cancelled", "cancelled")]
    public void RejectsSkippingReversingOrLeavingTerminalStates(
        string currentValue,
        string nextValue)
    {
        Assert.False(CharacterIndependentTaskLifecyclePolicy.CanTransition(
            Activity(currentValue),
            CharacterIndependentTaskActivityState.Parse(nextValue)));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyThePureDecision()
    {
        CharacterIndependentTaskActivity activity = Activity("assigned");

        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskLifecyclePolicy.CanTransition(
                null!,
                CharacterIndependentTaskActivityState.Active));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterIndependentTaskLifecyclePolicy.CanTransition(activity, null!));

        Type policyType = typeof(CharacterIndependentTaskLifecyclePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanTransition", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    internal static CharacterIndependentTaskActivity Activity(string stateValue) =>
        new(
            CharacterIndependentTaskAssignmentTests.Assignment("task", "healer", "moonwort"),
            CharacterIndependentTaskActivityState.Parse(stateValue));
}
