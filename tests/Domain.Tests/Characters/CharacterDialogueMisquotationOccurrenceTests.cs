using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationOccurrenceTests
{
    [Fact]
    public void RetainsExactProposalAndDeliveredEvent()
    {
        Input input = CreateInput();

        CharacterDialogueMisquotationOccurrence occurrence = new(
            input.Proposal,
            input.DeliveredEvent);

        Assert.Same(input.Proposal, occurrence.Proposal);
        Assert.Same(input.DeliveredEvent, occurrence.DeliveredEvent);
        Assert.All(
            typeof(CharacterDialogueMisquotationOccurrence).GetProperties(),
            property => Assert.False(property.CanWrite));
    }

    [Fact]
    public void AcceptsEligibleProposalBoundToExactActorTextAndAudience()
    {
        Input input = CreateInput();
        CharacterDialogueMisquotationOccurrence occurrence = new(
            input.Proposal,
            input.DeliveredEvent);

        Assert.True(CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
            input.Character,
            occurrence));
        Assert.Same(
            input.Proposal.ActorId,
            input.DeliveredEvent.Utterance.SpeakerCharacterId);
        Assert.Equal(input.Proposal.MisquotedText, input.DeliveredEvent.Utterance.Text);
        Assert.Single(input.DeliveredEvent.Audience.ListenerCharacterIds);
        Assert.Equal("Use the clean herb now.", input.Proposal.SourceEvent.Utterance.Text);
    }

    [Fact]
    public void RejectsDistinctActorWrongTextAndEmptyAudience()
    {
        Input input = CreateInput();
        CharacterId valueEqualActor = new(input.Proposal.ActorId.Value);
        DialogueEvent distinctActor = DeliveredEvent(
            valueEqualActor,
            input.Proposal.MisquotedText,
            [input.RecipientId]);
        DialogueEvent wrongText = DeliveredEvent(
            input.Proposal.ActorId,
            "The sage said something else.",
            [input.RecipientId]);
        DialogueEvent emptyAudience = DeliveredEvent(
            input.Proposal.ActorId,
            input.Proposal.MisquotedText,
            []);

        Assert.Equal(input.Proposal.ActorId, valueEqualActor);
        Assert.NotSame(input.Proposal.ActorId, valueEqualActor);
        Assert.False(CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
            input.Character,
            new CharacterDialogueMisquotationOccurrence(input.Proposal, distinctActor)));
        Assert.False(CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
            input.Character,
            new CharacterDialogueMisquotationOccurrence(input.Proposal, wrongText)));
        Assert.False(CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
            input.Character,
            new CharacterDialogueMisquotationOccurrence(input.Proposal, emptyAudience)));
        Assert.Same(input.Proposal.SourceEvent, input.SourceEvent);
    }

    [Fact]
    public void DelegatesProposalEligibilityAndRejectsNulls()
    {
        Input input = CreateInput();
        DialogueEvent unheardSource = SourceEvent(
            input.SourceSpeakerId,
            new CharacterId("other-listener"));
        CharacterDialogueMisquotationProposal unheardProposal = new(
            input.Proposal.ActorId,
            unheardSource,
            input.Proposal.MisquotedText,
            input.Proposal.RequiredCapabilityKey);
        CharacterDialogueMisquotationOccurrence unheardOccurrence = new(
            unheardProposal,
            input.DeliveredEvent);

        Assert.False(CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
            input.Character,
            unheardOccurrence));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationOccurrence(null!, input.DeliveredEvent));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationOccurrence(input.Proposal, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationOccurrencePolicy.CanOccur(
                null!,
                new CharacterDialogueMisquotationOccurrence(
                    input.Proposal,
                    input.DeliveredEvent)));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationOccurrencePolicy.CanOccur(input.Character, null!));

        Type policyType = typeof(CharacterDialogueMisquotationOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanOccur", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(Character), typeof(CharacterDialogueMisquotationOccurrence)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static Input CreateInput()
    {
        Character character = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId sourceSpeakerId = new("sage");
        CharacterId recipientId = new("seeker");
        DialogueEvent sourceEvent = SourceEvent(sourceSpeakerId, character.Identity.Id);
        CharacterDialogueMisquotationProposal proposal = new(
            character.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        DialogueEvent deliveredEvent = DeliveredEvent(
            proposal.ActorId,
            proposal.MisquotedText,
            [recipientId]);
        return new(
            character,
            sourceSpeakerId,
            recipientId,
            sourceEvent,
            proposal,
            deliveredEvent);
    }

    private static DialogueEvent SourceEvent(CharacterId speakerId, CharacterId listenerId) => new(
        new DialogueDelivery(
            new DialogueUtterance(speakerId, "Use the clean herb now."),
            DialogueDeliveryMode.OrdinarySpeech),
        new DialogueAudienceSnapshot(speakerId, [listenerId]));

    private static DialogueEvent DeliveredEvent(
        CharacterId actorId,
        string text,
        IReadOnlyList<CharacterId> listeners) => new(
            new DialogueDelivery(
                new DialogueUtterance(actorId, text),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(actorId, listeners));

    private sealed record Input(
        Character Character,
        CharacterId SourceSpeakerId,
        CharacterId RecipientId,
        DialogueEvent SourceEvent,
        CharacterDialogueMisquotationProposal Proposal,
        DialogueEvent DeliveredEvent);
}
