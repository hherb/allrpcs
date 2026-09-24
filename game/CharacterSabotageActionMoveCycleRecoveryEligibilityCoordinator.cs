using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Publishes one exact eligible third-cycle Move recovery without recovering it.</summary>
public partial class CharacterSabotageActionMoveCycleRecoveryEligibilityCoordinator : Node
{
    private CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator candidates = null!;
    private CharacterObjectSabotageMoveController state = null!;

    /// <summary>Gets the exact recovery candidate paired here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <summary>Gets the exact explicitly supplied evidence view paired here.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidates = GetNode<CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator>(
            "../CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator");
        state = GetNode<CharacterObjectSabotageMoveController>(
            "../CharacterObjectSabotageMoveController");
    }

    /// <summary>Pairs the exact current candidate and visible evidence once.</summary>
    public bool TryPublish(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (RecoveryCandidate is not null
            || EvidenceView is not null
            || expectedEvidenceView is null
            || !candidates.IsCurrentRecoveryCandidate(expectedCandidate)
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence
            || !CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                placements,
                evidence,
                expectedEvidenceView,
                expectedCandidate!))
        {
            return false;
        }

        RecoveryCandidate = expectedCandidate;
        EvidenceView = expectedEvidenceView;
        return true;
    }

    /// <summary>Checks the exact published pair against current recovery authority.</summary>
    internal bool IsCurrentEligibility(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView) =>
        ReferenceEquals(RecoveryCandidate, expectedCandidate)
        && ReferenceEquals(EvidenceView, expectedEvidenceView)
        && candidates.IsCurrentRecoveryCandidate(expectedCandidate)
        && state.Placements is { } placements
        && state.Evidence is { } evidence
        && CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
            placements,
            evidence,
            expectedEvidenceView!,
            expectedCandidate!);
}
