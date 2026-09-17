namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded state for one provider-neutral influence-recovery choice.</summary>
public sealed record CharacterTargetSabotageInfluenceRecoveryConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying recovery.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest(
        Character actor,
        CharacterRelationshipMemory relationship,
        IEnumerable<CharacterTargetSabotageInfluenceRecoveryCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(relationship);
        ArgumentNullException.ThrowIfNull(candidates);

        List<CharacterTargetSabotageInfluenceRecoveryCandidate> retainedCandidates = [];
        HashSet<CharacterTargetSabotageInfluenceRecoveryCandidate> distinctCandidates = new(
            ReferenceEqualityComparer.Instance);

        foreach (CharacterTargetSabotageInfluenceRecoveryCandidate? candidate in candidates)
        {
            if (candidate is null)
            {
                throw new ArgumentException(
                    "Influence-recovery consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Influence-recovery consideration candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterTargetSabotageInfluenceRecoverySelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Influence-recovery consideration exceeded the bounded candidate count.");
            }
        }

        Actor = actor;
        Relationship = relationship;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering influence recovery.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact current directional relationship state.</summary>
    public CharacterRelationshipMemory Relationship { get; }

    /// <summary>Gets stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterTargetSabotageInfluenceRecoveryCandidate> Candidates { get; }
}
