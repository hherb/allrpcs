namespace Allrpcs.Domain.Characters;

/// <summary>Contains one accepted influence candidate and its fresh relationship state.</summary>
public sealed record CharacterTargetSabotageInfluenceResult
{
    /// <summary>Creates one immutable influence result.</summary>
    public CharacterTargetSabotageInfluenceResult(
        CharacterTargetSabotageInfluenceCandidate candidate,
        CharacterRelationshipMemory relationship)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(relationship);

        Candidate = candidate;
        Relationship = relationship;
    }

    /// <summary>Gets the exact influence candidate that was applied.</summary>
    public CharacterTargetSabotageInfluenceCandidate Candidate { get; }

    /// <summary>Gets the fresh target-to-actor relationship state.</summary>
    public CharacterRelationshipMemory Relationship { get; }
}
