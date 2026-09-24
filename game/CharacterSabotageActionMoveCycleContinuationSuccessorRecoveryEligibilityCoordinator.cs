using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Godot;

namespace Allrpcs.Game;

/// <summary>Publishes one exact eligible fifth-cycle Move recovery without recovering it.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator
        candidates = null!;

    /// <summary>Gets the exact recovery candidate admitted here.</summary>
    public CharacterObjectSabotageMoveRecoveryCandidate? RecoveryCandidate { get; private set; }

    /// <summary>Gets the exact retained evidence view admitted here.</summary>
    public CharacterEvidenceViewSnapshot? EvidenceView { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidates = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator");
    }

    /// <summary>Admits once only the exact current candidate and retained evidence view.</summary>
    public bool TryPublish(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView)
    {
        if (RecoveryCandidate is not null
            || EvidenceView is not null
            || !candidates.IsCurrentRecoveryCandidate(expectedCandidate, expectedEvidenceView))
        {
            return false;
        }

        RecoveryCandidate = expectedCandidate;
        EvidenceView = expectedEvidenceView;
        return true;
    }

    /// <summary>Checks the exact admitted pair against current recovery authority.</summary>
    internal bool IsCurrentEligibility(
        CharacterObjectSabotageMoveRecoveryCandidate? expectedCandidate,
        CharacterEvidenceViewSnapshot? expectedEvidenceView) =>
        ReferenceEquals(RecoveryCandidate, expectedCandidate)
        && ReferenceEquals(EvidenceView, expectedEvidenceView)
        && candidates.IsCurrentRecoveryCandidate(expectedCandidate, expectedEvidenceView);
}
