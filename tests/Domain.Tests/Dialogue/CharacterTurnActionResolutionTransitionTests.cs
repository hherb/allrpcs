using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnActionResolutionTransitionTests
{
    [Fact]
    public void ResolvesAnAwaitingActivityWithoutFabricatingAResponse()
    {
        CharacterTurnActivity awaiting = CharacterTurnStartPolicyTests.CreateActivity(
            CharacterTurnActivityState.AwaitingResponse);

        CharacterTurnActivity resolved = CharacterTurnActionResolutionTransition.Resolve(awaiting);

        Assert.NotSame(awaiting, resolved);
        Assert.Same(awaiting.Request, resolved.Request);
        Assert.Null(resolved.Response);
        Assert.Same(CharacterTurnActivityState.Resolved, resolved.State);
        Assert.True(CharacterTurnStartPolicy.CanStart(resolved));
    }

    [Fact]
    public void RejectsNullAndEveryNonAwaitingState()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnActionResolutionTransition.Resolve(null!));

        foreach (CharacterTurnActivityState state in CharacterTurnActivityState.All
                     .Where(state => !ReferenceEquals(
                         state,
                         CharacterTurnActivityState.AwaitingResponse)))
        {
            CharacterTurnActivity activity = CharacterTurnStartPolicyTests.CreateActivity(state);
            Assert.Throws<InvalidOperationException>(() =>
                CharacterTurnActionResolutionTransition.Resolve(activity));
        }
    }

    [Fact]
    public void ExposesOnlyThePureTransition()
    {
        Type transitionType = typeof(CharacterTurnActionResolutionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Resolve", method.Name);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }
}
