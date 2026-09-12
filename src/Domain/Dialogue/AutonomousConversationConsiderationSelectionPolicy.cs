namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Gates selection from one complete participation-consideration result round on the exact budget.
/// </summary>
public static class AutonomousConversationConsiderationSelectionPolicy
{
    /// <summary>
    /// Returns the exact aligned first-speaking result only while the autonomous chain may continue.
    /// </summary>
    public static CharacterTurnParticipationConsiderationResult? SelectNext(
        AutonomousConversationBudget budget,
        CharacterTurnParticipationConsiderationRoundResult round)
    {
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(round);

        CharacterTurnParticipationDecision? selectedDecision =
            CharacterTurnParticipationSelectionPolicy.SelectNext(
                round.Results.Select(result => result.Decision));

        if (!AutonomousConversationContinuationPolicy.CanContinue(budget)
            || selectedDecision is null)
        {
            return null;
        }

        return round.Results.First(result => ReferenceEquals(result.Decision, selectedDecision));
    }
}
