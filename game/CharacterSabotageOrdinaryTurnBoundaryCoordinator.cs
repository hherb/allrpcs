using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one exact sabotage admission to the ordinary character-turn start boundary.</summary>
public partial class CharacterSabotageOrdinaryTurnBoundaryCoordinator : Node
{
    private CharacterSabotageTurnAdmissionCoordinator admission = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact sabotage completion admitted at the ordinary turn boundary.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <summary>Gets the exact ordinary turn activity observed at successful binding.</summary>
    public CharacterTurnActivity? TurnActivity { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Binds the exact admission only while the exact ordinary turn slot can start.</summary>
    public bool TryBind(
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? expectedTurnActivity)
    {
        CharacterSabotageTurnCompletion? currentAdmission = admission.Admission;
        CharacterTurnActivity? currentTurnActivity = turns.Activity;
        if (Admission is not null
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
}
