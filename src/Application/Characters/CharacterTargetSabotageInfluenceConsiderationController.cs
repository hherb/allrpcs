using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral influence decision.</summary>
public static class CharacterTargetSabotageInfluenceConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterTargetSabotageInfluenceConsiderationResult> DecideAsync(
        ICharacterTargetSabotageInfluenceDecisionModel model,
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterTargetSabotageInfluenceDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterTargetSabotageInfluenceConsiderationResult(request, decision);
    }
}
