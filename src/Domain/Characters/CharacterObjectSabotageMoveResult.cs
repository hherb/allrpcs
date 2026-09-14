using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains fresh placement and evidence state from one accepted move sabotage.</summary>
public sealed class CharacterObjectSabotageMoveResult
{
    /// <summary>Creates one immutable move-sabotage result.</summary>
    public CharacterObjectSabotageMoveResult(
        WorldObjectPlacementSnapshot placements,
        CharacterObjectSabotageEvidenceResult evidenceResult)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidenceResult);

        Placements = placements;
        EvidenceResult = evidenceResult;
    }

    /// <summary>Gets the fresh snapshot containing the exact target at its exact destination.</summary>
    public WorldObjectPlacementSnapshot Placements { get; }

    /// <summary>Gets the existing fresh evidence result for the same exact occurrence.</summary>
    public CharacterObjectSabotageEvidenceResult EvidenceResult { get; }
}
