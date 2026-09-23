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

    /// <summary>Gets the exact generic author request whose Influence recovery was composed.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact Influence-recovery request composed by the existing owner.</summary>
    public CharacterTargetSabotageInfluenceRecoveryConsiderationRequest? Request
    {
        get;
        private set;
    }

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
            || eligibility.EligibilityCount != 1
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
            || !ReferenceEquals(influenceState.Relationship, outcome.Relationship)
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
        if (!ReferenceEquals(composed.Actor, candidate.Occurrence.Target)
            || !ReferenceEquals(composed.Relationship, outcome.Relationship)
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
}
