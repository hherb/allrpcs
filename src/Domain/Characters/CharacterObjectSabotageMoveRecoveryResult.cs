using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains restored placement state and the exact authority for move recovery.</summary>
public sealed record CharacterObjectSabotageMoveRecoveryResult
{
    /// <summary>Creates one immutable move-recovery result.</summary>
    public CharacterObjectSabotageMoveRecoveryResult(
        WorldObjectPlacementSnapshot placements,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        Placements = placements;
        EvidenceView = evidenceView;
        Candidate = candidate;
    }

    /// <summary>Gets the fresh placement snapshot containing the restored source placement.</summary>
    public WorldObjectPlacementSnapshot Placements { get; }

    /// <summary>Gets the exact character evidence view that authorized recovery.</summary>
    public CharacterEvidenceViewSnapshot EvidenceView { get; }

    /// <summary>Gets the exact recovery candidate.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate Candidate { get; }
}
