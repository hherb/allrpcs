using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes one exact ordinary-turn Delay recovery request.</summary>
public partial class CharacterSabotageOrdinaryTurnDelayRecoveryRequestCoordinator : Node
{
    private CharacterSabotageOrdinaryTurnResultBindingCoordinator binding = null!;
    private CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator candidate = null!;
    private CharacterSabotageOutcomeDelayRecoveryCoordinator recovery = null!;

    /// <summary>Gets the exact generic author request retained for the recovery request.</summary>
    public CharacterSabotageActionConsiderationRequest? AuthorRequest { get; private set; }

    /// <summary>Gets the exact Delay recovery candidate retained for the request.</summary>
    public CharacterTargetSabotageDelayRecoveryCandidate? Candidate { get; private set; }

    /// <summary>Gets the exact recovery request composed through the existing owner.</summary>
    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        binding = GetNode<CharacterSabotageOrdinaryTurnResultBindingCoordinator>(
            "../CharacterSabotageOrdinaryTurnResultBindingCoordinator");
        candidate = GetNode<CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator>(
            "../CharacterSabotageOrdinaryTurnDelayRecoveryCandidateCoordinator");
        recovery = GetNode<CharacterSabotageOutcomeDelayRecoveryCoordinator>(
            "../CharacterSabotageOutcomeDelayRecoveryCoordinator");
    }

    /// <summary>Composes once from only the exact retained ordinary-turn recovery tokens.</summary>
    public bool TryCompose(
        CharacterSabotageActionConsiderationRequest? expectedAuthorRequest,
        CharacterTargetSabotageDelayRecoveryCandidate? expectedCandidate)
    {
        if (expectedAuthorRequest is null
            || expectedCandidate is null
            || AuthorRequest is not null
            || Candidate is not null
            || Request is not null
            || recovery.AuthorRequest is not null
            || recovery.Request is not null
            || !ReferenceEquals(binding.SelectedAuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate.AuthorRequest, expectedAuthorRequest)
            || !ReferenceEquals(candidate.Candidate, expectedCandidate)
            || !ReferenceEquals(candidate.Delay, expectedCandidate.DelayResult)
            || !ReferenceEquals(candidate.RecoveryEvent, expectedCandidate.RecoveryEvent))
        {
            return false;
        }

        if (!recovery.TryCompose(expectedAuthorRequest, expectedCandidate)
            || recovery.Request is not { } request
            || !ReferenceEquals(recovery.AuthorRequest, expectedAuthorRequest)
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], expectedCandidate))
        {
            throw new InvalidOperationException(
                "A preflighted ordinary-turn Delay recovery request must compose once.");
        }

        AuthorRequest = expectedAuthorRequest;
        Candidate = expectedCandidate;
        Request = request;
        return true;
    }
}
