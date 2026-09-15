using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh contamination state and the exact evidence authorizing recovery.</summary>
public sealed record CharacterObjectSabotageContaminationRecoveryResult
{
    /// <summary>Creates one immutable contamination-recovery result.</summary>
    public CharacterObjectSabotageContaminationRecoveryResult(
        WorldObjectContaminationSnapshot contamination,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(contamination);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        Contamination = contamination;
        EvidenceView = evidenceView;
        Candidate = candidate;
    }

    /// <summary>Gets the fresh snapshot with the recovered target absent.</summary>
    public WorldObjectContaminationSnapshot Contamination { get; }

    /// <summary>Gets the exact character evidence view that authorized recovery.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }

    /// <summary>Gets the exact evidence-bound contamination candidate.</summary>
    public CharacterObjectSabotageEvidence Candidate { get; }
}
