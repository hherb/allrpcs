using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnCompletionTransitionTests
{
    [Fact]
    public void CompletesSpeakingActivityWithExactPayloads()
    {
        CharacterTurnActivity activity =
            CharacterTurnCompletionPolicyTests.CreateActivity(CharacterTurnActivityState.Speaking);

        CharacterTurnActivity result = CharacterTurnCompletionTransition.Complete(activity);

        Assert.NotSame(activity, result);
        Assert.Same(activity.Request, result.Request);
        Assert.Same(activity.Response, result.Response);
        Assert.Same(CharacterTurnActivityState.Completed, result.State);
        Assert.Same(CharacterTurnActivityState.Speaking, activity.State);
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RejectsIneligibleActivity(string stateValue)
    {
        CharacterTurnActivity activity =
            CharacterTurnCompletionPolicyTests.CreateActivity(CharacterTurnActivityState.Parse(stateValue));

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnCompletionTransition.Complete(activity));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
    }

    [Fact]
    public void RejectsANullActivity()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnCompletionTransition.Complete(null!));
    }

    [Fact]
    public void ExposesOnlyThePureTransition()
    {
        Type transitionType = typeof(CharacterTurnCompletionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Complete", method.Name);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
