using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnCancellationPolicyTests
{
    [Theory]
    [InlineData("awaiting-response", true)]
    [InlineData("speaking", true)]
    [InlineData("completed", false)]
    [InlineData("resolved", false)]
    [InlineData("cancelled", false)]
    public void AllowsOnlyAwaitingOrSpeakingActivity(string stateValue, bool expected)
    {
        CharacterTurnActivity activity = CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));

        Assert.Equal(expected, CharacterTurnCancellationPolicy.CanCancel(activity));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), activity.State);
    }

    [Fact]
    public void RejectsANullActivity()
    {
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnCancellationPolicy.CanCancel(null!));
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityQuery()
    {
        Type policyType = typeof(CharacterTurnCancellationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanCancel", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(typeof(CharacterTurnActivity), parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterTurnActivity CreateActivity(CharacterTurnActivityState state)
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance utterance = new(seekerId, "Please wait.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterTurnResponse? response = state == CharacterTurnActivityState.AwaitingResponse
            || state == CharacterTurnActivityState.Resolved
            ? null
            : new CharacterTurnResponse(
                request,
                CharacterTurnDisposition.Accept,
                new DialogueDelivery(
                    new DialogueUtterance(healerId, "I will wait."),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);

        return new CharacterTurnActivity(request, response, state);
    }
}
