using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral move decision.</summary>
public static class CharacterObjectSabotageMoveConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterObjectSabotageMoveConsiderationResult> DecideAsync(
        ICharacterObjectSabotageMoveDecisionModel model,
        CharacterObjectSabotageMoveConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterObjectSabotageMoveDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterObjectSabotageMoveConsiderationResult(request, decision);
    }
}
