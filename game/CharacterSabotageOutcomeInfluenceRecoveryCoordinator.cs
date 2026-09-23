using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one exact eligible generic Influence recovery request.</summary>
public partial class CharacterSabotageOutcomeInfluenceRecoveryCoordinator : Node
{
    private CharacterSabotageActionAuthorBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;
    private CharacterTargetSabotageInfluenceController influenceState = null!;
    private CharacterTargetSabotageInfluenceResolutionCoordinator influenceResults = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateController recoveryCandidates = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateRequestController recoveryRequests =
        null!;
    private CharacterTargetSabotageInfluenceRecoveryController recoveryState = null!;
    private CharacterTargetSabotageInfluenceRecoveryTaskController recoveryTask = null!;
    private CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator recoveryResults = null!;
    private CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController
        recoveryFlow = null!;

    /// <summary>Gets the exact generic author request whose Influence recovery was composed.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact Influence-recovery request composed by the existing owner.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request
    {
        get;
        private set;
    }

    /// <summary>Gets the exact composed Influence-recovery request explicitly started once.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? StartedRequest
    {
        get;
        private set;
    }

    /// <summary>Gets the exact completed recovery result acknowledged by this generic route.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationResult? CompletedResult
    {
        get;
        private set;
    }

    /// <summary>Gets the exact applied Influence recovery retained as the terminal output.</summary>
    public CharacterTargetSabotageInfluenceRecoveryResult? RecoveredResult { get; private set; }

    /// <summary>Gets the exact fresh relationship retained with the terminal recovery.</summary>
    public CharacterRelationshipMemory? RecoveredRelationship { get; private set; }

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
        influenceState = GetNode<CharacterTargetSabotageInfluenceController>(
            "../CharacterTargetSabotageInfluenceController");
        influenceResults = GetNode<CharacterTargetSabotageInfluenceResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceResolutionCoordinator");
        recoveryCandidates = GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateController>(
            "../CharacterTargetSabotageInfluenceRecoveryCandidateController");
        recoveryRequests =
            GetNode<CharacterTargetSabotageInfluenceRecoveryCandidateRequestController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateRequestController");
        recoveryState = GetNode<CharacterTargetSabotageInfluenceRecoveryController>(
            "../CharacterTargetSabotageInfluenceRecoveryController");
        recoveryTask = GetNode<CharacterTargetSabotageInfluenceRecoveryTaskController>(
            "../CharacterTargetSabotageInfluenceRecoveryTaskController");
        recoveryResults = GetNode<CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator>(
            "../CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator");
        recoveryFlow = GetNode<
            CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController>(
                "../CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController");
    }

    /// <summary>Composes only the exact eligible Influence branch into its recovery request once.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageInfluenceRecoveryCandidate? expectedCandidate)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || AuthorRequest is not null
            || Request is not null
            || !IsCurrentOutcomeRoute(expectedAuthorRequest, expectedCandidate)
            || recoveryRequests.Request is not null
            || recoveryState.Relationship is not null
            || recoveryState.LatestAppliedResult is not null
            || recoveryTask.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null
            || !recoveryRequests.TryCreate(expectedCandidate))
        {
            return false;
        }

        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest composed =
            recoveryRequests.Request
            ?? throw new InvalidOperationException(
                "Successful Influence-recovery composition must install an exact request.");
        if (!ReferenceEquals(
                composed.Actor,
                expectedCandidate.InfluenceResult.Candidate.Occurrence.Target)
            || !ReferenceEquals(
                composed.Relationship,
                expectedCandidate.InfluenceResult.Relationship)
            || composed.Candidates.Count != 1
            || !ReferenceEquals(composed.Candidates[0], expectedCandidate))
        {
            throw new InvalidOperationException(
                "Influence-recovery composition must retain its exact authoritative graph.");
        }

        AuthorRequest = expectedAuthorRequest;
        Request = composed;
        return true;
    }

    /// <summary>Initializes and starts only this route's exact composed recovery request once.</summary>
    public bool TryStart(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageInfluenceRecoveryCandidate? expectedCandidate)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || !ReferenceEquals(AuthorRequest, expectedAuthorRequest)
            || Request is not { } request
            || !ReferenceEquals(recoveryRequests.Request, request)
            || StartedRequest is not null
            || !IsCurrentOutcomeRoute(expectedAuthorRequest, expectedCandidate)
            || !ReferenceEquals(request.Actor, expectedCandidate.InfluenceResult.Candidate.Occurrence.Target)
            || !ReferenceEquals(request.Relationship, expectedCandidate.InfluenceResult.Relationship)
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], expectedCandidate)
            || recoveryState.Relationship is not null
            || recoveryState.LatestAppliedResult is not null
            || recoveryTask.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Idle
            || recoveryTask.Result is not null)
        {
            return false;
        }

        if (!recoveryState.TryInitialize(request.Relationship)
            || !recoveryFlow.TryStart(request))
        {
            throw new InvalidOperationException(
                "A preflighted generic Influence recovery must start atomically on the scene thread.");
        }

        StartedRequest = request;
        return true;
    }

    /// <summary>Retains and acknowledges only this route's exact completed recovery once.</summary>
    public bool TryComplete(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedAuthorRequest is null
            || expectedRequest is null
            || CompletedResult is not null
            || RecoveredResult is not null
            || RecoveredRelationship is not null
            || !ReferenceEquals(AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Influence)
            || !ReferenceEquals(Request, expectedRequest)
            || !ReferenceEquals(StartedRequest, expectedRequest)
            || !ReferenceEquals(recoveryRequests.Request, expectedRequest)
            || eligibility.EligibilityCount != 1
            || eligibility.InfluenceCandidate is not { } candidate
            || !IsCurrentOutcomeRoute(expectedAuthorRequest, candidate)
            || expectedRequest.Candidates.Count != 1
            || !ReferenceEquals(expectedRequest.Candidates[0], candidate)
            || recoveryTask.Status != CharacterTargetSabotageInfluenceRecoveryTaskStatus.Completed
            || recoveryTask.Result is not { } completed
            || !ReferenceEquals(completed.Request, expectedRequest)
            || !ReferenceEquals(completed.SelectedCandidate, candidate)
            || recoveryState.LatestAppliedResult is not { } recovered
            || !ReferenceEquals(recovered.Candidate, candidate)
            || !ReferenceEquals(recoveryState.Relationship, recovered.Relationship)
            || !IsExactRecoveryAppend(expectedRequest, recovered, candidate)
            || !recoveryResults.TryAcknowledgeCompletedResult())
        {
            return false;
        }

        CompletedResult = completed;
        RecoveredResult = recovered;
        RecoveredRelationship = recovered.Relationship;
        return true;
    }

    private bool IsCurrentOutcomeRoute(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterTargetSabotageInfluenceRecoveryCandidate expectedCandidate)
    {
        if (eligibility.EligibilityCount != 1
            || !ReferenceEquals(eligibility.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(eligibility.InfluenceCandidate, expectedCandidate)
            || eligibility.EvidenceView is not null
            || !ReferenceEquals(
                binding.AcknowledgedResult?.SelectedActionKind,
                CharacterSabotageActionKind.Influence)
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || binding.SelectedCandidateBinding
                is not CharacterSabotageActionCandidateBinding<
                    CharacterTargetSabotageInfluenceCandidate> selected
            || !ReferenceEquals(selected.Request, expectedAuthorRequest)
            || results.InfluenceResult is not { SelectedCandidate: { } candidate } result
            || !ReferenceEquals(selected.Candidate, candidate)
            || !ReferenceEquals(results.AcknowledgedAuthorRequest, expectedAuthorRequest)
            || requests.InfluenceRequest is not { } request
            || !ReferenceEquals(result.Request, request)
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], candidate)
            || !ReferenceEquals(influenceResults.AcknowledgedResult, result)
            || outcomes.Influence is not { } outcome
            || !ReferenceEquals(outcomes.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(outcome.Candidate, candidate)
            || !ReferenceEquals(influenceResults.AcknowledgedAppliedResult, outcome)
            || !ReferenceEquals(expectedCandidate.InfluenceResult, outcome)
            || !ReferenceEquals(recoveryCandidates.Candidate, expectedCandidate)
            || !ReferenceEquals(influenceState.LatestAppliedResult, outcome)
            || !ReferenceEquals(influenceState.Relationship, outcome.Relationship))
        {
            return false;
        }

        return true;
    }

    private static bool IsExactRecoveryAppend(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        CharacterTargetSabotageInfluenceRecoveryResult recovered,
        CharacterTargetSabotageInfluenceRecoveryCandidate candidate)
    {
        CharacterRelationshipMemory before = request.Relationship;
        CharacterRelationshipMemory after = recovered.Relationship;
        if (ReferenceEquals(after, before)
            || !ReferenceEquals(after.CharacterId, before.CharacterId)
            || !ReferenceEquals(after.RelatedCharacterId, before.RelatedCharacterId)
            || after.Dimensions.Count != before.Dimensions.Count
            || after.Reasons.Count != before.Reasons.Count + 1
            || !ReferenceEquals(after.Reasons[^1], candidate.RecoveryChange.Reason)
            || !CharacterTargetSabotageInfluenceRecoveryPolicy.CanRecover(before, candidate))
        {
            return false;
        }

        for (int index = 0; index < before.Reasons.Count; index++)
        {
            if (!ReferenceEquals(after.Reasons[index], before.Reasons[index]))
            {
                return false;
            }
        }

        for (int index = 0; index < before.Dimensions.Count; index++)
        {
            CharacterRelationshipDimensionValue prior = before.Dimensions[index];
            CharacterRelationshipDimensionValue current = after.Dimensions[index];
            int expectedValue = ReferenceEquals(
                prior.Dimension,
                candidate.RecoveryChange.Reason.Dimension)
                ? prior.Value + candidate.RecoveryChange.Delta
                : prior.Value;
            if (!ReferenceEquals(current.Dimension, prior.Dimension)
                || current.Value != expectedValue)
            {
                return false;
            }
        }

        return true;
    }
}
