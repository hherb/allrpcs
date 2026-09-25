using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Retains one exact completed ordinary-turn Delay route at terminal state.</summary>
public partial class CharacterSabotageOrdinaryTurnTerminalRouteCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnSpecificResultCoordinator specificResult = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator recoveryRequest = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator recoveryResult = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterSabotageTerminalRouteCoordinator terminalRoute = null!;
    private CharacterTurnActivityController turns = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained at terminal state.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact generic author request retained at terminal state.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact canonical action kind retained at terminal state.</summary>
    public CharacterSabotageActionKind? ActionKind { get; private set; }

    /// <summary>Gets the exact applied Delay recovery retained at terminal state.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? DelayRecovery { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        specificResult = GetNode<CharacterSabotageOrdinaryTurnSpecificResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificResultCoordinator");
        recoveryRequest = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        recoveryResult = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryResultCoordinator");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        terminalRoute = GetNode<CharacterSabotageTerminalRouteCoordinator>(
            "../CharacterSabotageTerminalRouteCoordinator");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
    }

    /// <summary>Retains only the exact current completed ordinary-turn Delay graph once.</summary>
    public bool TryRetain(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRecoveryRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationResult? expectedCompletedResult,
        CharacterTargetSabotageDelayRecoveryResult? expectedAppliedRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime,
        CharacterSabotageActionKind? expectedActionKind)
    {
        if (AuthorRequest is not null
            || ActionKind is not null
            || DelayRecovery is not null
            || terminalRoute.AuthorRequest is not null
            || terminalRoute.ActionKind is not null
            || terminalRoute.TerminalOutputCount != 0
            || !HasCurrentGraph(
                expectedAuthorRequest,
                expectedRecoveryRequest,
                expectedCompletedResult,
                expectedAppliedRecovery,
                expectedEventLog,
                expectedWorldTime,
                expectedActionKind))
        {
            return false;
        }

        if (!terminalRoute.TryRetain(expectedAuthorRequest, expectedActionKind)
            || !ReferenceEquals(terminalRoute.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalRoute.ActionKind, expectedActionKind)
            || !ReferenceEquals(terminalRoute.DelayRecovery, expectedAppliedRecovery)
            || terminalRoute.TerminalOutputCount != 1)
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay route must retain exactly once.");
        }

        AuthorRequest = expectedAuthorRequest;
        ActionKind = expectedActionKind;
        DelayRecovery = expectedAppliedRecovery;
        return true;
    }

    /// <summary>Retains only the exact current reserved Delay graph once.</summary>
    public bool TryRetain(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRecoveryRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationResult? expectedCompletedResult,
        CharacterTargetSabotageDelayRecoveryResult? expectedAppliedRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime,
        CharacterSabotageActionKind? expectedActionKind)
    {
        if (Reservation is not null
            || AuthorRequest is not null
            || ActionKind is not null
            || DelayRecovery is not null
            || terminalRoute.AuthorRequest is not null
            || terminalRoute.ActionKind is not null
            || terminalRoute.TerminalOutputCount != 0
            || !HasCurrentReservedGraph(
                expectedReservation,
                expectedAuthorRequest,
                expectedRecoveryRequest,
                expectedCompletedResult,
                expectedAppliedRecovery,
                expectedEventLog,
                expectedWorldTime,
                expectedActionKind))
        {
            return false;
        }

        if (!terminalRoute.TryRetain(expectedAuthorRequest, expectedActionKind)
            || !ReferenceEquals(terminalRoute.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(terminalRoute.ActionKind, expectedActionKind)
            || !ReferenceEquals(terminalRoute.DelayRecovery, expectedAppliedRecovery)
            || terminalRoute.TerminalOutputCount != 1)
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay route must retain exactly once.");
        }

        Reservation = expectedReservation;
        AuthorRequest = expectedAuthorRequest;
        ActionKind = expectedActionKind;
        DelayRecovery = expectedAppliedRecovery;
        return true;
    }

    private bool HasCurrentReservedGraph(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRecoveryRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationResult? expectedCompletedResult,
        CharacterTargetSabotageDelayRecoveryResult? expectedAppliedRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime,
        CharacterSabotageActionKind? expectedActionKind)
    {
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            selectedBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedReservation is not null
            && selectedBinding is not null
            && ReferenceEquals(turns.SabotageReservation, expectedReservation)
            && binding.IsRetainedAcknowledgedBinding(
                expectedReservation,
                binding.AcknowledgedResult,
                expectedAuthorRequest,
                selectedBinding)
            && ReferenceEquals(specificResult.Reservation, expectedReservation)
            && ReferenceEquals(recoveryRequest.Reservation, expectedReservation)
            && ReferenceEquals(recoveryResult.Reservation, expectedReservation)
            && HasCurrentGraphCore(
                expectedAuthorRequest,
                expectedRecoveryRequest,
                expectedCompletedResult,
                expectedAppliedRecovery,
                expectedEventLog,
                expectedWorldTime,
                expectedActionKind,
                selectedBinding,
                true);
    }

    private bool HasCurrentGraph(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRecoveryRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationResult? expectedCompletedResult,
        CharacterTargetSabotageDelayRecoveryResult? expectedAppliedRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime,
        CharacterSabotageActionKind? expectedActionKind)
    {
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            selectedBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        bool retainedBinding = selectedBinding is not null
            && binding.IsRetainedAcknowledgedBinding(
                binding.AcknowledgedResult,
                expectedAuthorRequest,
                selectedBinding);
        return HasCurrentGraphCore(
            expectedAuthorRequest,
            expectedRecoveryRequest,
            expectedCompletedResult,
            expectedAppliedRecovery,
            expectedEventLog,
            expectedWorldTime,
            expectedActionKind,
            selectedBinding,
            retainedBinding);
    }

    private bool HasCurrentGraphCore(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRecoveryRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationResult? expectedCompletedResult,
        CharacterTargetSabotageDelayRecoveryResult? expectedAppliedRecovery,
        WorldEventLogSnapshot? expectedEventLog,
        WorldTimeSnapshot? expectedWorldTime,
        CharacterSabotageActionKind? expectedActionKind,
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            selectedBinding,
        bool retainedBinding)
    {
        return expectedAuthorRequest is not null
            && expectedRecoveryRequest is not null
            && expectedCompletedResult is not null
            && expectedAppliedRecovery is not null
            && expectedEventLog is not null
            && expectedWorldTime is not null
            && ReferenceEquals(expectedActionKind, CharacterSabotageActionKind.Delay)
            && selectedBinding is not null
            && retainedBinding
            && ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                expectedActionKind)
            && ReferenceEquals(specificResult.Result?.SelectedCandidate, selectedBinding.Candidate)
            && ReferenceEquals(recoveryRequest.AuthorRequest, expectedAuthorRequest)
            && ReferenceEquals(recoveryRequest.Request, expectedRecoveryRequest)
            && ReferenceEquals(recoveryResult.AuthorRequest, expectedAuthorRequest)
            && ReferenceEquals(recoveryResult.Request, expectedRecoveryRequest)
            && ReferenceEquals(recoveryResult.Result, expectedCompletedResult)
            && ReferenceEquals(recoveryResult.AppliedResult, expectedAppliedRecovery)
            && ReferenceEquals(recoveryResult.EventLog, expectedEventLog)
            && ReferenceEquals(recoveryResult.WorldTime, expectedWorldTime)
            && ReferenceEquals(expectedCompletedResult.Request, expectedRecoveryRequest)
            && ReferenceEquals(
                expectedCompletedResult.SelectedCandidate,
                recoveryRequest.Candidate)
            && ReferenceEquals(expectedAppliedRecovery.Candidate, recoveryRequest.Candidate)
            && ReferenceEquals(expectedAppliedRecovery.EventLog, expectedEventLog)
            && ReferenceEquals(expectedAppliedRecovery.WorldTime, expectedWorldTime)
            && ReferenceEquals(recoveryState.LatestAppliedResult, expectedAppliedRecovery)
            && ReferenceEquals(recoveryState.EventLog, expectedEventLog)
            && ReferenceEquals(recoveryState.WorldTime, expectedWorldTime);
    }
}
