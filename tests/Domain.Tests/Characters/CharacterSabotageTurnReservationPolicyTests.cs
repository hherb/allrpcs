using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterSabotageTurnReservationPolicyTests
{
    [Fact]
    public void AcceptsTheExactCurrentOpportunityWhenTheSharedTurnSlotIsEmptyOrTerminal()
    {
        CharacterSabotageTurnOpportunity opportunity =
            CharacterSabotageTurnReservationTests.CreateOpportunity();

        Assert.True(CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            opportunity,
            null,
            null));
        Assert.True(CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            opportunity,
            CreateTurn(CharacterTurnActivityState.Resolved),
            null));
    }

    [Fact]
    public void RejectsRebuiltStaleActiveAndRepeatedInputs()
    {
        CharacterSabotageTurnOpportunity opportunity =
            CharacterSabotageTurnReservationTests.CreateOpportunity();
        CharacterSabotageTurnOpportunity rebuilt = new(opportunity.Actor, opportunity.Signal);
        CharacterSabotageTurnReservation reservation = new(opportunity);

        Assert.False(CharacterSabotageTurnReservationPolicy.CanReserve(
            rebuilt,
            opportunity,
            null,
            null));
        Assert.False(CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            rebuilt,
            null,
            null));
        Assert.False(CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            opportunity,
            CreateTurn(CharacterTurnActivityState.AwaitingResponse),
            null));
        Assert.False(CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            opportunity,
            null,
            reservation));
    }

    [Fact]
    public void RejectsNullRequiredInputsAndExposesOnlyOnePureOperation()
    {
        CharacterSabotageTurnOpportunity opportunity =
            CharacterSabotageTurnReservationTests.CreateOpportunity();

        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnReservationPolicy.CanReserve(
                null!,
                opportunity,
                null,
                null));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterSabotageTurnReservationPolicy.CanReserve(
                opportunity,
                null!,
                null,
                null));

        Type policyType = typeof(CharacterSabotageTurnReservationPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanReserve", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterTurnActivity CreateTurn(CharacterTurnActivityState state)
    {
        CharacterId speakerId = new("speaker");
        CharacterId listenerId = new("listener");
        DialogueUtterance utterance = new(speakerId, "The path remains open.");
        CharacterTurnRequest request = new(
            listenerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(speakerId, [listenerId])),
            new DialogueAct(utterance, DialogueActType.ReportedClaim));

        return state == CharacterTurnActivityState.Resolved
            ? new CharacterTurnActivity(request, null, state)
            : new CharacterTurnActivity(request, null, CharacterTurnActivityState.AwaitingResponse);
    }
}
