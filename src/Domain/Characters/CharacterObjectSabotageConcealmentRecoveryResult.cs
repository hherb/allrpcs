using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh concealment state and the exact evidence authorizing recovery.</summary>
public sealed record CharacterObjectSabotageConcealmentRecoveryResult
{
    /// <summary>Creates one immutable concealment-recovery result.</summary>
    public CharacterObjectSabotageConcealmentRecoveryResult(
        WorldObjectConcealmentSnapshot concealment,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageEvidence candidate)
    {
        ArgumentNullException.ThrowIfNull(concealment);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        Concealment = concealment;
        EvidenceView = evidenceView;
        Candidate = candidate;
    }

    /// <summary>Gets the fresh snapshot with the recovered target absent.</summary>
    public WorldObjectConcealmentSnapshot Concealment { get; }

    /// <summary>Gets the exact character evidence view that authorized recovery.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }

    /// <summary>Gets the exact evidence-bound concealment candidate.</summary>
    public CharacterObjectSabotageEvidence Candidate { get; }
}
