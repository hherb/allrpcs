using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral target-delay decision.</summary>
public static class CharacterTargetSabotageDelayConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterTargetSabotageDelayConsiderationResult> DecideAsync(
        ICharacterTargetSabotageDelayDecisionModel model,
        CharacterTargetSabotageDelayConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterTargetSabotageDelayDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterTargetSabotageDelayConsiderationResult(request, decision);
    }
}
