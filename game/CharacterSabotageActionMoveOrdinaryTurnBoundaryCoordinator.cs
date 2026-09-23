using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one follow-up Move admission to the ordinary turn start boundary.</summary>
public partial class CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator : Node
{
    private CharacterSabotageActionMoveTurnAdmissionCoordinator admission = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact follow-up completion admitted at the ordinary boundary.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <summary>Gets the exact ordinary turn activity observed at successful binding.</summary>
    public CharacterTurnActivity? TurnActivity { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admission = GetNode<CharacterSabotageActionMoveTurnAdmissionCoordinator>(
            "../CharacterSabotageActionMoveTurnAdmissionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Binds the exact admission only while the ordinary turn slot can start.</summary>
    public bool TryBind(
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? expectedTurnActivity)
    {
        CharacterSabotageTurnCompletion? currentAdmission = admission.Admission;
        CharacterTurnActivity? currentTurnActivity = turns.Activity;
        if (Admission is not null
            || !admission.IsCurrentAdmission(expectedAdmission)
            || !CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                currentAdmission,
                expectedAdmission,
                currentTurnActivity,
                expectedTurnActivity))
        {
            return false;
        }

        Admission = currentAdmission;
        TurnActivity = currentTurnActivity;
        return true;
    }

    /// <summary>Checks the exact bound admission and still-current ordinary turn reference.</summary>
    internal bool IsCurrentBoundary(
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? expectedTurnActivity) =>
        expectedAdmission is not null
        && ReferenceEquals(Admission, expectedAdmission)
        && ReferenceEquals(TurnActivity, expectedTurnActivity)
        && admission.IsCurrentAdmission(expectedAdmission)
        && CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
            admission.Admission,
            expectedAdmission,
            TurnActivity,
            turns.Activity);
}
