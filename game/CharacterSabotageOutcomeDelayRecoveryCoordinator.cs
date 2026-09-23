using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes and starts one exact eligible generic Delay recovery request.</summary>
public partial class CharacterSabotageOutcomeDelayRecoveryCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterTargetSabotageDelayController delayState = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator delayResults = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController recoveryCandidates = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateRequestController recoveryRequests = null!;
    private CharacterTargetSabotageDelayRecoveryController recoveryState = null!;
    private CharacterTargetSabotageDelayRecoveryTaskController recoveryTask = null!;
    private CharacterTargetSabotageDelayRecoveryResolutionCoordinator recoveryResults = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController recoveryFlow =
        null!;

    /// <summary>Gets the exact generic author request whose Delay recovery was composed.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact Delay-recovery request composed by the existing owner.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <summary>Gets the exact composed Delay-recovery request explicitly started once.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? StartedRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact completed recovery result acknowledged by this generic route.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationResult? CompletedResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact applied Delay recovery retained as this route's terminal output.</summary>
    public CharacterTargetSabotageDelayRecoveryResult? RecoveredResult { get; private set; }

    /// <summary>Gets the exact fresh event log retained with the terminal Delay recovery.</summary>
    public WorldEventLogSnapshot? RecoveredEventLog { get; private set; }

    /// <summary>Gets the exact fresh world time retained with the terminal Delay recovery.</summary>
    public WorldTimeSnapshot? RecoveredWorldTime { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageActionAuthorBindingCoordinator>(
            "../CharacterSabotageActionAuthorBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        outcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        delayResults = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        recoveryCandidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        recoveryRequests =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateRequestController");
        recoveryState = GetNode<CharacterTargetSabotageDelayRecoveryController>(
            "../CharacterTargetSabotageDelayRecoveryController");
        recoveryTask = GetNode<CharacterTargetSabotageDelayRecoveryTaskController>(
            "../CharacterTargetSabotageDelayRecoveryTaskController");
        recoveryResults = GetNode<CharacterTargetSabotageDelayRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageDelayRecoveryResolutionCoordinator");
        recoveryFlow =
            GetNode<CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController>(
                "../CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController");
    }

    /// <summary>Composes only the exact eligible Delay branch into its recovery request once.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryCandidate? expectedCandidate)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || AuthorRequest is not null
            || Request is not null
            || eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.DelayCandidate, expectedCandidate)
            || eligibility.EvidenceView is not null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageDelayCandidate> selected
            || !ReferenceEquals(selected.Request, expectedAuthorRequest)
            || results.DelayResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || requests.DelayRequest is not { } request
            || !ReferenceEquals(result.Request, request)
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(delayResults.AcknowledgedResult, result)
            || outcomes.Delay is not { } outcome
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(outcome.Candidate, candidate)
            || !ReferenceEquals(delayResults.AcknowledgedAppliedResult, outcome)
            || !ReferenceEquals(expectedCandidate.DelayResult, outcome)
            || !ReferenceEquals(recoveryCandidates.Candidate, expectedCandidate)
            || !ReferenceEquals(delayState.LatestAppliedResult, outcome)
            || !ReferenceEquals(delayState.EventLog, outcome.EventLog)
            || !ReferenceEquals(delayState.WorldTime, outcome.WorldTime)
            || !recoveryRequests.TryCreate(request.WorldEvents, expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageDelayRecoveryConsiderationRequest composed =
            recoveryRequests.Request
            ?? throw new InvalidOperationException(
                "Successful Delay-recovery composition must install an exact request.");
        if (!ReferenceEquals(composed.Actor, candidate.Occurrence.Target)
            || !ReferenceEquals(composed.WorldEvents, request.WorldEvents)
            || !ReferenceEquals(composed.EventLog, outcome.EventLog)
            || !ReferenceEquals(composed.WorldTime, outcome.WorldTime)
            || composed.Candidates.Count != 1
            || !ReferenceEquals(composed.Candidates[0], expectedCandidate))
        {
            throw new InvalidOperationException(
                "Delay-recovery composition must retain its exact authoritative graph.");
        }

        AuthorRequest = expectedAuthorRequest;
        Request = composed;
        return true;
    }

    /// <summary>Starts only the exact composed Delay-recovery request once.</summary>
    public bool TryStart(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedAuthorRequest is null
            || expectedRequest is null
            || !ReferenceEquals(AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(Request, expectedRequest)
            || !ReferenceEquals(recoveryRequests.Request, expectedRequest)
            || StartedRequest is not null
            || expectedRequest.Candidates.Count != 1
            || eligibility.DelayCandidate is not { } candidate
            || !ReferenceEquals(expectedRequest.Candidates[0], candidate)
            || !ReferenceEquals(recoveryCandidates.Candidate, candidate)
            || !ReferenceEquals(delayState.LatestAppliedResult, candidate.DelayResult)
            || !ReferenceEquals(delayState.EventLog, expectedRequest.EventLog)
            || !ReferenceEquals(delayState.WorldTime, expectedRequest.WorldTime)
            || recoveryState.EventLog is not null
            || recoveryState.WorldTime is not null
            || recoveryState.LatestAppliedResult is not null
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null
            || !CharacterTargetSabotageDelayRecoveryPolicy.CanRecover(
                expectedRequest.WorldEvents,
                expectedRequest.EventLog,
                expectedRequest.WorldTime,
                candidate)
            || !CharacterTargetSabotageDelayRecoveryDecisionPolicy.CanAccept(
                expectedRequest,
                new CharacterTargetSabotageDelayRecoveryDecision(
                    expectedRequest.Actor.Identity.Id,
                    null)))
        {
            return false;
        }

        if (!recoveryState.TryInitialize(expectedRequest.EventLog, expectedRequest.WorldTime)
            || !recoveryFlow.TryStart(expectedRequest))
        {
            throw new InvalidOperationException(
                "A preflighted composed Delay-recovery request must start atomically.");
        }

        StartedRequest = expectedRequest;
        return true;
    }

    /// <summary>Retains and acknowledges only the exact completed recovery for this Delay route.</summary>
    public bool TryComplete(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedAuthorRequest is null
            || expectedRequest is null
            || CompletedResult is not null
            || RecoveredResult is not null
            || RecoveredEventLog is not null
            || RecoveredWorldTime is not null
            || !ReferenceEquals(AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(Request, expectedRequest)
            || !ReferenceEquals(StartedRequest, expectedRequest)
            || !ReferenceEquals(recoveryRequests.Request, expectedRequest)
            || eligibility.EligibilityCount != 1
            || eligibility.DelayCandidate is not { } candidate
            || expectedRequest.Candidates.Count != 1
            || !ReferenceEquals(expectedRequest.Candidates[0], candidate)
            || !ReferenceEquals(recoveryCandidates.Candidate, candidate)
            || !ReferenceEquals(delayState.LatestAppliedResult, candidate.DelayResult)
            || !ReferenceEquals(delayState.EventLog, expectedRequest.EventLog)
            || !ReferenceEquals(delayState.WorldTime, expectedRequest.WorldTime)
            || recoveryTask.Status != CharacterTargetSabotageDelayRecoveryTaskStatus.Completed
            || recoveryTask.Result is not { } completed
            || !ReferenceEquals(completed.Request, expectedRequest)
            || !ReferenceEquals(completed.SelectedCandidate, candidate)
            || recoveryState.LatestAppliedResult is not { } recovered
            || !ReferenceEquals(recovered.Candidate, candidate)
            || !ReferenceEquals(recoveryState.EventLog, recovered.EventLog)
            || !ReferenceEquals(recoveryState.WorldTime, recovered.WorldTime)
            || !IsExactRecoveryAppend(expectedRequest, recovered, candidate)
            || !recoveryResults.TryAcknowledgeCompletedResult())
        {
            return false;
        }

        CompletedResult = completed;
        RecoveredResult = recovered;
        RecoveredEventLog = recovered.EventLog;
        RecoveredWorldTime = recovered.WorldTime;
        return true;
    }

    private static bool IsExactRecoveryAppend(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CharacterTargetSabotageDelayRecoveryResult recovered,
        CharacterTargetSabotageDelayRecoveryCandidate candidate)
    {
        if (ReferenceEquals(recovered.EventLog, request.EventLog)
            || ReferenceEquals(recovered.WorldTime, request.WorldTime)
            || recovered.EventLog.Entries.Count != request.EventLog.Entries.Count + 1
            || !ReferenceEquals(recovered.EventLog.Entries[^1], candidate.RecoveryEvent.EventId)
            || !ReferenceEquals(recovered.WorldTime.CurrentEventId, candidate.RecoveryEvent.EventId))
        {
            return false;
        }

        for (int index = 0; index < request.EventLog.Entries.Count; index++)
        {
            if (!ReferenceEquals(
                    recovered.EventLog.Entries[index],
                    request.EventLog.Entries[index]))
            {
                return false;
            }
        }

        return true;
    }
}
