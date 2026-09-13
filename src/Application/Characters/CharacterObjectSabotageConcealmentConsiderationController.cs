using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral concealment decision.</summary>
public static class CharacterObjectSabotageConcealmentConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterObjectSabotageConcealmentConsiderationResult> DecideAsync(
        ICharacterObjectSabotageConcealmentDecisionModel model,
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterObjectSabotageConcealmentDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterObjectSabotageConcealmentConsiderationResult(request, decision);
    }
}
