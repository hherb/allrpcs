using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral delay-recovery decision.</summary>
public static class CharacterTargetSabotageDelayRecoveryConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterTargetSabotageDelayRecoveryConsiderationResult> DecideAsync(
        ICharacterTargetSabotageDelayRecoveryDecisionModel model,
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterTargetSabotageDelayRecoveryDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterTargetSabotageDelayRecoveryConsiderationResult(
            request,
            decision);
    }
}
