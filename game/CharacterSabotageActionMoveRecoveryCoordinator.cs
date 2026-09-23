using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Recovers one exact eligible follow-up Move through its existing state owner.</summary>
public partial class CharacterSabotageActionMoveRecoveryCoordinator : Node
{
    private CharacterSabotageActionMoveRecoveryEligibilityCoordinator eligibility = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact recovery candidate applied here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <summary>Gets the exact evidence view used for recovery.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <summary>Gets the exact fresh recovered placements installed by the state owner.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        eligibility = GetNode<CharacterSabotageActionMoveRecoveryEligibilityCoordinator>(
            "../CharacterSabotageActionMoveRecoveryEligibilityCoordinator");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Recovers once from only the exact current published candidate and view.</summary>
    public bool TryRecover(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        WorldObjectPlacementSnapshot? placements = state.Placements;
        DiscoveryEvidenceSnapshot? evidence = state.Evidence;
        if (RecoveryCandidate is not null
            || EvidenceView is not null
            || RecoveredPlacements is not null
            || placements is null
            || evidence is null
            || !eligibility.IsCurrentEligibility(expectedCandidate, expectedEvidenceView)
            || !state.TryRecover(
                placements,
                evidence,
                expectedEvidenceView!,
                expectedCandidate!))
        {
            return false;
        }

        WorldObjectPlacementSnapshot recovered = state.Placements
            ?? throw new InvalidOperationException(
                "Successful follow-up Move recovery must install a placement snapshot.");
        if (ReferenceEquals(recovered, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Follow-up Move recovery must replace only placements and retain evidence.");
        }

        RecoveryCandidate = expectedCandidate;
        EvidenceView = expectedEvidenceView;
        RecoveredPlacements = recovered;
        return true;
    }
}
