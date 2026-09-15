using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterDialogueMisquotationEvidenceDecisionTests
{
    [Fact]
    public void RetainsExactSelectionOrExplicitAbstention()
    {
        DecisionInput input = CreateInput();

        CharacterDialogueMisquotationEvidenceDecision selection = new(
            input.Actor.Identity.Id,
            input.Evidence);
        CharacterDialogueMisquotationEvidenceDecision abstention = new(
            input.Actor.Identity.Id,
            null);

        Assert.Same(input.Actor.Identity.Id, selection.ActorId);
        Assert.Same(input.Evidence, selection.SelectedEvidence);
        Assert.Same(input.Actor.Identity.Id, abstention.ActorId);
        Assert.Null(abstention.SelectedEvidence);
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterDialogueMisquotationEvidenceDecision(null!, null));
    }

    [Fact]
    public void AcceptsExactAbstentionAndExactObservableSelection()
    {
        DecisionInput input = CreateInput();

        Assert.True(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterDialogueMisquotationEvidenceDecision(
                input.Actor.Identity.Id,
                null)));
        Assert.True(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterDialogueMisquotationEvidenceDecision(
                input.Actor.Identity.Id,
                input.Evidence)));
        Assert.Same(input.SourceEvent, input.Evidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, input.Evidence.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void RejectsValueEqualActorReplacementAndNonRequestCandidate()
    {
        DecisionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence replacement = new(
            input.Evidence.Occurrence,
            new CharacterId(input.WitnessId.Value));

        Assert.False(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterDialogueMisquotationEvidenceDecision(
                new CharacterId(input.Actor.Identity.Id.Value),
                null)));
        Assert.False(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            input.Request,
            new CharacterDialogueMisquotationEvidenceDecision(
                input.Actor.Identity.Id,
                replacement)));
        Assert.NotSame(input.Evidence, replacement);
        Assert.Same(input.Evidence.Occurrence, replacement.Occurrence);
    }

    [Fact]
    public void RejectsRequestCandidateThatIsNoLongerObservableForTheExactActor()
    {
        DecisionInput input = CreateInput();
        CharacterDialogueMisquotationEvidence actorEvidence = new(
            input.Evidence.Occurrence,
            input.Actor.Identity.Id);
        CharacterDialogueMisquotationEvidenceConsiderationRequest request = new(
            input.Actor,
            [actorEvidence]);
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            actorEvidence);

        Assert.False(CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(
            request,
            decision));
        Assert.Same(input.SourceEvent, actorEvidence.Occurrence.Proposal.SourceEvent);
        Assert.Same(input.DeliveredEvent, actorEvidence.Occurrence.DeliveredEvent);
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyBoundedProviderNeutralSurfaces()
    {
        DecisionInput input = CreateInput();
        CharacterDialogueMisquotationEvidenceDecision decision = new(
            input.Actor.Identity.Id,
            input.Evidence);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(null!, decision));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterDialogueMisquotationEvidenceDecisionPolicy.CanAccept(input.Request, null!));

        Type decisionType = typeof(CharacterDialogueMisquotationEvidenceDecision);
        System.Reflection.PropertyInfo[] properties = decisionType.GetProperties()
            .OrderBy(property => property.Name, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(["ActorId", "SelectedEvidence"], properties.Select(property => property.Name));
        Assert.All(properties, property => Assert.False(property.CanWrite));
        Assert.Single(decisionType.GetConstructors());
        Assert.Empty(decisionType.GetEvents());
        Assert.True(decisionType.IsSealed);

        Type policyType = typeof(CharacterDialogueMisquotationEvidenceDecisionPolicy);
        System.Reflection.MethodInfo method = Assert.Single(policyType.GetMethods().Where(
            candidate => candidate.DeclaringType == policyType));
        Assert.Equal("CanAccept", method.Name);
        Assert.True(method.IsStatic);
        Assert.Empty(policyType.GetConstructors());
        Assert.True(policyType.IsAbstract && policyType.IsSealed);
    }

    private static DecisionInput CreateInput()
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

    private sealed record DecisionInput(
        Character Actor,
        CharacterId WitnessId,
        DialogueEvent SourceEvent,
        DialogueEvent DeliveredEvent,
        CharacterDialogueMisquotationEvidence Evidence,
        CharacterDialogueMisquotationEvidenceConsiderationRequest Request);
}
