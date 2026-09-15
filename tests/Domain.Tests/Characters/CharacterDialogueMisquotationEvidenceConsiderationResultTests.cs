using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceConsiderationResultTests
{
    [Fact]
    public void RetainsExactAcceptedSelectionAndEvidence()
    {
        ResultInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Evidence);

        CharacterDialogueMisquotationEvidenceConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Same(input.Evidence, result.SelectedEvidence);
    }

    [Fact]
    public void RetainsExactAcceptedAbstentionWithoutEvidence()
    {
        ResultInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            null);

        CharacterDialogueMisquotationEvidenceConsiderationResult result = new(
            input.Request,
            decision);

        Assert.Same(input.Request, result.Request);
        Assert.Same(decision, result.Decision);
        Assert.Null(result.SelectedEvidence);
    }

    [Fact]
    public void RejectsWrongActorAndReplacementCandidate()
    {
        ResultInput input = CreateInput();
        CharacterDialogueMisquotationEvidence replacement = new(
            input.Evidence.Occurrence,
            new CharacterId(input.WitnessId.Value));

        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceConsiderationResult(
                input.Request,
                new CharacterDialogueMisquotationEvidenceDecision(
                    new CharacterId(input.Actor.Identity.Id.Value),
                    null)));
        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceConsiderationResult(
                input.Request,
                new CharacterDialogueMisquotationEvidenceDecision(
                    input.Actor.Identity.Id,
                    replacement)));
        Assert.NotSame(input.Evidence, replacement);
        Assert.Same(input.Evidence.Occurrence, replacement.Occurrence);
    }

    [Fact]
    public void RejectsCurrentlyIneligibleSelectionWithoutChangingInputs()
    {
        ResultInput input = CreateInput();
        CharacterDialogueMisquotationEvidence actorEvidence = new(
            input.Evidence.Occurrence,
            input.Actor.Identity.Id);
        CharacterDialogueMisquotationEvidence[] supplied = [actorEvidence];
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            input.Actor,
            supplied);
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            actorEvidence);

        Assert.Throws<ArgumentException>(() =>
            new CharacterDialogueMisquotationEvidenceConsiderationResult(request, decision));
        Assert.Same(actorEvidence, supplied[0]);
        Assert.Same(actorEvidence, Assert.Single(request.Candidates));
        Assert.Same(input.SourceEvent, actorEvidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, actorEvidence.Occurrence.DeliveredEvent);
        Assert.Same(input.Actor.Identity.Id, input.Evidence.Occurrence.Proposal.ActorId);
        Assert.Contains(input.Actor.Identity.Id, input.SourceEvent.Audience.ListenerCharacterIds);
        Assert.Contains(input.WitnessId, input.DeliveredEvent.Audience.ListenerCharacterIds);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableResultSurface()
    {
        ResultInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            null);

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceConsiderationResult(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceConsiderationResult(input.Request, null!));

        Type resultType = typeof(CharacterDialogueMisquotationEvidenceConsiderationResult);
        System.Reflection.ConstructorInfo constructor = Assert.Single(resultType.GetConstructors());
        (string Name, Type Type, bool CanWrite)[] properties = resultType
            .GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .Select(property => (property.Name, property.PropertyType, property.CanWrite))
            .ToArray();

        Assert.Equal(
            [
                typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest),
                typeof(CharacterDialogueMisquotationEvidenceDecision),
            ],
            constructor.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Equal(
            [
                ("Decision", typeof(CharacterDialogueMisquotationEvidenceDecision)),
                ("Request", typeof(CharacterDialogueMisquotationEvidenceConsiderationRequest)),
                ("SelectedEvidence", typeof(CharacterDialogueMisquotationEvidence)),
            ],
            properties.Select(property => (property.Name, property.Type)).ToArray());
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Empty(resultType.GetEvents());
        Assert.True(resultType.IsSealed);
    }

    private static ResultInput CreateInput()
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
        DialogueEvent deliveredEvent = new(
            new DialogueDelivery(
                new DialogueUtterance(proposal.ActorId, proposal.MisquotedText),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(proposal.ActorId, [witnessId]));
        CharacterDialogueMisquotationEvidence evidence = new(
            new CharacterDialogueMisquotationOccurrence(proposal, deliveredEvent),
            witnessId);
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            actor,
            [evidence]);
        return new(actor, witnessId, sourceEvent, deliveredEvent, evidence, request);
    }

    private sealed record ResultInput(
        Character Actor,
        CharacterId WitnessId,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}
