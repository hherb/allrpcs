namespace Allrpcs.Domain.Characters;

/// <summary>Applies one exact-current inverse influence change to immutable relationship state.</summary>
public static class CharacterTargetSabotageInfluenceRecoveryTransition
{
    /// <summary>Returns the exact recovery candidate and freshly restored relationship.</summary>
    public static CharacterTargetSabotageInfluenceRecoveryResult Recover(
        CharacterRelationshipMemory currentRelationship,
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(currentRelationship);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                currentRelationship,
                candidate))
        {
            throw new InvalidOperationException(
                "Only an exact-current eligible inverse influence change may be recovered.");
        }

        CharacterRelationshipMemory resultingRelationship =
            CharacterRelationshipChangeTransition.Apply(
                currentRelationship,
                candidate.RecoveryChange);

        return new CharacterTargetSabotageInfluenceRecoveryResult(
            candidate,
            resultingRelationship);
    }
}
