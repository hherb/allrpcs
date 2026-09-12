using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Obtains and validates one complete bounded round of participation decisions.
/// </summary>
public static class CharacterTurnParticipationConsiderationController
{
    /// <summary>
    /// Preflights all requests, then obtains one exact accepted result per request in stable order.
    /// </summary>
    public static async Task<CharacterTurnParticipationConsiderationRoundResult> DecideRoundAsync(
        ICharacterTurnParticipationModel model,
        IEnumerable<Character> scheduledCharacters,
        DialogueEvent heardDialogueEvent,
        IEnumerable<CharacterId> directlyAddressedCharacterIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(model);

        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> requests =
            CharacterTurnParticipationConsiderationRoundTransition.CreateRequests(
                scheduledCharacters,
                heardDialogueEvent,
                directlyAddressedCharacterIds);
        List<CharacterTurnParticipationConsiderationResult> results = [];

        foreach (CharacterTurnParticipationConsiderationRequest request in requests)
        {
            CharacterTurnParticipationDecision decision = await model
                .DecideAsync(request, cancellationToken)
                .ConfigureAwait(false);
            results.Add(new CharacterTurnParticipationConsiderationResult(request, decision));
        }

        return new CharacterTurnParticipationConsiderationRoundResult(requests, results);
    }
}
