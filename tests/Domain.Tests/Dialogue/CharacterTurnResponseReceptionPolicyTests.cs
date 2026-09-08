using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnResponseReceptionPolicyTests
{
    [Fact]
    public void AllowsOnlyAnExactAwaitedRequestResponse()
    {
        CharacterTurnActivity activity = CreateAwaitingActivity("Please examine the flowers.");
        CharacterTurnResponse response = CreateResponse(activity.Request);

        Assert.True(CharacterTurnResponseReceptionPolicy.CanReceive(activity, response));
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, activity.State);
        Assert.Null(activity.Response);
    }

    [Theory]
    [InlineData("speaking")]
    [InlineData("completed")]
    [InlineData("cancelled")]
    public void RejectsNonAwaitingActivity(string stateValue)
    {
        CharacterTurnActivity awaiting = CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response = CreateResponse(awaiting.Request);
        CharacterTurnActivity activity = new(
            awaiting.Request,
            response,
            CharacterTurnActivityState.Parse(stateValue));

        Assert.False(CharacterTurnResponseReceptionPolicy.CanReceive(activity, response));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
    }

    [Fact]
    public void RejectsADistinctRequestWithEquivalentValues()
    {
        CharacterTurnActivity activity = CreateAwaitingActivity("Please wait.");
        CharacterTurnActivity other = CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response = CreateResponse(other.Request);

        Assert.NotSame(activity.Request, other.Request);
        Assert.Equal(
            activity.Request.RespondingCharacterId,
            other.Request.RespondingCharacterId);
        Assert.Equal(
            activity.Request.DialogueEvent.Utterance,
            other.Request.DialogueEvent.Utterance);
        Assert.Same(activity.Request.DialogueAct.Type, other.Request.DialogueAct.Type);
        Assert.False(CharacterTurnResponseReceptionPolicy.CanReceive(activity, response));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnActivity activity = CreateAwaitingActivity("Please wait.");
        CharacterTurnResponse response = CreateResponse(activity.Request);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseReceptionPolicy.CanReceive(null!, response));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnResponseReceptionPolicy.CanReceive(activity, null!));
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityQuery()
    {
        Type policyType = typeof(CharacterTurnResponseReceptionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanReceive", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            [typeof(CharacterTurnActivity), typeof(CharacterTurnResponse)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterTurnActivity CreateAwaitingActivity(string text)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, text);
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));

        return new CharacterTurnActivity(
            request,
            null,
            CharacterTurnActivityState.AwaitingResponse);
    }

    internal static CharacterTurnResponse CreateResponse(CharacterTurnRequest request) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(request.RespondingCharacterId, "I will do that."),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);
}
