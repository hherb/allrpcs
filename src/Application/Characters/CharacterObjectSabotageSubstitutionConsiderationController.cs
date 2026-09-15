using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral substitution decision.</summary>
public static class CharacterObjectSabotageSubstitutionConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterObjectSabotageSubstitutionConsiderationResult> DecideAsync(
        ICharacterObjectSabotageSubstitutionDecisionModel model,
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterObjectSabotageSubstitutionDecision decision = await model
            .DecideAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterObjectSabotageSubstitutionConsiderationResult(request, decision);
    }
}
