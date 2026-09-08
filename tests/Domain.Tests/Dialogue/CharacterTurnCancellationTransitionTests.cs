using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnCancellationTransitionTests
{
    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("speaking")]
    public void CancelsEligibleActivityAndRetainsExactPayloads(string stateValue)
    {
        CharacterTurnActivity activity = CharacterTurnCancellationPolicyTests.CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));

        CharacterTurnActivity result = CharacterTurnCancellationTransition.Cancel(activity);

        Assert.NotSame(activity, result);
        Assert.Same(activity.Request, result.Request);
        Assert.Same(activity.Response, result.Response);
        Assert.Same(CharacterTurnActivityState.Cancelled, result.State);
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RejectsIneligibleActivityWithoutChangingIt(string stateValue)
    {
        CharacterTurnActivityState state = CharacterTurnActivityState.Parse(stateValue);
        CharacterTurnActivity activity =
            CharacterTurnCancellationPolicyTests.CreateActivity(state);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnCancellationTransition.Cancel(activity));
        Assert.Same(state, activity.State);
    }

    [Fact]
    public void RejectsANullActivityThroughThePolicy()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnCancellationTransition.Cancel(null!));
    }

    [Fact]
    public void ExposesOnlyThePureCancellationTransition()
    {
        Type transitionType = typeof(CharacterTurnCancellationTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Cancel", method.Name);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterTurnActivity), parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
