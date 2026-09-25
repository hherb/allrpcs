using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one exact sabotage admission to the ordinary character-turn start boundary.</summary>
public partial class CharacterSabotageOrdinaryTurnBoundaryCoordinator : Node
{
    private CharacterSabotageTurnAdmissionCoordinator admission = null!;
    private CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator ordinaryAdmission = null!;
    private CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator resolution = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact reservation released after successful terminal binding.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact sabotage completion admitted at the ordinary turn boundary.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <summary>Gets the exact ordinary turn activity observed at successful binding.</summary>
    public CharacterTurnActivity? TurnActivity { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        admission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        ordinaryAdmission = GetNode<CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator>(
            "../CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator");
        resolution = GetNode<CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator>(
            "../CharacterSabotageOrdinaryTurnLifecycleResolutionCoordinator");
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

    /// <summary>Binds the exact reserved successor, then releases only its shared-slot reservation.</summary>
    public bool TryBindAndRelease(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? expectedTurnActivity)
    {
        if (Reservation is not null
            || expectedReservation is null
            || expectedAdmission is null
            || expectedTurnActivity is null
            || !ReferenceEquals(turns.SabotageReservation, expectedReservation)
            || !ReferenceEquals(ordinaryAdmission.Reservation, expectedReservation)
            || !ReferenceEquals(ordinaryAdmission.Admission, expectedAdmission)
            || !ReferenceEquals(resolution.Reservation, expectedReservation)
            || !ReferenceEquals(resolution.Completion, expectedAdmission)
            || !ReferenceEquals(resolution.ResolvedTurn, expectedTurnActivity)
            || !ReferenceEquals(
                expectedReservation.Opportunity.Actor,
                expectedAdmission.Actor)
            || !ReferenceEquals(
                expectedTurnActivity.Request.RespondingCharacterId,
                expectedAdmission.Actor.Identity.Id)
            || !ReferenceEquals(expectedTurnActivity.State, CharacterTurnActivityState.Resolved)
            || expectedTurnActivity.Response is not null)
        {
            return false;
        }

        if (!TryBind(expectedAdmission, expectedTurnActivity)
            || !turns.TryReleaseSabotageTurn(expectedReservation, expectedTurnActivity)
            || turns.SabotageReservation is not null)
        {
            throw new InvalidOperationException(
                "A preflighted reserved terminal turn must bind and release exactly once.");
        }

        Reservation = expectedReservation;
        return true;
    }
}
