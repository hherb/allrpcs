namespace Allrpcs.Domain.Characters;

/// <summary>Retains exact bounded relationship state for one provider-neutral influence choice.</summary>
/// <remarks>Instances use reference identity; copied collections do not define value equality.</remarks>
public sealed class CharacterTargetSabotageInfluenceConsiderationRequest
{
    /// <summary>Creates one immutable request without selecting or applying influence.</summary>
    public CharacterTargetSabotageInfluenceConsiderationRequest(
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
                    "Influence consideration candidates cannot contain null values.",
                    nameof(candidates));
            }

            if (!distinctCandidates.Add(candidate))
            {
                throw new ArgumentException(
                    "Influence consideration candidates cannot contain duplicate exact references.",
                    nameof(candidates));
            }

            retainedCandidates.Add(candidate);
            if (retainedCandidates.Count
                > CharacterTargetSabotageInfluenceSelection.MaximumCandidateCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(candidates),
                    retainedCandidates.Count,
                    "Influence consideration exceeded the bounded candidate count.");
            }
        }

        Actor = actor;
        InteractionAccess = interactionAccess;
        Relationship = relationship;
        Candidates = retainedCandidates.AsReadOnly();
    }

    /// <summary>Gets the exact character considering influence.</summary>
    public Character Actor { get; }

    /// <summary>Gets the exact authoritative ordinary-interaction access snapshot.</summary>
    public CharacterInteractionAccessSnapshot InteractionAccess { get; }

    /// <summary>Gets the exact current directional relationship state.</summary>
    public CharacterRelationshipMemory Relationship { get; }

    /// <summary>Gets stable bounded exact candidates in caller order.</summary>
    public IReadOnlyList<CharacterTargetSabotageInfluenceCandidate> Candidates { get; }
}
