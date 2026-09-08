using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnStartPolicyTests
{
    [Fact]
    public void AllowsStartingWithoutACurrentActivity()
    {
        Assert.True(CharacterTurnStartPolicy.CanStart(null));
    }

    [Theory]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void AllowsStartingAfterATerminalActivity(string stateValue)
    {
        CharacterTurnActivity current = CreateActivity(CharacterTurnActivityState.Parse(stateValue));

        Assert.True(CharacterTurnStartPolicy.CanStart(current));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), current.State);
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("speaking")]
    public void RejectsReplacingAnActiveActivity(string stateValue)
    {
        CharacterTurnActivity current = CreateActivity(CharacterTurnActivityState.Parse(stateValue));

        Assert.False(CharacterTurnStartPolicy.CanStart(current));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), current.State);
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityQuery()
    {
        Type policyType = typeof(CharacterTurnStartPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanStart", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterTurnActivity CreateActivity(CharacterTurnActivityState state)
    {
        CharacterTurnActivity awaiting =
            CharacterTurnResponseReceptionPolicyTests.CreateAwaitingActivity("Please wait.");
        if (ReferenceEquals(state, CharacterTurnActivityState.AwaitingResponse))
        {
            return awaiting;
        }

        CharacterTurnResponse response =
            CharacterTurnResponseReceptionPolicyTests.CreateResponse(awaiting.Request);
        return new CharacterTurnActivity(
            awaiting.Request,
            ReferenceEquals(state, CharacterTurnActivityState.Cancelled) ? null : response,
            state);
    }
}
