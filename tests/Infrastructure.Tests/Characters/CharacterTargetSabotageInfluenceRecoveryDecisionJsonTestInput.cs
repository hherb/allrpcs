using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Infrastructure.Tests.Characters;

internal static class CharacterTargetSabotageInfluenceRecoveryDecisionJsonTestInput
{
    internal static Input Create(int candidateCount = 1)
    {
        Character enemy = RepresentativeEnemyAgentCharacterSource.Create();
        Character actor = RepresentativeSageCharacterSource.Create();
        CharacterTargetSabotageOccurrence occurrence = new(
            new CharacterTargetSabotageProposal(
                enemy.Identity.Id,
                new CharacterTargetAccessRequirement(
                    actor.Identity.Id,
                    "assess-local-influence",
                    enemy.Knowledge.KnowledgeReferences.Single()),
                CharacterSabotageActionKind.Influence),
            actor);
        CharacterRelationshipChange influenceChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            1,
            Reason("influence-attempt", "enemy-agent-influence"));
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = new(
            occurrence,
            influenceChange);
        CharacterRelationshipMemory priorRelationship = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            [new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 10)],
            []);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipChangeTransition.Apply(priorRelationship, influenceChange);
        CharacterTargetSabotageInfluenceResult influenceResult = new(
            influenceCandidate,
            relationship);
        CharacterRelationshipChange recoveryChange = new(
            actor.Identity.Id,
            enemy.Identity.Id,
            -1,
            Reason("influence-recovered", "sage-recovery"));
        CharacterTargetSabotageInfluenceRecoveryCandidate[] candidates = Enumerable
            .Range(0, candidateCount)
            .Select(_ => new CharacterTargetSabotageInfluenceRecoveryCandidate(
                influenceResult,
                recoveryChange))
            .ToArray();
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request = new(
            actor,
            relationship,
            candidates);

        return new(
            actor,
            relationship,
            influenceChange,
            influenceResult,
            recoveryChange,
            candidates,
            request);
    }

    private static CharacterRelationshipReason Reason(string id, string turnId) => new(
        new RelationshipReasonId(id),
        CharacterRelationshipDimension.Trust,
        new CharacterTurnId(turnId),
        id);

    internal sealed record Input(
        Character Actor,
        CharacterRelationshipMemory Relationship,
        CharacterRelationshipChange InfluenceChange,
        CharacterTargetSabotageInfluenceResult InfluenceResult,
        CharacterRelationshipChange RecoveryChange,
        IReadOnlyList<CharacterTargetSabotageInfluenceRecoveryCandidate> Candidates,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest Request);
}
