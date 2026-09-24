using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact continuation Move completion admitted to scheduling.</summary>
public partial class CharacterSabotageActionMoveCycleContinuationTurnAdmissionCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationTurnCompletionCoordinator completion = null!;
    private CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact completed-turn token admitted once by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        completion = GetNode<CharacterSabotageActionMoveCycleContinuationTurnCompletionCoordinator>(
            "../CharacterSabotageActionMoveCycleContinuationTurnCompletionCoordinator");
        recovery = GetNode<CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator>(
            "../CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator");
    }

    /// <summary>Retains only the exact current completion accepted by the scheduling gate.</summary>
    public bool TryAdmit(CharacterSabotageTurnCompletion? expectedCompletion)
    {
        CharacterSabotageTurnCompletion? currentCompletion = completion.Completion;
        if (Admission is not null
            || !completion.IsCurrentCompletion(
                expectedCompletion,
                recovery.RecoveredPlacements,
                recovery.RecoveredEvidence)
            || !CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                currentCompletion,
                expectedCompletion))
        {
            return false;
        }

        Admission = currentCompletion;
        return true;
    }

    /// <summary>Checks the exact admitted completion against its current route.</summary>
    internal bool IsCurrentAdmission(CharacterSabotageTurnCompletion? expectedAdmission) =>
        expectedAdmission is not null
        && ReferenceEquals(Admission, expectedAdmission)
        && completion.IsCurrentCompletion(
            expectedAdmission,
            recovery.RecoveredPlacements,
            recovery.RecoveredEvidence)
        && CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
            completion.Completion,
            expectedAdmission);
}
