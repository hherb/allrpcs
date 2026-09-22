using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral delay-recovery decision for an exact request.</summary>
public interface ICharacterTargetSabotageDelayRecoveryDecisionModel
{
    /// <summary>Returns one delay-recovery decision while observing caller cancellation.</summary>
    ValueTask<CharacterTargetSabotageDelayRecoveryDecision> DecideAsync(
        CharacterTargetSabotageDelayRecoveryConsiderationRequest request,
        CancellationToken cancellationToken);
}
