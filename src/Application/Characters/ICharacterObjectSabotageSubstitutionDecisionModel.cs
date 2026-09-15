using Allrpcs.Domain.Characters;

namespace Allrpcs.Application.Characters;

/// <summary>Obtains one provider-neutral substitution decision for an exact request.</summary>
public interface ICharacterObjectSabotageSubstitutionDecisionModel
{
    /// <summary>Returns one substitution decision while observing caller cancellation.</summary>
    ValueTask<CharacterObjectSabotageSubstitutionDecision> DecideAsync(
        CharacterObjectSabotageSubstitutionConsiderationRequest request,
        CancellationToken cancellationToken);
}
