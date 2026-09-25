using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one acknowledged ordinary-turn Delay result into its typed request.</summary>
public partial class CharacterSabotageOrdinaryTurnSpecificRequestCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageActionSpecificRequestCoordinator requests = null!;
    private CharacterSabotageActionDelayRequestController delayAuthor = null!;
    private CharacterTargetSabotageDelayController delayState = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained by typed composition.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact typed Delay request composed through the existing seam.</summary>
    public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        requests = GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
            "../CharacterSabotageActionSpecificRequestCoordinator");
        delayAuthor = GetNode<CharacterSabotageActionDelayRequestController>(
            "../CharacterSabotageActionDelayRequestController");
        delayState = GetNode<CharacterTargetSabotageDelayController>(
            "../CharacterTargetSabotageDelayController");
    }

    /// <summary>Composes once from only the exact acknowledged Delay author provenance.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            expectedBinding)
    {
        if (Request is not null
            || requests.RequestCount != 0
            || !binding.IsCurrentAcknowledgedBinding(
                expectedResult,
                expectedAuthorRequest,
                expectedBinding)
            || !ReferenceEquals(expectedResult!.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(expectedBinding!.ActionKind, CharacterSabotageActionKind.Delay)
            || !HasCurrentDelayGraph(expectedAuthorRequest!, expectedBinding))
        {
            return false;
        }

        if (!requests.TryCompose(expectedAuthorRequest, expectedBinding)
            || requests.DelayRequest is not { } request)
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay result must compose one exact typed request.");
        }

        Request = request;
        return true;
    }

    /// <summary>Composes once from only the exact acknowledged reserved Delay provenance.</summary>
    public bool TryCompose(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageActionConsiderationResult? expectedResult,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            expectedBinding)
    {
        if (Reservation is not null
            || Request is not null
            || requests.RequestCount != 0
            || !binding.IsCurrentAcknowledgedBinding(
                expectedReservation,
                expectedResult,
                expectedAuthorRequest,
                expectedBinding)
            || !ReferenceEquals(expectedResult!.SelectedActionKind, CharacterSabotageActionKind.Delay)
            || !ReferenceEquals(expectedBinding!.ActionKind, CharacterSabotageActionKind.Delay)
            || !HasCurrentDelayGraph(expectedAuthorRequest!, expectedBinding))
        {
            return false;
        }

        if (!requests.TryCompose(expectedAuthorRequest, expectedBinding)
            || requests.DelayRequest is not { } request)
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay result must compose one exact typed request.");
        }

        Reservation = expectedReservation;
        Request = request;
        return true;
    }

    /// <summary>Checks the retained typed request against its complete current provenance.</summary>
    internal bool IsCurrentRequest(
        CharacterTargetSabotageDelayConsiderationRequest? expectedRequest)
    {
        CharacterSabotageActionConsiderationResult? result = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return Request is not null
            && ReferenceEquals(Request, expectedRequest)
            && requests.RequestCount == 1
            && ReferenceEquals(requests.DelayRequest, Request)
            && binding.IsCurrentAcknowledgedBinding(result, authorRequest, candidateBinding)
            && HasCurrentDelayGraph(authorRequest!, candidateBinding!)
            && HasExactRequestValues(Request, candidateBinding!.Candidate);
    }

    /// <summary>Checks the retained typed request against its exact reserved provenance.</summary>
    internal bool IsCurrentRequest(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterTargetSabotageDelayConsiderationRequest? expectedRequest)
    {
        CharacterSabotageActionConsiderationResult? result = binding.AcknowledgedResult;
        CharacterSabotageActionConsiderationRequest? authorRequest =
            binding.SelectedAuthorRequest;
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>?
            candidateBinding = binding.SelectedCandidateBinding as
                CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>;
        return Reservation is not null
            && ReferenceEquals(Reservation, expectedReservation)
            && Request is not null
            && ReferenceEquals(Request, expectedRequest)
            && requests.RequestCount == 1
            && ReferenceEquals(requests.DelayRequest, Request)
            && binding.IsCurrentAcknowledgedBinding(
                expectedReservation,
                result,
                authorRequest,
                candidateBinding)
            && HasCurrentDelayGraph(authorRequest!, candidateBinding!)
            && HasExactRequestValues(Request, candidateBinding!.Candidate);
    }

    private bool HasCurrentDelayGraph(
        CharacterSabotageActionConsiderationRequest expectedAuthorRequest,
        CharacterSabotageActionCandidateBinding<CharacterTargetSabotageDelayCandidate>
            expectedBinding)
    {
        CharacterTargetSabotageDelayConsiderationRequest? sourceRequest =
            delayAuthor.SourceRequest;
        return sourceRequest is not null
            && ReferenceEquals(expectedBinding.Request, expectedAuthorRequest)
            && ReferenceEquals(expectedBinding.ActionKind, CharacterSabotageActionKind.Delay)
            && ReferenceEquals(expectedAuthorRequest.Actor, sourceRequest.Actor)
            && sourceRequest.Candidates.Any(candidate =>
                ReferenceEquals(candidate, expectedBinding.Candidate))
            && ReferenceEquals(delayState.EventLog, sourceRequest.EventLog)
            && ReferenceEquals(delayState.WorldTime, sourceRequest.WorldTime)
            && CharacterTargetSabotageDelayPolicy.CanApply(
                sourceRequest.Actor,
                sourceRequest.InteractionAccess,
                sourceRequest.WorldEvents,
                sourceRequest.EventLog,
                sourceRequest.WorldTime,
                expectedBinding.Candidate);
    }

    private bool HasExactRequestValues(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CharacterTargetSabotageDelayCandidate candidate)
    {
        CharacterTargetSabotageDelayConsiderationRequest? sourceRequest =
            delayAuthor.SourceRequest;
        return sourceRequest is not null
            && ReferenceEquals(request.Actor, sourceRequest.Actor)
            && ReferenceEquals(request.InteractionAccess, sourceRequest.InteractionAccess)
            && ReferenceEquals(request.WorldEvents, sourceRequest.WorldEvents)
            && ReferenceEquals(request.EventLog, sourceRequest.EventLog)
            && ReferenceEquals(request.WorldTime, sourceRequest.WorldTime)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates[0], candidate);
    }
}
