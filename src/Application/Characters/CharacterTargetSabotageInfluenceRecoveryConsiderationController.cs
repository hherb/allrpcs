using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral influence-recovery decision.</summary>
public static class CharacterTargetSabotageInfluenceRecoveryConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterTargetSabotageInfluenceRecoveryConsiderationResult> DecideAsync(
        ICharacterTargetSabotageInfluenceRecoveryDecisionModel model,
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterTargetSabotageInfluenceRecoveryDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterTargetSabotageInfluenceRecoveryConsiderationResult(
            request,
            decision);
    }
}
