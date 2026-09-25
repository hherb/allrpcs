using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterTargetSabotageInfluenceInvestigationTests
{
    [Fact]
    public void ExactReachableRecoveredInfluenceProducesOnlyItsAuthoredEvidence()
    {
        InvestigationInput input = CreateInput();

        Assert.True(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            input.Recovery,
            input.Recovery.Relationship,
            input.Candidate,
            input.Candidate));

        CharacterEvidenceViewSnapshot view =
            CharacterTargetSabotageInfluenceInvestigationTransition.Investigate(
                input.Investigator,
                input.Access,
                input.Recovery,
                input.Recovery.Relationship,
                input.Candidate,
                input.Candidate);

        Assert.Same(input.Investigator.Identity.Id, view.CharacterId);
        Assert.Collection(view.EvidenceIds, evidence => Assert.Same(input.EvidenceId, evidence));
    }

    [Fact]
    public void RejectsInaccessibleRebuiltStaleAndIncompleteHistory()
    {
        InvestigationInput input = CreateInput();
        CharacterInteractionAccessSnapshot inaccessible = new(
            input.Investigator.Identity.Id,
            []);
        CharacterTargetSabotageInfluenceRecoveryResult rebuiltRecovery = new(
            input.Recovery.Candidate,
            input.Recovery.Relationship);
        CharacterRelationshipMemory rebuiltRelationship = new(
            input.Recovery.Relationship.CharacterId,
            input.Recovery.Relationship.RelatedCharacterId,
            input.Recovery.Relationship.Dimensions,
            input.Recovery.Relationship.Reasons);
        CharacterTargetSabotageInfluenceInvestigationCandidate rebuiltCandidate = new(
            input.Recovery,
            input.EvidenceId);
        CharacterTargetSabotageInfluenceRecoveryResult incomplete = new(
            input.Recovery.Candidate,
            input.Recovery.Candidate.InfluenceResult.Relationship);
        CharacterTargetSabotageInfluenceInvestigationCandidate incompleteCandidate = new(
            incomplete,
            input.EvidenceId);

        Assert.False(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            inaccessible,
            input.Recovery,
            input.Recovery.Relationship,
            input.Candidate,
            input.Candidate));
        Assert.False(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            rebuiltRecovery,
            input.Recovery.Relationship,
            input.Candidate,
            input.Candidate));
        Assert.False(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            input.Recovery,
            rebuiltRelationship,
            input.Candidate,
            input.Candidate));
        Assert.False(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            input.Recovery,
            input.Recovery.Relationship,
            input.Candidate,
            rebuiltCandidate));
        Assert.False(CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
            input.Investigator,
            input.Access,
            incomplete,
            incomplete.Relationship,
            incompleteCandidate,
            incompleteCandidate));
    }

    [Fact]
    public void ContractsRejectNullAndFailedInvestigationWithoutPartialOutput()
    {
        InvestigationInput input = CreateInput();

        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceInvestigationCandidate(null!, input.EvidenceId));
        Assert.Throws<ArgumentNullException>(() =>
            new CharacterTargetSabotageInfluenceInvestigationCandidate(input.Recovery, null!));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTargetSabotageInfluenceInvestigationPolicy.CanInvestigate(
                null!,
                input.Access,
                input.Recovery,
                input.Recovery.Relationship,
                input.Candidate,
                input.Candidate));
        Assert.Throws<InvalidOperationException>(() =>
            CharacterTargetSabotageInfluenceInvestigationTransition.Investigate(
                input.Investigator,
                new CharacterInteractionAccessSnapshot(input.Investigator.Identity.Id, []),
                input.Recovery,
                input.Recovery.Relationship,
                input.Candidate,
                input.Candidate));
    }

    private static InvestigationInput CreateInput()
    {
        Character investigator = RepresentativeHermitCharacterSource.Create()[0];
        Character actor = RepresentativeEnemyAgentCharacterSource.Create();
        Character target = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                actor.Identity.Id,
                new CharacterTargetAccessRequirement(
                    target.Identity.Id,
                    "assess-local-influence",
                    actor.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            target);
        CharacterRelationshipMemory before = new(
            target.Identity.Id,
            actor.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipReason influenceReason = new(
            new RelationshipReasonId("investigated-influence"),
            CharacterRelationshipDimension.Trust,
            new CharacterTurnId("investigated-influence"),
            "The enemy attempted to gain influence.");
        CharacterRelationshipChange influenceChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            1,
            influenceReason);
        CharacterRelationshipMemory influenced = CharacterRelationshipChangeTransition.Apply(
            before,
            influenceChange);
        CharacterTargetSabotageInfluenceResult influence = new(
            new CharacterTargetSabotageInfluenceCandidate(occurrence, influenceChange),
            influenced);
        CharacterRelationshipChange recoveryChange = new(
            target.Identity.Id,
            actor.Identity.Id,
            -1,
            new CharacterRelationshipReason(
                new RelationshipReasonId("investigated-influence-recovery"),
                CharacterRelationshipDimension.Trust,
                new CharacterTurnId("investigated-influence-recovery"),
                "The influence attempt was corrected."));
        CharacterTargetSabotageInfluenceRecoveryCandidate recoveryCandidate = new(
            influence,
            recoveryChange);
        CharacterTargetSabotageInfluenceRecoveryResult recovery =
            CharacterTargetSabotageInfluenceRecoveryTransition.Recover(
                influenced,
                recoveryCandidate);
        EvidenceId evidenceId = new("sage-influence-history");

        return new InvestigationInput(
            investigator,
            new CharacterInteractionAccessSnapshot(
                investigator.Identity.Id,
                [target.Identity.Id]),
            recovery,
            evidenceId,
            new CharacterTargetSabotageInfluenceInvestigationCandidate(recovery, evidenceId));
    }

    private sealed record InvestigationInput(
        Character Investigator,
        CharacterInteractionAccessSnapshot Access,
        CharacterTargetSabotageInfluenceRecoveryResult Recovery,
        EvidenceId EvidenceId,
        CharacterTargetSabotageInfluenceInvestigationCandidate Candidate);
}
