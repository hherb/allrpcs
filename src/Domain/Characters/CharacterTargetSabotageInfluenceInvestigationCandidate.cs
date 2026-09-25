using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Binds one exact recovered influence attempt to authored discoverable evidence.</summary>
public sealed record CharacterTargetSabotageInfluenceInvestigationCandidate
{
    /// <summary>Creates one immutable investigation candidate without exposing evidence.</summary>
    public CharacterTargetSabotageInfluenceInvestigationCandidate(
        CharacterTargetSabotageInfluenceRecoveryResult recoveryResult,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(recoveryResult);
        ArgumentNullException.ThrowIfNull(evidenceId);

        RecoveryResult = recoveryResult;
        EvidenceId = evidenceId;
    }

    /// <summary>Gets the exact recovered influence history that may be investigated.</summary>
    public CharacterTargetSabotageInfluenceRecoveryResult RecoveryResult { get; }

    /// <summary>Gets the authored evidence identifier exposed by successful investigation.</summary>
    public EvidenceId EvidenceId { get; }
}
