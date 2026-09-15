using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceConsiderationRequestTests
{
    [Fact]
    public void RetainsExactActorAndStableCopiedCandidates()
    {
        RequestInput input = CreateInput();
        CharacterDialogueMisquotationEvidence second = Candidate(
            input.Actor,
            "second-source",
            "second-witness");
        CharacterDialogueMisquotationEvidence[] supplied = [input.Evidence, second];

        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            input.Actor,
            supplied);
        supplied[0] = second;

        Assert.Same(input.Actor, request.Actor);
        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Evidence, candidate),
            candidate => Assert.Same(second, candidate));
        Assert.IsType<System.Collections.ObjectModel.ReadOnlyCollection<
            CharacterDialogueMisquotationEvidence>>(request.Candidates);
        Assert.Same(input.SourceEvent, input.Evidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Evidence.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void AcceptsEmptyAndMaximumBoundedCandidateCollections()
    {
        RequestInput input = CreateInput();
        CharacterDialogueMisquotationEvidence[] maximum = Enumerable
            .Range(0, CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount)
            .Select(index => Candidate(input.Actor, $"source-{index}", $"witness-{index}"))
            .ToArray();

        CharacterDialogueMisquotationEvidenceConsiderationRequest empty = new(
            input.Actor,
            []);
        CharacterDialogueMisquotationEvidenceConsiderationRequest full = new(
            input.Actor,
            maximum);

        Assert.Empty(empty.Candidates);
        Assert.Equal(
            CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount,
            full.Candidates.Count);
        Assert.Same(maximum[0], full.Candidates[0]);
        Assert.Same(maximum[^1], full.Candidates[^1]);
    }

    [Fact]
    public void RejectsNullDuplicateExactDeliveryAndOverBoundCollections()
    {
        RequestInput input = CreateInput();
        CharacterDialogueMisquotationEvidence duplicate = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                input.DeliveredEvent),
            input.WitnessId);
        CharacterDialogueMisquotationEvidence[] excessive = Enumerable
            .Range(0, CharacterDialogueMisquotationEvidenceSelection.MaximumCandidateCount + 1)
            .Select(index => Candidate(
                input.Actor,
                $"excessive-source-{index}",
                $"excessive-witness-{index}"))
            .ToArray();

        Assert.Throws<ArgumentException>(() => New(
            input.Actor,
            [input.Evidence, null!]));
        Assert.Throws<ArgumentException>(() => New(
            input.Actor,
            [input.Evidence, duplicate]));
        Assert.Throws<ArgumentOutOfRangeException>(() => New(input.Actor, excessive));
        Assert.Same(input.DeliveredEvent, duplicate.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void AcceptsDistinctValueEqualDeliveredEvents()
    {
        RequestInput input = CreateInput();
        DialogueEvent valueEqualDelivery = DeliveredEvent(
            input.Evidence.Occurrence.Proposal,
            new CharacterId(input.WitnessId.Value));
        CharacterDialogueMisquotationEvidence valueEqual = new(
            new CharacterDialogueMisquotationOccurrence(
                input.Evidence.Occurrence.Proposal,
                valueEqualDelivery),
            input.WitnessId);

        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            input.Actor,
            [input.Evidence, valueEqual]);

        Assert.Collection(
            request.Candidates,
            candidate => Assert.Same(input.Evidence, candidate),
            candidate => Assert.Same(valueEqual, candidate));
        Assert.NotSame(input.DeliveredEvent, valueEqualDelivery);
        Assert.Equal(input.DeliveredEvent.Utterance, valueEqualDelivery.Utterance);
        Assert.Equal(
            input.DeliveredEvent.Audience.ListenerCharacterIds,
            valueEqualDelivery.Audience.ListenerCharacterIds);
    }

    [Fact]
    public void RejectsNullInputsAndExposesOnlyImmutableProviderNeutralSurface()
    {
        RequestInput input = CreateInput();

        Assert.Throws<ArgumentNullException>(() => New(null!, [input.Evidence]));
        Assert.Throws<ArgumentNullException>(() => New(input.Actor, null!));

        Type requestType = typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest);
        System.Reflection.PropertyInfo[] properties = requestType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            ["Actor", "Candidates"],
            properties.Select(property => property.Name));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(requestType.GetConstructors());
        Assert.Empty(requestType.GetEvents());
        Assert.True(requestType.IsSealed);
    }

    private static CharacterDialogueMisquotationEvidenceConsiderationRequest New(
        Character actor,
        IEnumerable<CharacterDialogueMisquotationEvidence> candidates) => new(actor, candidates);

    private static RequestInput CreateInput()
    {
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterId witnessId = new("seeker");
        CharacterId sourceSpeakerId = new("sage");
        DialogueEvent sourceEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(sourceSpeakerId, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(
                sourceSpeakerId,
                [actor.Identity.Id, witnessId]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            sourceEvent,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        DialogueEvent deliveredEvent = DeliveredEvent(proposal, witnessId);
        CharacterDialogueMisquotationEvidence evidence = new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
        return new(actor, witnessId, sourceEvent, deliveredEvent, evidence);
    }

    private static CharacterDialogueMisquotationEvidence Candidate(
        Character actor,
        string sourceSpeakerId,
        string witnessId)
    {
        CharacterId speaker = new(sourceSpeakerId);
        CharacterId witness = new(witnessId);
        DialogueEvent source = new(
            new DialogueDelivery(
                new DialogueUtterance(speaker, "Use the clean herb now."),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speaker, [actor.Identity.Id, witness]));
        CharacterDialogueMisquotationProposal proposal = new(
            actor.Identity.Id,
            source,
            "The sage said the remedy should wait for royal approval.",
            "assess-local-influence");
        return new(
            new CharacterDialogueMisquotationOccurrence(
                proposal,
                DeliveredEvent(proposal, witness)),
            witness);
    }

    private static DialogueEvent DeliveredEvent(
        CharacterDialogueMisquotationProposal proposal,
        CharacterId witnessId) => new(
        new DialogueDelivery(
            new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
            DialogueDeliveryMode.OrdinarySpeech),
        new DialogueAudienceSnapshot(proposal.ActorId, [witnessId]));

    private sealed record RequestInput(
        Character Actor,
        CharacterId WitnessId,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence);
}
