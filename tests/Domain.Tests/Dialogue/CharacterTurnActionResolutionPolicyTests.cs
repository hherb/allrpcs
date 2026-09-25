using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnActionResolutionPolicyTests
{
    [Fact]
    public void AllowsOnlyAResponseFreeAwaitingActivity()
    {
        CharacterTurnActivity awaiting = CharacterTurnStartPolicyTests.CreateActivity(
            CharacterTurnActivityState.AwaitingResponse);

        Assert.True(CharacterTurnActionResolutionPolicy.CanResolve(awaiting));

        foreach (CharacterTurnActivityState state in CharacterTurnActivityState.All
                     .Where(state => !ReferenceEquals(
                         state,
                         CharacterTurnActivityState.AwaitingResponse)))
        {
            Assert.False(CharacterTurnActionResolutionPolicy.CanResolve(
                CharacterTurnStartPolicyTests.CreateActivity(state)));
        }
    }

    [Fact]
    public void RejectsNullAndExposesOnlyThePureEligibilityQuery()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnActionResolutionPolicy.CanResolve(null!));

        Type policyType = typeof(CharacterTurnActionResolutionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanResolve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }
}
