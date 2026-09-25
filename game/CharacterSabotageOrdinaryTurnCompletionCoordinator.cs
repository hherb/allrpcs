using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Exposes one exact completed ordinary-turn sabotage token after terminal reset.</summary>
public partial class CharacterSabotageOrdinaryTurnCompletionCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnTerminalResetCoordinator terminalReset = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterSabotageTurnCompletionCoordinator completion = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained at completion.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact completed-turn token exposed through the existing owner.</summary>
    public CharacterSabotageTurnCompletion? Completion { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        terminalReset = GetNode<CharacterSabotageOrdinaryTurnTerminalResetCoordinator>(
            "../CharacterSabotageOrdinaryTurnTerminalResetCoordinator");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        completion = GetNode<CharacterSabotageTurnCompletionCoordinator>(
            "../CharacterSabotageTurnCompletionCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Exposes completion only for the exact reset ordinary-turn Delay route.</summary>
    public bool TryComplete(
        Character? expectedActor,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery)
    {
        if (Completion is not null
            || completion.Completion is not null
            || expectedActor is null
            || expectedAuthorRequest is null
            || expectedDelayRecovery is null
            || !ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(expectedAuthorRequest.Actor, expectedActor)
            || !ReferenceEquals(terminalReset.ResetAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalReset.ResetActionKind, expectedActionKind)
            || !ReferenceEquals(terminalReset.DelayRecovery, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.LatestAppliedResult, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.EventLog, expectedDelayRecovery.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, expectedDelayRecovery.WorldTime))
        {
            return false;
        }

        if (!completion.TryComplete(expectedActor, expectedAuthorRequest, expectedActionKind)
            || completion.Completion is not { } completed
            || !ReferenceEquals(completed.Actor, expectedActor)
            || !ReferenceEquals(completed.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(completed.ActionKind, expectedActionKind))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay route must expose one exact completion.");
        }

        Completion = completed;
        return true;
    }

    /// <summary>Exposes completion only for the exact reset reserved Delay route.</summary>
    public bool TryComplete(
        CharacterSabotageTurnReservation? expectedReservation,
        Character? expectedActor,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery)
    {
        if (Reservation is not null
            || expectedReservation is null
            || !ReferenceEquals(turns.SabotageReservation, expectedReservation)
            || !ReferenceEquals(terminalReset.Reservation, expectedReservation)
            || !ReferenceEquals(expectedReservation.Opportunity.Actor, expectedActor))
        {
            return false;
        }

        if (!TryComplete(
                expectedActor,
                expectedAuthorRequest,
                expectedActionKind,
                expectedDelayRecovery))
        {
            return false;
        }

        Reservation = expectedReservation;
        return true;
    }
}
