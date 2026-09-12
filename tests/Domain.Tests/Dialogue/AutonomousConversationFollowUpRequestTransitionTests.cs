using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class AutonomousConversationFollowUpRequestTransitionTests
{
    [Theory]
    [InlineData("respond")]
    [InlineData("interject")]
    public void CreatesAnExactRequestForASelectedSpeakingListener(string kindValue)
    {
        CharacterId responder = new("village-sage");
        CharacterTurnParticipationDecision decision = Decision(responder, kindValue);
        DialogueEvent dialogueEvent = Event(new CharacterId(responder.Value));

        CharacterTurnRequest request =
            AutonomousConversationFollowUpRequestTransition.Create(
                decision,
                dialogueEvent,
                DialogueActType.Question);

        Assert.Same(responder, request.RespondingCharacterId);
        Assert.Same(dialogueEvent, request.DialogueEvent);
        Assert.Same(dialogueEvent.Utterance, request.DialogueAct.Utterance);
        Assert.Same(DialogueActType.Question, request.DialogueAct.Type);
        Assert.Same(responder, decision.ScheduledCharacterId);
        Assert.NotSame(responder, dialogueEvent.Audience.ListenerCharacterIds[0]);
        Assert.Equal(responder, dialogueEvent.Audience.ListenerCharacterIds[0]);
    }

    [Fact]
    public void RejectsMeaningfulSilenceWithoutChangingItsExactMotive()
    {
        CharacterId responder = new("woodland-hermit");
        CharacterGoal motive = new("protect-refuge", "Keep the refuge concealed.");
        CharacterTurnParticipationDecision silence = new(
            responder,
            CharacterTurnParticipationKind.RemainSilent,
            motive);
        DialogueEvent dialogueEvent = Event(responder);

        Assert.Throws<InvalidOperationException>(() =>
            AutonomousConversationFollowUpRequestTransition.Create(
                silence,
                dialogueEvent,
                DialogueActType.ReportedClaim));
        Assert.Same(motive, silence.Motive);
        Assert.Same(responder, dialogueEvent.Audience.ListenerCharacterIds[0]);
    }

    [Fact]
    public void RejectsASelectedSpeakerWhoDidNotHearTheExactEvent()
    {
        CharacterId responder = new("village-sage");
        CharacterTurnParticipationDecision decision = Decision(responder, "interject");
        DialogueEvent dialogueEvent = Event(new CharacterId("woodland-hermit"));

        Assert.Throws<ArgumentException>(() =>
            AutonomousConversationFollowUpRequestTransition.Create(
                decision,
                dialogueEvent,
                DialogueActType.ReportedClaim));
        Assert.Same(responder, decision.ScheduledCharacterId);
        Assert.Equal("woodland-hermit", dialogueEvent.Audience.ListenerCharacterIds[0].Value);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterId responder = new("village-sage");
        CharacterTurnParticipationDecision decision = Decision(responder, "respond");
        DialogueEvent dialogueEvent = Event(responder);

        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationFollowUpRequestTransition.Create(
                null!,
                dialogueEvent,
                DialogueActType.Question));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationFollowUpRequestTransition.Create(
                decision,
                null!,
                DialogueActType.Question));
        Assert.Throws<ArgumentNullException>(() =>
            AutonomousConversationFollowUpRequestTransition.Create(
                decision,
                dialogueEvent,
                null!));
    }

    [Fact]
    public void ExposesOnlyThePureRequestCreationTransition()
    {
        Type transitionType = typeof(AutonomousConversationFollowUpRequestTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Create", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterTurnRequest), method.ReturnType);
        Assert.Equal(
            [
                typeof(CharacterTurnParticipationDecision),
                typeof(DialogueEvent),
                typeof(DialogueActType),
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
        CharacterGoal? motive = kind == CharacterTurnParticipationKind.Respond
            ? null
            : new CharacterGoal("compare-evidence", "Compare the available evidence.");
        return new CharacterTurnParticipationDecision(responder, kind, motive);
    }

    private static DialogueEvent Event(CharacterId listener)
    {
        DialogueUtterance utterance = new(
            new CharacterId("travelling-healer"),
            "The observed leaves support that conclusion.");
        return new DialogueEvent(
            utterance,
            new DialogueAudienceSnapshot(utterance.SpeakerCharacterId, [listener]));
    }
}
