using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains exact references authorizing one future ordinary-turn scheduling step.</summary>
public sealed class CharacterSabotageOrdinaryTurnSchedulingAuthorization
{
    /// <summary>Creates one fresh authorization from the exact revalidated boundary references.</summary>
    internal CharacterSabotageOrdinaryTurnSchedulingAuthorization(
        CharacterSabotageTurnCompletion admission,
        CharacterTurnActivity? turnActivity)
    {
        Admission = admission;
        TurnActivity = turnActivity;
    }

    /// <summary>Gets the exact sabotage completion admitted at the ordinary turn boundary.</summary>
    public CharacterSabotageTurnCompletion Admission { get; }

    /// <summary>Gets the exact ordinary turn activity observed at authorization.</summary>
    public CharacterTurnActivity? TurnActivity { get; }
}

/// <summary>Authorizes one exact revalidated sabotage admission without reserving its turn.</summary>
public partial class CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator : Node
{
    private CharacterSabotageTurnAdmissionCoordinator admission = null!;
    private CharacterSabotageOrdinaryTurnBoundaryCoordinator boundary = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the one exact ordinary-turn scheduling authorization retained here.</summary>
    public CharacterSabotageOrdinaryTurnSchedulingAuthorization? Authorization { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        boundary = GetNode<CharacterSabotageOrdinaryTurnBoundaryCoordinator>(
            "../CharacterSabotageOrdinaryTurnBoundaryCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Retains fresh authorization only for the exact still-current boundary graph.</summary>
    public bool TryAuthorize(
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? expectedTurnActivity)
    {
        CharacterSabotageTurnCompletion? boundAdmission = boundary.Admission;
        CharacterTurnActivity? boundTurnActivity = boundary.TurnActivity;
        if (Authorization is not null
            || boundAdmission is null
            || !ReferenceEquals(boundAdmission, expectedAdmission)
            || !ReferenceEquals(boundTurnActivity, expectedTurnActivity)
            || !CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                admission.Admission,
                boundAdmission,
                boundTurnActivity,
                turns.Activity))
        {
            return false;
        }

        Authorization = new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
            boundAdmission,
            boundTurnActivity);
        return true;
    }
}
