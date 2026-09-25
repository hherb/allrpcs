using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact recovered target delay to authored discoverable evidence.</summary>
public sealed record CharacterTargetSabotageDelayInvestigationCandidate
{
    /// <summary>Creates one immutable investigation candidate without exposing evidence.</summary>
    public CharacterTargetSabotageDelayInvestigationCandidate(
        CharacterTargetSabotageDelayRecoveryResult recoveryResult,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(recoveryResult);
        ArgumentNullException.ThrowIfNull(evidenceId);

        RecoveryResult = recoveryResult;
        EvidenceId = evidenceId;
    }

    /// <summary>Gets the exact recovered delay whose history may be investigated.</summary>
    public CharacterTargetSabotageDelayRecoveryResult RecoveryResult { get; }

    /// <summary>Gets the authored evidence identifier exposed by successful investigation.</summary>
    public EvidenceId EvidenceId { get; }
}
