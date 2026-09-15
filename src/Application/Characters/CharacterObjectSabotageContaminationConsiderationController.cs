using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral contamination decision.</summary>
public static class CharacterObjectSabotageContaminationConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterObjectSabotageContaminationConsiderationResult> DecideAsync(
        ICharacterObjectSabotageContaminationDecisionModel model,
        CharacterObjectSabotageContaminationConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterObjectSabotageContaminationDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterObjectSabotageContaminationConsiderationResult(request, decision);
    }
}
