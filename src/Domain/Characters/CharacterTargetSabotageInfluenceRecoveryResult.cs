namespace Allrpcs.Domain.Characters;

/// <summary>Contains one accepted influence recovery and its fresh relationship state.</summary>
public sealed record CharacterTargetSabotageInfluenceRecoveryResult
{
    /// <summary>Creates one immutable influence-recovery result.</summary>
    public CharacterTargetSabotageInfluenceRecoveryResult(
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate,
        CharacterRelationshipMemory relationship)
    {
        ArgumentNullException.ThrowIfNull(candidate);
        ArgumentNullException.ThrowIfNull(relationship);

        Candidate = candidate;
        Relationship = relationship;
    }

    /// <summary>Gets the exact recovery candidate that was applied.</summary>
    public CharacterTargetSabotageInfluenceRecoveryCandidate Candidate { get; }

    /// <summary>Gets the fresh numerically restored relationship state.</summary>
    public CharacterRelationshipMemory Relationship { get; }
}
