namespace Allrpcs.Domain.Characters;

/// <summary>Requires exact current influence state and an applicable inverse change.</summary>
public static class CharacterTargetSabotageInfluenceRecoveryPolicy
{
    /// <summary>Returns whether the exact current influence result may be reversed once.</summary>
    public static bool CanRecover(
        CharacterRelationshipMemory currentRelationship,
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(currentRelationship);
        ArgumentNullException.ThrowIfNull(candidate);

        CharacterTargetSabotageInfluenceResult influenceResult = candidate.InfluenceResult;
        CharacterRelationshipChange appliedChange =
            influenceResult.Candidate.RelationshipChange;
        CharacterRelationshipChange recoveryChange = candidate.RecoveryChange;
        IReadOnlyList<CharacterRelationshipReason> currentReasons = currentRelationship.Reasons;

        return ReferenceEquals(currentRelationship, influenceResult.Relationship)
            && influenceResult.Candidate.Occurrence.Proposal.ActionKind
                == CharacterSabotageActionKind.Influence
            && currentReasons.Count > 0
            && ReferenceEquals(currentReasons[^1], appliedChange.Reason)
            && recoveryChange.CharacterId == appliedChange.CharacterId
            && recoveryChange.RelatedCharacterId == appliedChange.RelatedCharacterId
            && recoveryChange.Reason.Dimension == appliedChange.Reason.Dimension
            && recoveryChange.Delta == -appliedChange.Delta
            && CharacterRelationshipChangePolicy.CanApply(
                currentRelationship,
                recoveryChange);
    }
}
