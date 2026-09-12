namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains the exact completed activity and advanced autonomous-conversation budget.
/// </summary>
public sealed class AutonomousConversationTurnCompletionResult
{
    /// <summary>Creates one immutable atomic completion result.</summary>
    public AutonomousConversationTurnCompletionResult(
        CharacterTurnActivity activity,
        AutonomousConversationBudget budget)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(budget);

        Activity = activity;
        Budget = budget;
    }

    /// <summary>Gets the exact resulting completed character-turn activity.</summary>
    public CharacterTurnActivity Activity { get; }

    /// <summary>Gets the exact resulting autonomous-conversation budget.</summary>
    public AutonomousConversationBudget Budget { get; }
}
