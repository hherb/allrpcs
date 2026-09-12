using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one exact immutable autonomous-conversation budget.</summary>
public partial class AutonomousConversationBudgetController : Node
{
    /// <summary>Gets the exact initialized budget, if any.</summary>
    public AutonomousConversationBudget? Budget { get; private set; }

    /// <summary>Initializes this controller once from one exact existing budget.</summary>
    public bool TryInitialize(AutonomousConversationBudget budget)
    {
        ArgumentNullException.ThrowIfNull(budget);

        if (Budget is not null)
        {
            return false;
        }

        Budget = budget;
        return true;
    }

    /// <summary>Records exactly one further completed turn when the budget permits it.</summary>
    public bool TryRecordCompletedTurn()
    {
        AutonomousConversationBudget? budget = Budget;
        if (budget is null || !AutonomousConversationContinuationPolicy.CanContinue(budget))
        {
            return false;
        }

        Budget = AutonomousConversationBudgetTransition.RecordCompletedTurn(budget);
        return true;
    }

    /// <summary>Monotonically records the player's first interruption.</summary>
    public bool TryInterrupt()
    {
        AutonomousConversationBudget? budget = Budget;
        if (budget is null || budget.HasPlayerInterrupted)
        {
            return false;
        }

        Budget = AutonomousConversationBudgetTransition.Interrupt(budget);
        return true;
    }
}
