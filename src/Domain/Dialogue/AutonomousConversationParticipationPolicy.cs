using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Gates a complete validated participation round on the autonomous-conversation budget.
/// </summary>
public static class AutonomousConversationParticipationPolicy
{
    /// <summary>
    /// Returns the exact selected speaker only while the autonomous chain may continue.
    /// </summary>
    public static CharacterTurnParticipationDecision? SelectNext(
        AutonomousConversationBudget budget,
        IEnumerable<Character> scheduledCharacters,
        IEnumerable<CharacterTurnParticipationDecision> decisions,
        IEnumerable<CharacterId> directlyAddressedCharacterIds)
    {
        ArgumentNullException.ThrowIfNull(budget);

        CharacterTurnParticipationDecision? selected =
            CharacterTurnParticipationRoundPolicy.SelectNext(
                scheduledCharacters,
                decisions,
                directlyAddressedCharacterIds);

        return AutonomousConversationContinuationPolicy.CanContinue(budget)
            ? selected
            : null;
    }
}
