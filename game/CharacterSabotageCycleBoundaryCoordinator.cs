using Godot;

namespace Allrpcs.Game;

/// <summary>Retains exact provenance for one explicitly entered sabotage-cycle boundary.</summary>
public sealed class CharacterSabotageCycleBoundary
{
    /// <summary>Creates one fresh boundary from the exact current scheduling authorization.</summary>
    internal CharacterSabotageCycleBoundary(
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization)
    {
        Authorization = authorization;
    }

    /// <summary>Gets the exact ordinary-turn scheduling authorization consumed at this boundary.</summary>
    public CharacterSabotageOrdinaryTurnSchedulingAuthorization Authorization { get; }
}

/// <summary>Consumes one exact scheduling authorization at an explicit sabotage-cycle boundary.</summary>
public partial class CharacterSabotageCycleBoundaryCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator authorization = null!;
    private CharacterSabotageTurnAdmissionCoordinator admission = null!;
    private CharacterSabotageOrdinaryTurnBoundaryCoordinator turnBoundary = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the one exact sabotage-cycle boundary retained here.</summary>
    public CharacterSabotageCycleBoundary? Boundary { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        authorization =
            GetNode<CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator");
        admission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turnBoundary = GetNode<CharacterSabotageOrdinaryTurnBoundaryCoordinator>(
            "../CharacterSabotageOrdinaryTurnBoundaryCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Enters once only with the exact current revalidated authorization.</summary>
    public bool TryEnter(
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? expectedAuthorization)
    {
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? currentAuthorization =
            authorization.Authorization;
        if (Boundary is not null
            || currentAuthorization is null
            || !ReferenceEquals(currentAuthorization, expectedAuthorization)
            || !ReferenceEquals(currentAuthorization.Admission, admission.Admission)
            || !ReferenceEquals(currentAuthorization.Admission, turnBoundary.Admission)
            || !ReferenceEquals(currentAuthorization.TurnActivity, turnBoundary.TurnActivity)
            || !CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                admission.Admission,
                currentAuthorization.Admission,
                turnBoundary.TurnActivity,
                turns.Activity))
        {
            return false;
        }

        Boundary = new CharacterSabotageCycleBoundary(currentAuthorization);
        return true;
    }
}
