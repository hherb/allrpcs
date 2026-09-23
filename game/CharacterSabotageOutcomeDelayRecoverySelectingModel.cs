using Allrpcs.Application.Characters;
using Allrpcs.Domain.Characters;

namespace Allrpcs.Game;

/// <summary>Selects the exact sole Delay-recovery candidate for the generic-route smoke.</summary>
internal sealed class CharacterSabotageOutcomeDelayRecoverySelectingModel
    : ICharacterTargetSabotageDelayRecoveryDecisionModel
{
    internal int CallCount { get; private set; }

    internal CharacterTargetSabotageDelayRecoveryConsiderationRequest? Request { get; private set; }

    /// <inheritdoc />
    public ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest value,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        CallCount++;
        Request = value;
        return ValueTask.FromResult(
            new CharacterTargetSabotageDelayRecoveryDecision(
                value.Actor.Identity.Id,
                value.Candidates.Single()));
    }
}
