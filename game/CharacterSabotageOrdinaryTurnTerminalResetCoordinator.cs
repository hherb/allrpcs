using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Resets one exact retained ordinary-turn Delay terminal route.</summary>
public partial class CharacterSabotageOrdinaryTurnTerminalResetCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnTerminalRouteCoordinator ordinaryTerminalRoute = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained across terminal reset.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact generic author request whose terminal route was reset.</summary>
    public CharacterSabotageActionConsiderationRequest? ResetAuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical action kind whose terminal route was reset.</summary>
    public CharacterSabotageActionKind? ResetActionKind { get; private set; }

    /// <summary>Gets the exact applied Delay recovery retained across terminal reset.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? DelayRecovery { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ordinaryTerminalRoute = GetNode<CharacterSabotageOrdinaryTurnTerminalRouteCoordinator>(
            "../CharacterSabotageOrdinaryTurnTerminalRouteCoordinator");
        recoveryResult = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Resets only the exact retained ordinary-turn Delay terminal route once.</summary>
    public bool TryReset(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime)
    {
        if (ResetAuthorRequest is not null
            || ResetActionKind is not null
            || DelayRecovery is not null
            || expectedAuthorRequest is null
            || expectedDelayRecovery is null
            || expectedEventLog is null
            || expectedWorldTime is null
            || !ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(ordinaryTerminalRoute.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(ordinaryTerminalRoute.ActionKind, expectedActionKind)
            || !ReferenceEquals(ordinaryTerminalRoute.DelayRecovery, expectedDelayRecovery)
            || !ReferenceEquals(recoveryResult.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(recoveryResult.AppliedResult, expectedDelayRecovery)
            || !ReferenceEquals(recoveryResult.EventLog, expectedEventLog)
            || !ReferenceEquals(recoveryResult.WorldTime, expectedWorldTime)
            || !ReferenceEquals(recoveryState.LatestAppliedResult, expectedDelayRecovery)
            || !ReferenceEquals(recoveryState.EventLog, expectedEventLog)
            || !ReferenceEquals(recoveryState.WorldTime, expectedWorldTime)
            || !ReferenceEquals(terminalRoute.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalRoute.ActionKind, expectedActionKind)
            || !ReferenceEquals(terminalRoute.DelayRecovery, expectedDelayRecovery)
            || terminalRoute.TerminalOutputCount != 1)
        {
            return false;
        }

        if (!terminalRoute.TryReset(expectedAuthorRequest, expectedActionKind)
            || terminalRoute.AuthorRequest is not null
            || terminalRoute.ActionKind is not null
            || terminalRoute.TerminalOutputCount != 0
            || !ReferenceEquals(terminalRoute.ResetAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalRoute.ResetActionKind, expectedActionKind))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay terminal route must reset exactly once.");
        }

        ResetAuthorRequest = expectedAuthorRequest;
        ResetActionKind = expectedActionKind;
        DelayRecovery = expectedDelayRecovery;
        return true;
    }

    /// <summary>Resets only the exact retained reserved Delay terminal route once.</summary>
    public bool TryReset(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionKind? expectedActionKind,
        CharacterTargetSabotageDelayRecoveryResult? expectedDelayRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime)
    {
        if (Reservation is not null
            || expectedReservation is null
            || !ReferenceEquals(turns.SabotageReservation, expectedReservation)
            || !ReferenceEquals(ordinaryTerminalRoute.Reservation, expectedReservation)
            || !ReferenceEquals(recoveryResult.Reservation, expectedReservation))
        {
            return false;
        }

        if (!TryReset(
                expectedAuthorRequest,
                expectedActionKind,
                expectedDelayRecovery,
                expectedEventLog,
                expectedWorldTime))
        {
            return false;
        }

        Reservation = expectedReservation;
        return true;
    }
}
