using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral incompatibility response for an exact request.</summary>
public interface ICharacterBeliefGoalIncompatibilityResponseModel
{
    /// <summary>Returns one response while observing caller cancellation.</summary>
    ValueTask<CharacterBeliefGoalIncompatibilityResponse> RespondAsync(
        CharacterBeliefGoalIncompatibilityConsiderationRequest request,
        CancellationToken cancellationToken);
}
