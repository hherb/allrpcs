using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral sabotage-action decision.</summary>
public static class CharacterSabotageActionConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterSabotageActionConsiderationResult> DecideAsync(
        ICharacterSabotageActionDecisionModel model,
        CharacterSabotageActionConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterSabotageActionDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterSabotageActionConsiderationResult(request, decision);
    }
}
