using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Obtains one provider-neutral participation decision for an exact consideration request.
/// </summary>
public interface ICharacterTurnParticipationModel
{
    /// <summary>
    /// Returns one participation decision while observing caller cancellation.
    /// </summary>
    ValueTask<CharacterTurnParticipationDecision> DecideAsync(
        CharacterTurnParticipationConsiderationRequest request,
        CancellationToken cancellationToken);
}
