namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Produces immutable completed-turn and player-interruption budget states.
/// </summary>
public static class AutonomousConversationBudgetTransition
{
    /// <summary>Returns a new state recording exactly one further completed character turn.</summary>
    public static AutonomousConversationBudget RecordCompletedTurn(
        AutonomousConversationBudget budget)
    {
        if (!AutonomousConversationContinuationPolicy.CanContinue(budget))
        {
            throw new InvalidOperationException(
                "An exhausted or interrupted autonomous conversation cannot record another turn.");
        }

        return new AutonomousConversationBudget(
            budget.CompletedTurnCount + 1,
            budget.MaximumTurnCount,
            budget.HasPlayerInterrupted);
    }

    /// <summary>Returns a monotonically interrupted state, retaining an existing one exactly.</summary>
    public static AutonomousConversationBudget Interrupt(AutonomousConversationBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        return budget.HasPlayerInterrupted
            ? budget
            : new AutonomousConversationBudget(
                budget.CompletedTurnCount,
                budget.MaximumTurnCount,
                true);
    }
}
