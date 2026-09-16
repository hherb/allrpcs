using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral influence decision for an exact request.</summary>
public interface ICharacterTargetSabotageInfluenceDecisionModel
{
    /// <summary>Returns one influence decision while observing caller cancellation.</summary>
    ValueTask<CharacterTargetSabotageInfluenceDecision> DecideAsync(
        CharacterTargetSabotageInfluenceConsiderationRequest request,
        CancellationToken cancellationToken);
}
