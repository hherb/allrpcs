namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Atomically completes one autonomous character turn and advances its bounded conversation.
/// </summary>
public static class AutonomousConversationTurnCompletionTransition
{
    /// <summary>Returns exact new activity and budget state for one eligible completion.</summary>
    public static AutonomousConversationTurnCompletionResult Complete(
        AutonomousConversationBudget budget,
        CharacterTurnActivity activity)
    {
        ArgumentNullException.ThrowIfNull(budget);
        ArgumentNullException.ThrowIfNull(activity);

        if (!AutonomousConversationContinuationPolicy.CanContinue(budget)
            || !CharacterTurnCompletionPolicy.CanComplete(activity))
        {
            throw new InvalidOperationException(
                "Only a speaking turn in an open autonomous conversation may complete.");
        }

        CharacterTurnActivity completedActivity =
            CharacterTurnCompletionTransition.Complete(activity);
        AutonomousConversationBudget advancedBudget =
            AutonomousConversationBudgetTransition.RecordCompletedTurn(budget);

        return new AutonomousConversationTurnCompletionResult(
            completedActivity,
            advancedBudget);
    }
}
