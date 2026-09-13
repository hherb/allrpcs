using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationTurnStartPolicyTests
{
    [Theory]
    [InlineData("respond")]
    [InlineData("interject")]
    public void AllowsAnExactSpeakingDecisionToStartWhileTheBudgetCanContinue(string kindValue)
    {
        CharacterId responder = new("village-sage");
        CharacterTurnParticipationDecision decision = Decision(responder, kindValue);
        CharacterTurnRequest request = Request(responder);
        AutonomousConversationBudget budget = new(0, 2, false);

        bool canStart = AutonomousConversationTurnStartPolicy.CanStart(
            budget,
            decision,
            request,
            null);

        Assert.True(canStart);
        Assert.Same(responder, decision.ScheduledCharacterId);
        Assert.Same(responder, request.RespondingCharacterId);
        Assert.Equal(0, budget.CompletedTurnCount);
        Assert.Equal(2, budget.MaximumTurnCount);
        Assert.False(budget.HasPlayerInterrupted);
    }

    [Fact]
    public void RejectsMeaningfulSilence()
    {
        CharacterId responder = new("woodland-hermit");

        Assert.False(AutonomousConversationTurnStartPolicy.CanStart(
            new AutonomousConversationBudget(0, 2, false),
            Decision(responder, "remain-silent"),
            Request(responder),
            null));
    }

    [Theory]
    [InlineData(2, 2, false)]
    [InlineData(0, 2, true)]
    public void RejectsAClosedConversationBudget(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        CharacterId responder = new("village-sage");
        AutonomousConversationBudget budget = new(
            completedTurnCount,
            maximumTurnCount,
            hasPlayerInterrupted);

        Assert.False(AutonomousConversationTurnStartPolicy.CanStart(
            budget,
            Decision(responder, "respond"),
            Request(responder),
            null));
        Assert.Equal(completedTurnCount, budget.CompletedTurnCount);
        Assert.Equal(maximumTurnCount, budget.MaximumTurnCount);
        Assert.Equal(hasPlayerInterrupted, budget.HasPlayerInterrupted);
    }

    [Fact]
    public void RequiresTheExactResponderIdentifierInstance()
    {
        CharacterId exactResponder = new("village-sage");
        CharacterTurnParticipationDecision decision = Decision(exactResponder, "respond");
        CharacterTurnRequest exactRequest = Request(exactResponder);
        CharacterTurnRequest valueEqualRequest = Request(new CharacterId(exactResponder.Value));
        AutonomousConversationBudget budget = new(0, 2, false);

        Assert.True(AutonomousConversationTurnStartPolicy.CanStart(
            budget,
            decision,
            exactRequest,
            null));
        Assert.False(AutonomousConversationTurnStartPolicy.CanStart(
            budget,
            decision,
            valueEqualRequest,
            null));
        Assert.Equal(decision.ScheduledCharacterId, valueEqualRequest.RespondingCharacterId);
        Assert.NotSame(decision.ScheduledCharacterId, valueEqualRequest.RespondingCharacterId);
    }

    [Theory]
    [InlineData("awaiting-response", false)]
    [InlineData("speaking", false)]
    [InlineData("completed", true)]
    [InlineData("cancelled", true)]
    public void DelegatesCurrentActivityEligibilityToTheExistingStartPolicy(
        string stateValue,
        bool expected)
    {
        CharacterId responder = new("village-sage");
        CharacterTurnActivity current = CharacterTurnStartPolicyTests.CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));

        bool canStart = AutonomousConversationTurnStartPolicy.CanStart(
            new AutonomousConversationBudget(0, 2, false),
            Decision(responder, "respond"),
            Request(responder),
            current);

        Assert.Equal(expected, canStart);
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), current.State);
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterId responder = new("village-sage");
        AutonomousConversationBudget budget = new(0, 2, false);
        CharacterTurnParticipationDecision decision = Decision(responder, "respond");
        CharacterTurnRequest request = Request(responder);

        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartPolicy.CanStart(null!, decision, request, null));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartPolicy.CanStart(budget, null!, request, null));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartPolicy.CanStart(budget, decision, null!, null));
    }

    [Fact]
    public void ExposesOnlyThePureEligibilityOperation()
    {
        Type policyType = typeof(AutonomousConversationTurnStartPolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanStart", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [
                typeof(AutonomousConversationBudget),
                typeof(CharacterTurnParticipationDecision),
                typeof(CharacterTurnRequest),
                typeof(CharacterTurnActivity),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static CharacterTurnParticipationDecision Decision(
        CharacterId responder,
        string kindValue)
    {
        CharacterTurnParticipationKind kind = CharacterTurnParticipationKind.Parse(kindValue);
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : new CharacterGoal("compare-evidence", "Compare the available evidence.");
        return new CharacterTurnParticipationDecision(responder, kind, motive);
    }

    private static CharacterTurnRequest Request(CharacterId responder)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "What do you think?");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [responder]));
        return new CharacterTurnRequest(
            responder,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Question));
    }
}
