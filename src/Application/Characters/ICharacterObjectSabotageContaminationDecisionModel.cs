using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral contamination decision for an exact request.</summary>
public interface ICharacterObjectSabotageContaminationDecisionModel
{
    /// <summary>Returns one contamination decision while observing caller cancellation.</summary>
    ValueTask<CharacterObjectSabotageContaminationDecision> DecideAsync(
        CharacterObjectSabotageContaminationConsiderationRequest request,
        CancellationToken cancellationToken);
}
