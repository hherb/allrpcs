using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Admits one exact ordinary-turn sabotage completion to the scheduling gate.</summary>
public partial class CharacterSabotageOrdinaryTurnCompletionAdmissionCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnCompletionCoordinator ordinaryCompletion = null!;
    private CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterSabotageTurnAdmissionCoordinator admission = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained at admission.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact completed-turn token admitted through the existing gate.</summary>
    public CharacterSabotageTurnCompletion? Admission { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ordinaryCompletion = GetNode<CharacterSabotageOrdinaryTurnCompletionCoordinator>(
            "../CharacterSabotageOrdinaryTurnCompletionCoordinator");
        terminalReset = GetNode<CharacterSabotageOrdinaryTurnTerminalResetCoordinator>(
            "../CharacterSabotageOrdinaryTurnTerminalResetCoordinator");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        admission = GetNode<CharacterSabotageTurnAdmissionCoordinator>(
            "../CharacterSabotageTurnAdmissionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Admits only the exact current completed ordinary-turn Delay token once.</summary>
    public bool TryAdmit(
        CharacterSabotageTurnCompletion? expectedCompletion,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery)
    {
        if (Admission is not null
            || admission.Admission is not null
            || expectedCompletion is null
            || expectedDelayRecovery is null
            || !ReferenceEquals(ordinaryCompletion.Completion, expectedCompletion)
            || !ReferenceEquals(
                terminalReset.ResetAuthorRequest,
                expectedCompletion.AuthorRequest)
            || !ReferenceEquals(
                terminalReset.ResetActionKind,
                expectedCompletion.ActionKind)
            || !ReferenceEquals(terminalReset.DelayRecovery, expectedDelayRecovery)
            || !ReferenceEquals(expectedCompletion.ActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(recoveryState.LatestAppliedResult, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.EventLog, expectedDelayRecovery.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, expectedDelayRecovery.WorldTime))
        {
            return false;
        }

        if (!admission.TryAdmit(expectedCompletion)
            || !ReferenceEquals(admission.Admission, expectedCompletion))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay completion must admit exactly once.");
        }

        Admission = expectedCompletion;
        return true;
    }

    /// <summary>Admits only the exact current reserved Delay completion once.</summary>
    public bool TryAdmit(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageTurnCompletion? expectedCompletion,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery)
    {
        if (Reservation is not null
            || expectedReservation is null
            || !ReferenceEquals(turns.SabotageReservation, expectedReservation)
            || !ReferenceEquals(ordinaryCompletion.Reservation, expectedReservation)
            || !ReferenceEquals(expectedReservation.Opportunity.Actor, expectedCompletion?.Actor))
        {
            return false;
        }

        if (!TryAdmit(expectedCompletion, expectedDelayRecovery))
        {
            return false;
        }

        Reservation = expectedReservation;
        return true;
    }
}
