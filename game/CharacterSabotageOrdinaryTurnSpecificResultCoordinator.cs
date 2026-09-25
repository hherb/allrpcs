using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Observes, resolves, and acknowledges one exact ordinary-turn Delay result.</summary>
public partial class CharacterSabotageOrdinaryTurnSpecificResultCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestCoordinator request = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator start = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator resolution = null!;
    private CharacterTargetSabotageDelayController state = null!;
    private CharacterTargetSabotageDelayTaskController task = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained by result acknowledgement.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact typed Delay result acknowledged through the existing seam.</summary>
    public CharacterTargetSabotageDelayConsiderationResult? Result { get; private set; }

    /// <summary>Gets the exact authoritative event log produced by the Delay result.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the exact authoritative world time produced by the Delay result.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <summary>Gets the exact applied Delay result retained by authoritative state.</summary>
    public CharacterTargetSabotageDelayResult? AppliedResult { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        request = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestCoordinator");
        start = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        resolution = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        task = GetNode<CharacterTargetSabotageDelayTaskController>(
            "../CharacterTargetSabotageDelayTaskController");
    }

    /// <summary>Observes, resolves, and acknowledges one exact completed ordinary-turn result.</summary>
    public bool TryObserveAndAcknowledge(
        CharacterTargetSabotageDelayConsiderationResult? expectedResult)
    {
        if (Result is not null
            || EventLog is not null
            || WorldTime is not null
            || AppliedResult is not null
            || results.ResultCount != 0
            || results.AcknowledgedAuthorRequest is not null
            || !HasCurrentCompletedGraph(expectedResult))
        {
            return false;
        }

        if (!results.TryObserveCompletedResult()
            || !ReferenceEquals(results.DelayResult, expectedResult)
            || results.ResultCount != 1
            || !resolution.TryResolveCompletedResult()
            || state.EventLog is not { } eventLog
            || state.WorldTime is not { } worldTime
            || state.LatestAppliedResult is not { } appliedResult
            || !ReferenceEquals(appliedResult.Candidate, expectedResult!.SelectedCandidate)
            || !ReferenceEquals(appliedResult.EventLog, eventLog)
            || !ReferenceEquals(appliedResult.WorldTime, worldTime)
            || !results.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(resolution.AcknowledgedResult, expectedResult)
            || !ReferenceEquals(resolution.AcknowledgedAppliedResult, appliedResult)
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || task.Result is not null)
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay result must resolve and acknowledge once.");
        }

        Result = expectedResult;
        EventLog = eventLog;
        WorldTime = worldTime;
        AppliedResult = appliedResult;
        return true;
    }

    /// <summary>Observes, resolves, and acknowledges one exact completed reserved result.</summary>
    public bool TryObserveAndAcknowledge(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayConsiderationResult? expectedResult)
    {
        if (Reservation is not null
            || Result is not null
            || EventLog is not null
            || WorldTime is not null
            || AppliedResult is not null
            || results.ResultCount != 0
            || results.AcknowledgedAuthorRequest is not null
            || !HasCurrentCompletedGraph(expectedReservation, expectedResult))
        {
            return false;
        }

        if (!results.TryObserveCompletedResult()
            || !ReferenceEquals(results.DelayResult, expectedResult)
            || results.ResultCount != 1
            || !resolution.TryResolveCompletedResult()
            || state.EventLog is not { } eventLog
            || state.WorldTime is not { } worldTime
            || state.LatestAppliedResult is not { } appliedResult
            || !ReferenceEquals(appliedResult.Candidate, expectedResult!.SelectedCandidate)
            || !ReferenceEquals(appliedResult.EventLog, eventLog)
            || !ReferenceEquals(appliedResult.WorldTime, worldTime)
            || !results.TryAcknowledgeCompletedResult()
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(resolution.AcknowledgedResult, expectedResult)
            || !ReferenceEquals(resolution.AcknowledgedAppliedResult, appliedResult)
            || task.Status != CharacterTargetSabotageDelayTaskStatus.Idle
            || task.Result is not null)
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay result must resolve and acknowledge once.");
        }

        Reservation = expectedReservation;
        Result = expectedResult;
        EventLog = eventLog;
        WorldTime = worldTime;
        AppliedResult = appliedResult;
        return true;
    }

    private bool HasCurrentCompletedGraph(
        CharacterTargetSabotageDelayConsiderationResult? expectedResult)
    {
        CharacterSabotageActionConsiderationResult? genericResult = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedResult is not null
            && candidateBinding is not null
            && binding.IsCurrentAcknowledgedBinding(
                genericResult,
                authorRequest,
                candidateBinding)
            && request.IsCurrentRequest(start.Request)
            && start.IsCurrentCompletedResult(expectedResult)
            && ReferenceEquals(expectedResult.Request, start.Request)
            && ReferenceEquals(expectedResult.SelectedCandidate, candidateBinding.Candidate)
            && ReferenceEquals(task.Result, expectedResult);
    }

    private bool HasCurrentCompletedGraph(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayConsiderationResult? expectedResult)
    {
        CharacterSabotageActionConsiderationResult? genericResult = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedReservation is not null
            && expectedResult is not null
            && candidateBinding is not null
            && binding.IsCurrentAcknowledgedBinding(
                expectedReservation,
                genericResult,
                authorRequest,
                candidateBinding)
            && request.IsCurrentRequest(expectedReservation, start.Request)
            && start.IsCurrentCompletedResult(expectedReservation, expectedResult)
            && ReferenceEquals(expectedResult.Request, start.Request)
            && ReferenceEquals(expectedResult.SelectedCandidate, candidateBinding.Candidate)
            && ReferenceEquals(task.Result, expectedResult);
    }
}
