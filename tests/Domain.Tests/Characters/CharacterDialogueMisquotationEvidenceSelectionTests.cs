using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceSelectionTests
{
    [Fact]
    public void SelectsExactFirstObservableCandidateInStableOrder()
    {
        SelectionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence second = Candidate(
            input.Actor,
            new CharacterId("second-source"),
            new CharacterId("second-witness"));
        CharacterDialogueMisquotationEvidence[] candidates = [input.Evidence, second];

        CharacterDialogueMisquotationEvidence? selected = Select(input.Actor, candidates);

        Assert.Same(input.Evidence, selected);
        Assert.Collection(
            candidates,
            candidate => Assert.Same(input.Evidence, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.Same(input.SourceEvent, input.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Occurrence.DeliveredEvent);
        Assert.Same(input.WitnessId, input.Evidence.WitnessCharacterId);
    }

    [Fact]
    public void SkipsIneligibleCandidatesBeforeLaterObservableCandidate()
    {
        SelectionInput input = CreateInput();
        CharacterId sourceOnlyWitness = new("source-only-witness");
        CharacterId deliveryOnlyWitness = new("delivery-only-witness");
        DialogueEvent source = SourceEvent(
            new CharacterId("sage-two"),
            [input.Actor.Identity.Id, sourceOnlyWitness, input.WitnessId]);
        CharacterDialogueMisquotationProposal proposal = Proposal(input.Actor, source);
        DialogueEvent delivered = DeliveredEvent(
            proposal,
            [deliveryOnlyWitness, input.WitnessId]);
        CharacterDialogueMisquotationOccurrence occurrence = new(proposal, delivered);
        CharacterDialogueMisquotationEvidence actorCandidate = Candidate(
            input.Actor,
            new CharacterId("actor-source"),
            new CharacterId("actor-witness"));
        CharacterDialogueMisquotationEvidence actor = new(
            actorCandidate.Occurrence,
            input.Actor.Identity.Id);
        CharacterDialogueMisquotationEvidence sourceOnly = Evidence(
            input.Actor,
            new CharacterId("source-only-speaker"),
            [input.Actor.Identity.Id, sourceOnlyWitness],
            [new CharacterId("other-delivery-witness")],
            sourceOnlyWitness);
        CharacterDialogueMisquotationEvidence deliveryOnly = Evidence(
            input.Actor,
            new CharacterId("delivery-only-speaker"),
            [input.Actor.Identity.Id, new CharacterId("other-source-witness")],
            [deliveryOnlyWitness],
            deliveryOnlyWitness);
        CharacterDialogueMisquotationEvidence observable = new(
            occurrence,
            input.WitnessId);

        CharacterDialogueMisquotationEvidence? selected = Select(
            input.Actor,
            [actor, sourceOnly, deliveryOnly, observable]);

        Assert.Same(observable, selected);
        Assert.Same(source, proposal.SourceEvent);
        Assert.Same(delivered, occurrence.DeliveredEvent);
        Assert.Equal("Use the clean herb now.", source.Utterance.Text);
        Assert.Equal(proposal.MisquotedText, delivered.Utterance.Text);
    }

    [Fact]
    public void ReturnsNullWithoutChangingCandidatesWhenNoneAreObservable()
    {
        SelectionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence ineligible = new(
            input.Occurrence,
            input.Actor.Identity.Id);
        CharacterDialogueMisquotationEvidence[] candidates = [ineligible];

        CharacterDialogueMisquotationEvidence? selected = Select(input.Actor, candidates);

        Assert.Null(selected);
        Assert.Same(ineligible, Assert.Single(candidates));
        Assert.Same(input.Occurrence, ineligible.Occurrence);
        Assert.Same(input.SourceEvent, input.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Occurrence.DeliveredEvent);
        Assert.Contains(input.Actor.Identity.Id, input.SourceEvent.Audience.ListenerCharacterIds);
        Assert.Contains(input.WitnessId, input.DeliveredEvent.Audience.ListenerCharacterIds);
    }

    [Fact]
    public void ValidatesCompleteCollectionBeforeSelecting()
    {
        SelectionInput input = CreateInput();

        Assert.Throws<ArgumentException>(() => Select(
            input.Actor,
            [input.Evidence, null!]));
        Assert.Same(input.SourceEvent, input.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void RejectsDuplicateExactDeliveredEventsButAcceptsValueEqualDistinctEvents()
    {
        SelectionInput input = CreateInput();
        CharacterDialogueMisquotationOccurrence duplicateOccurrence = new(
            input.Proposal,
            input.DeliveredEvent);
        CharacterDialogueMisquotationEvidence duplicate = new(
            duplicateOccurrence,
            input.WitnessId);
        DialogueEvent valueEqualDelivery = DeliveredEvent(
            input.Proposal,
            [new CharacterId(input.WitnessId.Value)]);
        CharacterDialogueMisquotationOccurrence valueEqualOccurrence = new(
            input.Proposal,
            valueEqualDelivery);
        CharacterDialogueMisquotationEvidence valueEqual = new(
            valueEqualOccurrence,
            input.WitnessId);

        Assert.Throws<ArgumentException>(() => Select(
            input.Actor,
            [input.Evidence, duplicate]));
        Assert.Same(input.Evidence, Select(input.Actor, [input.Evidence, valueEqual]));
        Assert.NotSame(input.DeliveredEvent, valueEqualDelivery);
        Assert.Equal(
            input.DeliveredEvent.Utterance,
            valueEqualDelivery.Utterance);
        Assert.Equal(
            input.DeliveredEvent.Audience.ListenerCharacterIds,
            valueEqualDelivery.Audience.ListenerCharacterIds);
        Assert.Same(input.DeliveredEvent, duplicateOccurrence.DeliveredEvent);
    }

    [Fact]
    public void AcceptsMaximumCountAndRejectsExcessiveCandidates()
    {
        SelectionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence[] maximum = Enumerable
            .Range(0, CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount)
            .Select(index => Candidate(
                input.Actor,
                new CharacterId($"source-{index}"),
                new CharacterId($"witness-{index}")))
            .ToArray();
        CharacterDialogueMisquotationEvidence[] excessive = maximum
            .Append(Candidate(
                input.Actor,
                new CharacterId("excessive-source"),
                new CharacterId("excessive-witness")))
            .ToArray();

        Assert.Same(maximum[0], Select(input.Actor, maximum));
        Assert.Throws<ArgumentOutOfRangeException>(() => Select(input.Actor, excessive));
        Assert.Equal(
            CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount,
            maximum.Length);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnePureSelectionOperation()
    {
        SelectionInput input = CreateInput();

        Assert.Throws<ArgumentNullException>(() => Select(null!, [input.Evidence]));
        Assert.Throws<ArgumentNullException>(() => Select(input.Actor, null!));

        Type selectionType = typeof(CharacterDialogueMisquotationEvidenceSelection);
        System.Reflection.MethodInfo method = Assert.Single(selectionType.GetMethods().Where(
            candidate => candidate.DeclaringType == selectionType));
        Assert.Equal("SelectFirstObservable", method.Name);
        Assert.True(method.IsStatic);
        Assert.Equal(typeof(CharacterDialogueMisquotationEvidence), method.ReturnType);
        Assert.Equal(
            [typeof(Character), typeof(IEnumerable<CharacterDialogueMisquotationEvidence>)],
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(selectionType.GetConstructors());
        Assert.True(selectionType.IsAbstract && selectionType.IsSealed);
    }

    private static CharacterDialogueMisquotationEvidence? Select(
        Character actor,
        IEnumerable<CharacterDialogueMisquotationEvidence> candidates) =>
        CharacterDialogueMisquotationEvidenceSelection.SelectFirstObservable(actor, candidates);

    private static SelectionInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        DialogueEvent sourceEvent = SourceEvent(
            new CharacterId("sage"),
            [actor.Identity.Id, witnessId]);
        CharacterDialogueMisquotationProposal proposal = Proposal(actor, sourceEvent);
        DialogueEvent deliveredEvent = DeliveredEvent(proposal, [witnessId]);
        CharacterDialogueMisquotationOccurrence occurrence = new(proposal, deliveredEvent);
        CharacterDialogueMisquotationEvidence evidence = new(occurrence, witnessId);
        return new(
            actor,
            witnessId,
            sourceEvent,
            proposal,
            deliveredEvent,
            occurrence,
            evidence);
    }

    private static CharacterDialogueMisquotationEvidence Candidate(
        Character actor,
        CharacterId sourceSpeakerId,
        CharacterId witnessId)
    {
        DialogueEvent sourceEvent = SourceEvent(
            sourceSpeakerId,
            [actor.Identity.Id, witnessId]);
        CharacterDialogueMisquotationProposal proposal = Proposal(actor, sourceEvent);
        DialogueEvent deliveredEvent = DeliveredEvent(proposal, [witnessId]);
        return new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
    }

    private static CharacterDialogueMisquotationEvidence Evidence(
        Character actor,
        CharacterId sourceSpeakerId,
        IReadOnlyList<CharacterId> sourceListeners,
        IReadOnlyList<CharacterId> deliveryListeners,
        CharacterId witnessId)
    {
        DialogueEvent sourceEvent = SourceEvent(sourceSpeakerId, sourceListeners);
        CharacterDialogueMisquotationProposal proposal = Proposal(actor, sourceEvent);
        DialogueEvent deliveredEvent = DeliveredEvent(proposal, deliveryListeners);
        return new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
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

    private sealed record SelectionInput(
        Character Actor,
        CharacterId WitnessId,
        DialogueEvent SourceEvent,
        CharacterDialogueMisquotationProposal Proposal,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationOccurrence Occurrence,
        CharacterDialogueMisquotationEvidence Evidence);
}
