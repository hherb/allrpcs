using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Records the exact generic sabotage decision requested by the opportunity smoke.</summary>
internal sealed class CharacterSabotageTurnOpportunityRecordingModel
    : ICharacterSabotageActionDecisionModel
{
    public int CallCount { get; private set; }

    public ValueTask<CharacterSabotageActionDecision> DecideAsync(
        CharacterSabotageActionConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        return ValueTask.FromResult(
            new CharacterSabotageActionDecision(
                request.Actor.Identity.Id,
                CharacterSabotageActionKind.Delay));
    }
}

/// <summary>Records the exact typed Delay decision requested by the opportunity smoke.</summary>
internal sealed class CharacterSabotageTurnOpportunityRecordingDelayModel
    : ICharacterTargetSabotageDelayDecisionModel
{
    public int CallCount { get; private set; }

    public CharacterTargetSabotageDelayConsiderationRequest? Request { get; private set; }

    public ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        Request = request;
        return ValueTask.FromResult(
            new CharacterTargetSabotageDelayDecision(
                request.Actor.Identity.Id,
                request.Candidates.Single()));
    }
}

/// <summary>Returns one exact Delay recovery decision for the opportunity smoke.</summary>
internal sealed class CharacterSabotageTurnOpportunityRecordingDelayRecoveryModel
    : ICharacterTargetSabotageDelayRecoveryDecisionModel
{
    public int CallCount { get; private set; }

    public CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        Request = request;
        return ValueTask.FromResult(
            new CharacterTargetSabotageDelayRecoveryDecision(
                request.Actor.Identity.Id,
                request.Candidates.Single()));
    }
}
