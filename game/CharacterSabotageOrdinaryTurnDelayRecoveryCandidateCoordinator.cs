using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Publishes one exact ordinary-turn Delay recovery candidate.</summary>
public partial class CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator : Node
{
    private TemporaryCharacterSabotageOrdinaryTurnAuthoringSource source = null!;
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnSpecificResultCoordinator result = null!;
    private CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator outcome = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator resolution = null!;
    private CharacterTargetSabotageDelayController state = null!;
    private CharacterTargetSabotageDelayRecoveryCandidateController candidates = null!;
    private CharacterSabotageOutcomeEligibilityCoordinator eligibility = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained for recovery.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact generic author request retained for recovery.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact applied Delay result retained for recovery.</summary>
    public CharacterTargetSabotageDelayResult? Delay { get; private set; }

    /// <summary>Gets the exact scene-owned recovery event retained here.</summary>
    public WorldEvent? RecoveryEvent { get; private set; }

    /// <summary>Gets the exact published Delay recovery candidate.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? Candidate { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        source = GetNode<TemporaryCharacterSabotageOrdinaryTurnAuthoringSource>(
            "../TemporaryCharacterSabotageOrdinaryTurnAuthoringSource");
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        result = GetNode<CharacterSabotageOrdinaryTurnSpecificResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificResultCoordinator");
        outcome = GetNode<CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        outcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        resolution = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
        candidates = GetNode<CharacterTargetSabotageDelayRecoveryCandidateController>(
            "../CharacterTargetSabotageDelayRecoveryCandidateController");
        eligibility = GetNode<CharacterSabotageOutcomeEligibilityCoordinator>(
            "../CharacterSabotageOutcomeEligibilityCoordinator");
    }

    /// <summary>Publishes once from only the exact retained ordinary-turn Delay graph.</summary>
    public bool TryPublish(
        CharacterTargetSabotageDelayResult? expectedDelay,
        WorldEvent? expectedRecoveryEvent)
    {
        if (AuthorRequest is not null
            || Delay is not null
            || RecoveryEvent is not null
            || Candidate is not null
            || candidates.Candidate is not null
            || eligibility.AuthorRequest is not null
            || eligibility.EligibilityCount != 0
            || !HasRetainedGraph(expectedDelay, expectedRecoveryEvent))
        {
            return false;
        }

        if (!candidates.TryCreate(
                source.DelayRequest.WorldEvents,
                expectedDelay!,
                expectedRecoveryEvent!)
            || candidates.Candidate is not { } candidate
            || !ReferenceEquals(candidate.DelayResult, expectedDelay)
            || !ReferenceEquals(candidate.RecoveryEvent, expectedRecoveryEvent)
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, null)
            || !ReferenceEquals(eligibility.AuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(eligibility.DelayCandidate, candidate)
            || eligibility.EligibilityCount != 1)
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay recovery candidate must publish once.");
        }

        AuthorRequest = binding.SelectedAuthorRequest;
        Delay = expectedDelay;
        RecoveryEvent = expectedRecoveryEvent;
        Candidate = candidate;
        return true;
    }

    /// <summary>Publishes once from only the exact retained reserved Delay graph.</summary>
    public bool TryPublish(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayResult? expectedDelay,
        WorldEvent? expectedRecoveryEvent)
    {
        if (Reservation is not null
            || AuthorRequest is not null
            || Delay is not null
            || RecoveryEvent is not null
            || Candidate is not null
            || candidates.Candidate is not null
            || eligibility.AuthorRequest is not null
            || eligibility.EligibilityCount != 0
            || !HasRetainedGraph(expectedReservation, expectedDelay, expectedRecoveryEvent))
        {
            return false;
        }

        if (!candidates.TryCreate(
                source.DelayRequest.WorldEvents,
                expectedDelay!,
                expectedRecoveryEvent!)
            || candidates.Candidate is not { } candidate
            || !ReferenceEquals(candidate.DelayResult, expectedDelay)
            || !ReferenceEquals(candidate.RecoveryEvent, expectedRecoveryEvent)
            || !eligibility.TryPublish(binding.SelectedAuthorRequest, null)
            || !ReferenceEquals(eligibility.AuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(eligibility.DelayCandidate, candidate)
            || eligibility.EligibilityCount != 1)
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay recovery candidate must publish once.");
        }

        Reservation = expectedReservation;
        AuthorRequest = binding.SelectedAuthorRequest;
        Delay = expectedDelay;
        RecoveryEvent = expectedRecoveryEvent;
        Candidate = candidate;
        return true;
    }

    private bool HasRetainedGraph(
        CharacterTargetSabotageDelayResult? expectedDelay,
        WorldEvent? expectedRecoveryEvent)
    {
        CharacterSabotageActionConsiderationResult? genericResult = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedDelay is not null
            && expectedRecoveryEvent is not null
            && candidateBinding is not null
            && ReferenceEquals(source.RecoveryEvent, expectedRecoveryEvent)
            && source.DelayRequest.WorldEvents.Entries.Any(
                worldEvent => ReferenceEquals(worldEvent, expectedRecoveryEvent))
            && binding.IsRetainedAcknowledgedBinding(
                genericResult,
                authorRequest,
                candidateBinding)
            && ReferenceEquals(result.Result, results.DelayResult)
            && ReferenceEquals(result.AppliedResult, expectedDelay)
            && ReferenceEquals(result.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(result.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(resolution.AcknowledgedResult, result.Result)
            && ReferenceEquals(resolution.AcknowledgedAppliedResult, expectedDelay)
            && ReferenceEquals(outcome.AuthorRequest, authorRequest)
            && ReferenceEquals(outcome.Result, result.Result)
            && ReferenceEquals(outcome.Delay, expectedDelay)
            && ReferenceEquals(outcome.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(outcome.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(outcomes.AuthorRequest, authorRequest)
            && ReferenceEquals(outcomes.Delay, expectedDelay)
            && outcomes.OutcomeCount == 1
            && ReferenceEquals(state.LatestAppliedResult, expectedDelay)
            && ReferenceEquals(state.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(state.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(expectedDelay.Candidate, candidateBinding.Candidate);
    }

    private bool HasRetainedGraph(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayResult? expectedDelay,
        WorldEvent? expectedRecoveryEvent)
    {
        CharacterSabotageActionConsiderationResult? genericResult = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedReservation is not null
            && expectedDelay is not null
            && expectedRecoveryEvent is not null
            && candidateBinding is not null
            && ReferenceEquals(source.RecoveryEvent, expectedRecoveryEvent)
            && source.DelayRequest.WorldEvents.Entries.Any(
                worldEvent => ReferenceEquals(worldEvent, expectedRecoveryEvent))
            && binding.IsRetainedAcknowledgedBinding(
                expectedReservation,
                genericResult,
                authorRequest,
                candidateBinding)
            && ReferenceEquals(result.Reservation, expectedReservation)
            && ReferenceEquals(result.Result, results.DelayResult)
            && ReferenceEquals(result.AppliedResult, expectedDelay)
            && ReferenceEquals(result.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(result.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(resolution.AcknowledgedResult, result.Result)
            && ReferenceEquals(resolution.AcknowledgedAppliedResult, expectedDelay)
            && ReferenceEquals(outcome.Reservation, expectedReservation)
            && ReferenceEquals(outcome.AuthorRequest, authorRequest)
            && ReferenceEquals(outcome.Result, result.Result)
            && ReferenceEquals(outcome.Delay, expectedDelay)
            && ReferenceEquals(outcome.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(outcome.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(outcomes.AuthorRequest, authorRequest)
            && ReferenceEquals(outcomes.Delay, expectedDelay)
            && outcomes.OutcomeCount == 1
            && ReferenceEquals(state.LatestAppliedResult, expectedDelay)
            && ReferenceEquals(state.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(state.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(expectedDelay.Candidate, candidateBinding.Candidate);
    }
}
