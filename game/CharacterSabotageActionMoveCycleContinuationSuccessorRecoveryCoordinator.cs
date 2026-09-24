using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Recovers one exact eligible fifth-cycle Move through its existing state owner.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator
    : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator
        eligibility = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact recovery candidate applied here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <summary>Gets the exact evidence view used for recovery.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <summary>Gets the exact fresh recovered placements installed by the state owner.</summary>
    public WorldObjectPlacementSnapshot? RecoveredPlacements { get; private set; }

    /// <summary>Gets the exact authoritative evidence retained across recovery.</summary>
    public DiscoveryEvidenceSnapshot? RecoveredEvidence { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        eligibility = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator");
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
            || RecoveredEvidence is not null
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
                "Successful fifth-cycle Move recovery must install a placement snapshot.");
        if (ReferenceEquals(recovered, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            throw new InvalidOperationException(
                "Fifth-cycle Move recovery must replace only placements and retain evidence.");
        }

        RecoveryCandidate = expectedCandidate;
        EvidenceView = expectedEvidenceView;
        RecoveredPlacements = recovered;
        RecoveredEvidence = evidence;
        return true;
    }

    /// <summary>Checks the exact recovery output against current authoritative state.</summary>
    internal bool IsCurrentRecovery(
        WorldObjectPlacementSnapshot? expectedPlacements,
        DiscoveryEvidenceSnapshot? expectedEvidence) =>
        RecoveryCandidate is not null
        && EvidenceView is not null
        && expectedPlacements is not null
        && expectedEvidence is not null
        && ReferenceEquals(RecoveredPlacements, expectedPlacements)
        && ReferenceEquals(RecoveredEvidence, expectedEvidence)
        && ReferenceEquals(state.Placements, expectedPlacements)
        && ReferenceEquals(state.Evidence, expectedEvidence);
}
