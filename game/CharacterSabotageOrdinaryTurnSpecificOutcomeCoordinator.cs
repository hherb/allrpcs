using Allrpcs.Domain.Characters;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Binds one acknowledged ordinary-turn Delay result to its exact outcome.</summary>
public partial class CharacterSabotageOrdinaryTurnSpecificOutcomeCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnSpecificRequestCoordinator request = null!;
    private CharacterSabotageOrdinaryTurnSpecificResultCoordinator result = null!;
    private CharacterSabotageActionSpecificResultCoordinator results = null!;
    private CharacterSabotageActionSpecificOutcomeCoordinator outcomes = null!;
    private CharacterTargetSabotageDelayResolutionCoordinator resolution = null!;
    private CharacterTargetSabotageDelayController state = null!;

    /// <summary>Gets the exact generic author request bound to the ordinary-turn outcome.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact acknowledged typed Delay result bound here.</summary>
    public CharacterTargetSabotageDelayConsiderationResult? Result { get; private set; }

    /// <summary>Gets the exact authoritative event log paired with the outcome.</summary>
    public WorldEventLogSnapshot? EventLog { get; private set; }

    /// <summary>Gets the exact authoritative world time paired with the outcome.</summary>
    public WorldTimeSnapshot? WorldTime { get; private set; }

    /// <summary>Gets the exact applied Delay outcome bound through the existing seam.</summary>
    public CharacterTargetSabotageDelayResult? Delay { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        request = GetNode<CharacterSabotageOrdinaryTurnSpecificRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificRequestCoordinator");
        result = GetNode<CharacterSabotageOrdinaryTurnSpecificResultCoordinator>(
            "../CharacterSabotageOrdinaryTurnSpecificResultCoordinator");
        results = GetNode<CharacterSabotageActionSpecificResultCoordinator>(
            "../CharacterSabotageActionSpecificResultCoordinator");
        outcomes = GetNode<CharacterSabotageActionSpecificOutcomeCoordinator>(
            "../CharacterSabotageActionSpecificOutcomeCoordinator");
        resolution = GetNode<CharacterTargetSabotageDelayResolutionCoordinator>(
            "../CharacterTargetSabotageDelayResolutionCoordinator");
        state = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
    }

    /// <summary>Binds once from only the exact retained result and authoritative outcome.</summary>
    public bool TryBind(
        CharacterTargetSabotageDelayConsiderationResult? expectedResult,
        CharacterTargetSabotageDelayResult? expectedDelay)
    {
        if (AuthorRequest is not null
            || Result is not null
            || EventLog is not null
            || WorldTime is not null
            || Delay is not null
            || outcomes.AuthorRequest is not null
            || outcomes.OutcomeCount != 0
            || !HasRetainedGraph(expectedResult, expectedDelay))
        {
            return false;
        }

        if (!outcomes.TryBindAcknowledgedOutcome()
            || outcomes.OutcomeCount != 1
            || !ReferenceEquals(outcomes.AuthorRequest, binding.SelectedAuthorRequest)
            || !ReferenceEquals(outcomes.Delay, expectedDelay))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay outcome must bind exactly once.");
        }

        AuthorRequest = binding.SelectedAuthorRequest;
        Result = expectedResult;
        EventLog = expectedDelay!.EventLog;
        WorldTime = expectedDelay.WorldTime;
        Delay = expectedDelay;
        return true;
    }

    private bool HasRetainedGraph(
        CharacterTargetSabotageDelayConsiderationResult? expectedResult,
        CharacterTargetSabotageDelayResult? expectedDelay)
    {
        CharacterSabotageActionConsiderationResult? genericResult = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return expectedResult is not null
            && expectedDelay is not null
            && candidateBinding is not null
            && binding.IsRetainedAcknowledgedBinding(
                genericResult,
                authorRequest,
                candidateBinding)
            && ReferenceEquals(result.Result, expectedResult)
            && ReferenceEquals(result.AppliedResult, expectedDelay)
            && ReferenceEquals(result.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(result.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(request.Request, expectedResult.Request)
            && ReferenceEquals(results.DelayResult, expectedResult)
            && ReferenceEquals(results.AcknowledgedAuthorRequest, authorRequest)
            && ReferenceEquals(resolution.AcknowledgedResult, expectedResult)
            && ReferenceEquals(resolution.AcknowledgedAppliedResult, expectedDelay)
            && ReferenceEquals(state.LatestAppliedResult, expectedDelay)
            && ReferenceEquals(state.EventLog, expectedDelay.EventLog)
            && ReferenceEquals(state.WorldTime, expectedDelay.WorldTime)
            && ReferenceEquals(expectedResult.SelectedCandidate, candidateBinding.Candidate)
            && ReferenceEquals(expectedDelay.Candidate, candidateBinding.Candidate);
    }
}
