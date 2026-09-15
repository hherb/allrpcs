using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceTests
{
    [Fact]
    public void RetainsExactOccurrenceAndWitnessWhoHeardSourceAndMisquotation()
    {
        Input input = CreateInput();
        CharacterDialogueMisquotationEvidence evidence = new(
            input.Occurrence,
            input.WitnessId);

        Assert.Same(input.Occurrence, evidence.Occurrence);
        Assert.Same(input.WitnessId, evidence.WitnessCharacterId);
        Assert.True(CharacterDialogueMisquotationEvidencePolicy.CanObserve(
            input.Actor,
            evidence));
        Assert.Equal("Use the clean herb now.", input.SourceEvent.Utterance.Text);
        Assert.Equal(input.Proposal.MisquotedText, input.DeliveredEvent.Utterance.Text);
        Assert.Same(input.SourceEvent, input.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void RejectsActorAndWitnessWhoDidNotHearBothExactEvents()
    {
        Input input = CreateInput();
        CharacterId sourceOnlyWitness = new("source-only-witness");
        CharacterId deliveryOnlyWitness = new("delivery-only-witness");
        DialogueEvent source = SourceEvent(
            input.SourceSpeakerId,
            [input.Actor.Identity.Id, input.WitnessId, sourceOnlyWitness]);
        CharacterDialogueMisquotationProposal proposal = Proposal(input.Actor, source);
        DialogueEvent delivered = DeliveredEvent(
            proposal,
            [input.WitnessId, deliveryOnlyWitness]);
        CharacterDialogueMisquotationOccurrence occurrence = new(proposal, delivered);

        Assert.False(CanObserve(
            input.Actor,
            new CharacterDialogueMisquotationEvidence(occurrence, input.Actor.Identity.Id)));
        Assert.False(CanObserve(
            input.Actor,
            new CharacterDialogueMisquotationEvidence(occurrence, sourceOnlyWitness)));
        Assert.False(CanObserve(
            input.Actor,
            new CharacterDialogueMisquotationEvidence(occurrence, deliveryOnlyWitness)));
        Assert.False(CanObserve(
            input.Actor,
            new CharacterDialogueMisquotationEvidence(
                occurrence,
                new CharacterId("unknown-witness"))));
        Assert.Same(source, proposal.SourceEvent);
        Assert.Same(delivered, occurrence.DeliveredEvent);
    }

    [Fact]
    public void RejectsEvidenceForAnIneligibleOccurrence()
    {
        Input input = CreateInput();
        CharacterId valueEqualActor = new(input.Actor.Identity.Id.Value);
        DialogueEvent invalidDelivery = new(
            new DialogueDelivery(
                new DialogueUtterance(valueEqualActor, input.Proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(valueEqualActor, [input.WitnessId]));
        CharacterDialogueMisquotationOccurrence invalidOccurrence = new(
            input.Proposal,
            invalidDelivery);

        Assert.False(CanObserve(
            input.Actor,
            new CharacterDialogueMisquotationEvidence(invalidOccurrence, input.WitnessId)));
        Assert.Same(input.SourceEvent, input.Proposal.SourceEvent);
        Assert.Equal("Use the clean herb now.", input.SourceEvent.Utterance.Text);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyOnePurePolicyOperation()
    {
        Input input = CreateInput();
        CharacterDialogueMisquotationEvidence evidence = new(
            input.Occurrence,
            input.WitnessId);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidence(null!, input.WitnessId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidence(input.Occurrence, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePolicy.CanObserve(null!, evidence));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidencePolicy.CanObserve(input.Actor, null!));
        Assert.All(
            typeof(CharacterDialogueMisquotationEvidence).GetProperties(),
            property => Assert.False(property.CanWrite));

        Type policyType = typeof(CharacterDialogueMisquotationEvidencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanObserve", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.Equal(
            [typeof(Character), typeof(CharacterDialogueMisquotationEvidence)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static bool CanObserve(
        Character actor,
        CharacterDialogueMisquotationEvidence evidence) =>
        CharacterDialogueMisquotationEvidencePolicy.CanObserve(actor, evidence);

    private static Input CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId sourceSpeakerId = new("sage");
        CharacterId witnessId = new("seeker");
        DialogueEvent sourceEvent = SourceEvent(
            sourceSpeakerId,
            [actor.Identity.Id, witnessId]);
        CharacterDialogueMisquotationProposal proposal = Proposal(actor, sourceEvent);
        DialogueEvent deliveredEvent = DeliveredEvent(proposal, [witnessId]);
        CharacterDialogueMisquotationOccurrence occurrence = new(proposal, deliveredEvent);
        return new(
            actor,
            sourceSpeakerId,
            witnessId,
            sourceEvent,
            proposal,
            deliveredEvent,
            occurrence);
    }

    private static CharacterDialogueMisquotationProposal Proposal(
        Character actor,
        DialogueEvent sourceEvent) => new(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");

    private static DialogueEvent SourceEvent(
        CharacterId speakerId,
        IReadOnlyList<CharacterId> listeners) => new(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerId, listeners));

    private static DialogueEvent DeliveredEvent(
        CharacterDialogueMisquotationProposal proposal,
        IReadOnlyList<CharacterId> listeners) => new(
            new DialogueDelivery(
                new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(proposal.ActorId, listeners));

    private sealed record Input(
        Character Actor,
        CharacterId SourceSpeakerId,
        CharacterId WitnessId,
        DialogueEvent SourceEvent,
        CharacterDialogueMisquotationProposal Proposal,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationOccurrence Occurrence);
}
