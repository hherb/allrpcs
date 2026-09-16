namespace Allrpcs.Domain.Characters;

/// <summary>Selects one exact eligible influence candidate without applying it.</summary>
public static class CharacterTargetSabotageInfluenceSelection
{
    /// <summary>Gets the maximum number of candidates accepted in one selection.</summary>
    public const int MaximumCandidateCount = 64;

    /// <summary>Returns the exact first eligible candidate in stable caller order, or null.</summary>
    public static CharacterTargetSabotageInfluenceCandidate? SelectFirstEligible(
        Character actor,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterRelationshipMemory relationship,
        IEnumerable<CharacterTargetSabotageInfluenceCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageInfluenceCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageInfluenceCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageInfluenceCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Influence candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Influence candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count > MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    $"Influence selection accepts at most {MaximumCandidateCount} candidates.");
            }
        }

        return retainedCandidates.FirstOrDefault(candidate =>
            CharacterTargetSabotageInfluencePolicy.CanApply(
                actor,
                interactionAccess,
                relationship,
                candidate));
    }
}
