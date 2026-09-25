namespace Allrpcs.Domain.Characters;

/// <summary>Requires exact recovered Influence history and ordinary access to its target.</summary>
public static class CharacterTargetSabotageInfluenceInvestigationPolicy
{
    /// <summary>Returns whether one investigator may expose the candidate's authored evidence.</summary>
    public static bool CanInvestigate(
        Character investigator,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetSabotageInfluenceRecoveryResult currentRecovery,
        CharacterRelationshipMemory currentRelationship,
        CharacterTargetSabotageInfluenceInvestigationCandidate currentCandidate,
        CharacterTargetSabotageInfluenceInvestigationCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(investigator);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(currentRecovery);
        ArgumentNullException.ThrowIfNull(currentRelationship);
        ArgumentNullException.ThrowIfNull(currentCandidate);
        ArgumentNullException.ThrowIfNull(candidate);

        CharacterTargetSabotageInfluenceRecoveryResult recovery = candidate.RecoveryResult;
        CharacterTargetSabotageInfluenceRecoveryCandidate recoveryCandidate = recovery.Candidate;
        CharacterTargetSabotageInfluenceResult influence = recoveryCandidate.InfluenceResult;
        CharacterTargetSabotageInfluenceCandidate influenceCandidate = influence.Candidate;

        return ReferenceEquals(candidate, currentCandidate)
            && ReferenceEquals(recovery, currentRecovery)
            && ReferenceEquals(recovery.Relationship, currentRelationship)
            && ReferenceEquals(interactionAccess.ActorId, investigator.Identity.Id)
            && interactionAccess.ReachableTargetCharacterIds.Any(targetId =>
                ReferenceEquals(targetId, influenceCandidate.Occurrence.Target.Identity.Id))
            && ReferenceEquals(
                influenceCandidate.Occurrence.Proposal.ActionKind,
                CharacterSabotageActionKind.Influence)
            && influence.Relationship.Reasons.Count > 0
            && ReferenceEquals(
                influence.Relationship.Reasons[^1],
                influenceCandidate.RelationshipChange.Reason)
            && recovery.Relationship.Reasons.Count == influence.Relationship.Reasons.Count + 1
            && influence.Relationship.Reasons.Select((reason, index) =>
                ReferenceEquals(reason, recovery.Relationship.Reasons[index])).All(value => value)
            && ReferenceEquals(
                recovery.Relationship.Reasons[^1],
                recoveryCandidate.RecoveryChange.Reason)
            && recoveryCandidate.RecoveryChange.Delta
                == -influenceCandidate.RelationshipChange.Delta;
    }
}
