using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains and validates one provider-neutral incompatibility response.</summary>
public static class CharacterBeliefGoalIncompatibilityConsiderationController
{
    /// <summary>Returns one accepted result for the exact request and caller cancellation.</summary>
    public static async Task<CharacterBeliefGoalIncompatibilityConsiderationResult> RespondAsync(
        ICharacterBeliefGoalIncompatibilityResponseModel model,
        CharacterBeliefGoalIncompatibilityConsiderationRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        CharacterBeliefGoalIncompatibilityResponse response = await model
            .RespondAsync(request, cancellationToken)
            .ConfigureAwait(false);

        return new CharacterBeliefGoalIncompatibilityConsiderationResult(request, response);
    }
}
