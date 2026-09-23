using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact follow-up Move completion admitted to ordinary scheduling.</summary>
public partial class CharacterSabotageActionMoveTurnAdmissionCoordinator : Node
{
    private CharacterSabotageActionMoveTurnCompletionCoordinator completion = null!;
    private CharacterSabotageActionMoveRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact completed-turn token admitted once by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        completion = GetNode<CharacterSabotageActionMoveTurnCompletionCoordinator>(
            "../CharacterSabotageActionMoveTurnCompletionCoordinator");
        recovery = GetNode<CharacterSabotageActionMoveRecoveryCoordinator>(
            "../CharacterSabotageActionMoveRecoveryCoordinator");
    }

    /// <summary>Retains only the exact current completion accepted by the scheduling gate.</summary>
    public bool TryAdmit(CharacterSabotageTurnCompletion? expectedCompletion)
    {
        CharacterSabotageTurnCompletion? currentCompletion = completion.Completion;
        if (Admission is not null
            || !completion.IsCurrentCompletion(
                expectedCompletion,
                recovery.RecoveredPlacements)
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
            recovery.RecoveredPlacements)
        && CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
            completion.Completion,
            expectedAdmission);
}
