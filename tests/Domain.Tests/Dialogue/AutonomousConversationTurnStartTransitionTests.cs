using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationTurnStartTransitionTests
{
    [Theory]
    [InlineData("respond")]
    [InlineData("interject")]
    public void StartsOneExactApprovedFollowUpWithoutChangingInputs(string kindValue)
    {
        CharacterId responder = new("village-sage");
        AutonomousConversationBudget budget = new(1, 3, false);
        CharacterTurnParticipationDecision decision = Decision(responder, kindValue);
        CharacterTurnRequest request = Request(responder);

        CharacterTurnActivity result = AutonomousConversationTurnStartTransition.Start(
            budget,
            decision,
            request,
            null);

        Assert.Same(request, result.Request);
        Assert.Null(result.Response);
        Assert.Same(CharacterTurnActivityState.AwaitingResponse, result.State);
        Assert.Equal(1, budget.CompletedTurnCount);
        Assert.False(budget.HasPlayerInterrupted);
        Assert.Same(responder, decision.ScheduledCharacterId);
        Assert.Same(responder, request.RespondingCharacterId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("completed")]
    [InlineData("resolved")]
    [InlineData("cancelled")]
    public void StartsAfterEveryEligibleCurrentActivityState(string? stateValue)
    {
        CharacterId responder = new("village-sage");
        CharacterTurnActivity? current = stateValue is null
            ? null
            : CharacterTurnStartPolicyTests.CreateActivity(
                CharacterTurnActivityState.Parse(stateValue));

        CharacterTurnActivity result = AutonomousConversationTurnStartTransition.Start(
            new AutonomousConversationBudget(0, 2, false),
            Decision(responder, "respond"),
            Request(responder),
            current);

        Assert.NotSame(current, result);
        if (current is not null)
        {
            Assert.Same(CharacterTurnActivityState.Parse(stateValue!), current.State);
        }
    }

    [Theory]
    [InlineData("awaiting-response")]
    [InlineData("speaking")]
    public void RejectsReplacingActiveWork(string stateValue)
    {
        CharacterId responder = new("village-sage");
        CharacterTurnActivity current = CharacterTurnStartPolicyTests.CreateActivity(
            CharacterTurnActivityState.Parse(stateValue));

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnStartTransition.Start(
                new AutonomousConversationBudget(0, 2, false),
                Decision(responder, "respond"),
                Request(responder),
                current));
        Assert.Same(CharacterTurnActivityState.Parse(stateValue), current.State);
    }

    [Fact]
    public void RejectsEveryIneligibleConversationDecisionPair()
    {
        CharacterId responder = new("village-sage");
        CharacterId replacement = new(responder.Value);
        CharacterTurnRequest exactRequest = Request(responder);

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnStartTransition.Start(
                new AutonomousConversationBudget(2, 2, false),
                Decision(responder, "respond"),
                exactRequest,
                null));
        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnStartTransition.Start(
                new AutonomousConversationBudget(0, 2, true),
                Decision(responder, "respond"),
                exactRequest,
                null));
        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnStartTransition.Start(
                new AutonomousConversationBudget(0, 2, false),
                Decision(responder, "remain-silent"),
                exactRequest,
                null));
        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationTurnStartTransition.Start(
                new AutonomousConversationBudget(0, 2, false),
                Decision(replacement, "respond"),
                exactRequest,
                null));
        Assert.Equal(responder, replacement);
        Assert.NotSame(responder, replacement);
    }

    [Fact]
    public void RejectsNullRequiredInputs()
    {
        CharacterId responder = new("village-sage");
        AutonomousConversationBudget budget = new(0, 2, false);
        CharacterTurnParticipationDecision decision = Decision(responder, "respond");
        CharacterTurnRequest request = Request(responder);

        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartTransition.Start(null!, decision, request, null));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartTransition.Start(budget, null!, request, null));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationTurnStartTransition.Start(budget, decision, null!, null));
    }

    [Fact]
    public void ExposesOnlyThePureStartTransition()
    {
        Type transitionType = typeof(AutonomousConversationTurnStartTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Start", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnActivity), method.ReturnType);
        Assert.Equal(
            [
                typeof(AutonomousConversationBudget),
                typeof(CharacterTurnParticipationDecision),
                typeof(CharacterTurnRequest),
                typeof(CharacterTurnActivity),
            ],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterTurnParticipationDecision Decision(
        CharacterId responder,
        string kindValue)
    {
        CharacterTurnParticipationKind kind = CharacterTurnParticipationKind.Parse(kindValue);
        Allrpcs.Domain.Characters.CharacterGoal? motive =
            kind == CharacterTurnParticipationKind.Respond
                ? null
                : new("compare-evidence", "Compare the available evidence.");
        return new CharacterTurnParticipationDecision(responder, kind, motive);
    }

    private static CharacterTurnRequest Request(CharacterId responder)
    {
        DialogueUtterance utterance = new(new CharacterId("seeker"), "Please continue.");
        DialogueEvent dialogueEvent = new(
            new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [responder]));
        return new CharacterTurnRequest(
            responder,
            dialogueEvent,
            new DialogueAct(utterance, DialogueActType.Request));
    }
}
