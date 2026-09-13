using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral concealment decision for an exact request.</summary>
public interface ICharacterObjectSabotageConcealmentDecisionModel
{
    /// <summary>Returns one concealment decision while observing caller cancellation.</summary>
    ValueTask<CharacterObjectSabotageConcealmentDecision> DecideAsync(
        CharacterObjectSabotageConcealmentConsiderationRequest request,
        CancellationToken cancellationToken);
}
