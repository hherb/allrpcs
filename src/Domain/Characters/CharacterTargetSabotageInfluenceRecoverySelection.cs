namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible influence-recovery candidate without applying it.</summary>
public static class CharacterTargetSabotageInfluenceRecoverySelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterTargetSabotageInfluenceRecoveryCandidate? SelectFirstEligible(
        CharacterRelationshipMemory currentRelationship,
        IEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(currentRelationship);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageInfluenceRecoveryCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageInfluenceRecoveryCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageInfluenceRecoveryCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Influence-recovery candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Influence-recovery candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Influence-recovery selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(
                currentRelationship,
                candidate));
    }
}
