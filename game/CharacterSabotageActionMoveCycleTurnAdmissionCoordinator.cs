using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact third-cycle Move completion admitted to scheduling.</summary>
public partial class CharacterSabotageActionMoveCycleTurnAdmissionCoordinator : Node
{
    private CharacterSabotageActionMoveCycleTurnCompletionCoordinator completion = null!;
    private CharacterSabotageActionMoveCycleRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact completed-turn token admitted once by this coordinator.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        completion = GetNode<CharacterSabotageActionMoveCycleTurnCompletionCoordinator>(
            "../CharacterSabotageActionMoveCycleTurnCompletionCoordinator");
        recovery = GetNode<CharacterSabotageActionMoveCycleRecoveryCoordinator>(
            "../CharacterSabotageActionMoveCycleRecoveryCoordinator");
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
