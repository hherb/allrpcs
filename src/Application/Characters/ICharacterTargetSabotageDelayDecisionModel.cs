using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral target-delay decision for an exact request.</summary>
public interface ICharacterTargetSabotageDelayDecisionModel
{
    /// <summary>Returns one target-delay decision while observing caller cancellation.</summary>
    ValueTask<CharacterTargetSabotageDelayDecision> DecideAsync(
        CharacterTargetSabotageDelayConsiderationRequest request,
        CancellationToken cancellationToken);
}
