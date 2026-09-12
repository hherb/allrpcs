namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Determines whether a bounded autonomous character-conversation chain may continue.
/// </summary>
public static class AutonomousConversationContinuationPolicy
{
    /// <summary>Returns true only before interruption and while one turn remains available.</summary>
    public static bool CanContinue(AutonomousConversationBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        return !budget.HasPlayerInterrupted
            && budget.CompletedTurnCount < budget.MaximumTurnCount;
    }
}
