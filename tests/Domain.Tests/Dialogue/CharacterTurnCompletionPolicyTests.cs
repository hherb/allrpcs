using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnCompletionPolicyTests
{
    [Theory]
    [InlineData("awaiting-response", false)]
    [InlineData("speaking", true)]
    [InlineData("completed", false)]
    [InlineData("resolved", false)]
    [InlineData("cancelled", false)]
    public void AllowsOnlySpeakingActivity(string stateValue, bool expected)
    {
        CharacterTurnActivity activity = CreateActivity(CharacterTurnActivityState.Parse(stateValue));

        Assert.Equal(expected, CharacterTurnCompletionPolicy.CanComplete(activity));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
    }

    [Fact]
    public void RejectsANullActivity()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnCompletionPolicy.CanComplete(null!));
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityQuery()
    {
        Type policyType = typeof(CharacterTurnCompletionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanComplete", method.Name);
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
        CharacterTurnResponse? response = state == CharacterTurnActivityState.AwaitingResponse
            || state == CharacterTurnActivityState.Resolved
            ? null
            : CharacterTurnResponseReceptionPolicyTests.CreateResponse(awaiting.Request);
        return new CharacterTurnActivity(awaiting.Request, response, state);
    }
}
