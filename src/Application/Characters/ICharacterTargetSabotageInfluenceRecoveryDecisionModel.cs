using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral influence-recovery decision for an exact request.</summary>
public interface ICharacterTargetSabotageInfluenceRecoveryDecisionModel
{
    /// <summary>Returns one influence-recovery decision while observing caller cancellation.</summary>
    ValueTask<CharacterTargetSabotageInfluenceRecoveryDecision> DecideAsync(
        CharacterTargetSabotageInfluenceRecoveryConsiderationRequest request,
        CancellationToken cancellationToken);
}
