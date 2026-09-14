using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral move decision for an exact request.</summary>
public interface ICharacterObjectSabotageMoveDecisionModel
{
    /// <summary>Returns one move decision while observing caller cancellation.</summary>
    ValueTask<CharacterObjectSabotageMoveDecision> DecideAsync(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CancellationToken cancellationToken);
}
