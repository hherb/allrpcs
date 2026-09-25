using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Starts one exact ordinary-turn Delay recovery request.</summary>
public partial class CharacterSabotageOrdinaryTurnDelayRecoveryStartCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator candidate = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator request = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact sabotage-turn reservation retained at recovery start.</summary>
    public CharacterSabotageTurnReservation? Reservation { get; private set; }

    /// <summary>Gets the exact generic author request retained at recovery start.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact recovery request started through the existing owner.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        candidate = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator");
        request = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
    }

    /// <summary>Starts once from only the exact composed ordinary-turn recovery graph.</summary>
    public bool TryStart(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedAuthorRequest is null
            || expectedRequest is null
            || AuthorRequest is not null
            || Request is not null
            || recovery.StartedRequest is not null
            || !ReferenceEquals(candidate.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(request.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(request.Request, expectedRequest)
            || request.Candidate is not { } expectedCandidate
            || expectedRequest.Candidates.Count != 1
            || !ReferenceEquals(expectedRequest.Candidates[0], expectedCandidate)
            || !ReferenceEquals(candidate.Candidate, expectedCandidate))
        {
            return false;
        }

        if (!recovery.TryStart(expectedAuthorRequest, expectedRequest)
            || !ReferenceEquals(recovery.StartedRequest, expectedRequest))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay recovery request must start once.");
        }

        AuthorRequest = expectedAuthorRequest;
        Request = expectedRequest;
        return true;
    }

    /// <summary>Starts once from only the exact composed reserved recovery graph.</summary>
    public bool TryStart(
        CharacterSabotageTurnReservation? expectedReservation,
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest? expectedRequest)
    {
        if (expectedReservation is null
            || expectedAuthorRequest is null
            || expectedRequest is null
            || Reservation is not null
            || AuthorRequest is not null
            || Request is not null
            || recovery.StartedRequest is not null
            || !ReferenceEquals(candidate.Reservation, expectedReservation)
            || !ReferenceEquals(request.Reservation, expectedReservation)
            || !ReferenceEquals(candidate.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(request.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(request.Request, expectedRequest)
            || request.Candidate is not { } expectedCandidate
            || expectedRequest.Candidates.Count != 1
            || !ReferenceEquals(expectedRequest.Candidates[0], expectedCandidate)
            || !ReferenceEquals(candidate.Candidate, expectedCandidate))
        {
            return false;
        }

        if (!recovery.TryStart(expectedAuthorRequest, expectedRequest)
            || !ReferenceEquals(recovery.StartedRequest, expectedRequest))
        {
            throw new InvalidOperationException(
                "A preflighted reserved Delay recovery request must start once.");
        }

        Reservation = expectedReservation;
        AuthorRequest = expectedAuthorRequest;
        Request = expectedRequest;
        return true;
    }
}
