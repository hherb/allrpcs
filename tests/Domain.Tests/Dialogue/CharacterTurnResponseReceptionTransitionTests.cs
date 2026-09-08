using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnResponseReceptionTransitionTests
{
    [Fact]
    public void ReceivesAnExactResponseAsSpeakingActivity()
    {
        CharacterTurnActivity activity =
            CharacterTurnResponseReceptionPolicyTests.CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response =
            CharacterTurnResponseReceptionPolicyTests.CreateResponse(activity.Request);

        CharacterTurnActivity result = CharacterTurnResponseReceptionTransition.Receive(
            activity,
            response);

        Assert.NotSame(activity, result);
        Assert.Same(activity.Request, result.Request);
        Assert.Same(response, result.Response);
        Assert.Same(CharacterTurnActivityState.Speaking, result.State);
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, activity.State);
        Assert.Null(activity.Response);
    }

    [Fact]
    public void RejectsAnIneligibleResponseWithoutChangingInputs()
    {
        CharacterTurnActivity activity =
            CharacterTurnResponseReceptionPolicyTests.CreateAwaitingActivity("Please wait.");
        CharacterTurnActivity other =
            CharacterTurnResponseReceptionPolicyTests.CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response =
            CharacterTurnResponseReceptionPolicyTests.CreateResponse(other.Request);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnResponseReceptionTransition.Receive(activity, response));
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, activity.State);
        Assert.Null(activity.Response);
        Assert.Same(other.Request, response.Request);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnActivity activity =
            CharacterTurnResponseReceptionPolicyTests.CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response =
            CharacterTurnResponseReceptionPolicyTests.CreateResponse(activity.Request);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseReceptionTransition.Receive(null!, response));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseReceptionTransition.Receive(activity, null!));
    }

    [Fact]
    public void ExposesOnlyThePureTransition()
    {
        Type transitionType = typeof(CharacterTurnResponseReceptionTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Receive", method.Name);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity), typeof(CharacterTurnResponse)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
