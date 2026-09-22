using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral sabotage-action decision for an exact request.</summary>
public interface ICharacterSabotageActionDecisionModel
{
    /// <summary>Returns one sabotage-action decision while observing caller cancellation.</summary>
    ValueTask<CharacterSabotageActionDecision> DecideAsync(
        CharacterSabotageActionConsiderationRequest request,
        CancellationToken cancellationToken);
}
