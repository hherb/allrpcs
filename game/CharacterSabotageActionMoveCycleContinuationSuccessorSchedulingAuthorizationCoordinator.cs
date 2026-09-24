using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Authorizes scheduling for one exact current fifth-cycle Move boundary.</summary>
public partial class
    CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator : Node
{
    private CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator
        boundary = null!;

    /// <summary>Gets the one exact fifth-cycle scheduling authorization retained here.</summary>
    public CharacterSabotageOrdinaryTurnSchedulingAuthorization? Authorization { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        boundary = GetNode<
            CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator");
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
            || !boundary.IsCurrentBoundary(boundAdmission, boundTurnActivity))
        {
            return false;
        }

        Authorization = new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
            boundAdmission,
            boundTurnActivity);
        return true;
    }

    /// <summary>Checks the exact authorization against its complete current boundary graph.</summary>
    internal bool IsCurrentAuthorization(
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? expectedAuthorization) =>
        expectedAuthorization is not null
        && ReferenceEquals(Authorization, expectedAuthorization)
        && ReferenceEquals(expectedAuthorization.Admission, boundary.Admission)
        && ReferenceEquals(expectedAuthorization.TurnActivity, boundary.TurnActivity)
        && boundary.IsCurrentBoundary(
            expectedAuthorization.Admission,
            expectedAuthorization.TurnActivity);
}
