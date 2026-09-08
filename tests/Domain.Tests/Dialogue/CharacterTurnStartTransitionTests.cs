using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnStartTransitionTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void StartsFreshAwaitingActivityAfterEligibleState(string? stateValue)
    {
        CharacterTurnActivity? current = stateValue is null
            ? null
            : CharacterTurnStartPolicyTests.CreateActivity(
                CharacterTurnActivityState.Parse(stateValue));
        CharacterTurnRequest request = CharacterTurnResponseReceptionPolicyTests
            .CreateAwaitingActivity("Please examine the flowers.")
            .Request;

        CharacterTurnActivity result = CharacterTurnStartTransition.Start(current, request);

        Assert.NotSame(current, result);
        Assert.Same(request, result.Request);
        Assert.Null(result.Response);
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, result.State);
        if (current is not null)
        {
            Assert.Equal(stateValue, current.State.Value);
        }
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("speaking")]
    public void RejectsReplacingActiveActivity(string stateValue)
    {
        CharacterTurnActivity current = CharacterTurnStartPolicyTests.CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));
        CharacterTurnRequest request = CharacterTurnResponseReceptionPolicyTests
            .CreateAwaitingActivity("Please examine the flowers.")
            .Request;

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnStartTransition.Start(current, request));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), current.State);
    }

    [Fact]
    public void RejectsANullRequest()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnStartTransition.Start(null, null!));
    }

    [Fact]
    public void ExposesOnlyThePureTransition()
    {
        Type transitionType = typeof(CharacterTurnStartTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Start", method.Name);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity), typeof(CharacterTurnRequest)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
