using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh substitution state and the exact authority for recovery.</summary>
public sealed record CharacterObjectSabotageSubstitutionRecoveryResult
{
    /// <summary>Creates one immutable substitution-recovery result.</summary>
    public CharacterObjectSabotageSubstitutionRecoveryResult(
        CharacterObjectSabotageSubstitutionSnapshot substitutions,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageSubstitutionCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(substitutions);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        Substitutions = substitutions;
        EvidenceView = evidenceView;
        Candidate = candidate;
    }

    /// <summary>Gets the fresh snapshot with the recovered substitution absent.</summary>
    public CharacterObjectSabotageSubstitutionSnapshot Substitutions { get; }

    /// <summary>Gets the exact character evidence view that authorized recovery.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }

    /// <summary>Gets the exact evidence-bound substitution candidate.</summary>
    public CharacterObjectSabotageSubstitutionCandidate Candidate { get; }
}
