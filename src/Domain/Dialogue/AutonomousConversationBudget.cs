namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains the immutable bounded state of one autonomous character-conversation chain.
/// </summary>
public sealed class AutonomousConversationBudget
{
    /// <summary>Gets the largest configured turn count accepted by one chain.</summary>
    public const int MaximumAllowedTurnCount = CharacterRecentConversationWindow.MaximumTurnCount;

    /// <summary>Creates one bounded autonomous-conversation state.</summary>
    public AutonomousConversationBudget(
        int completedTurnCount,
        int maximumTurnCount,
        bool hasPlayerInterrupted)
    {
        if (maximumTurnCount is < 1 or > MaximumAllowedTurnCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumTurnCount),
                maximumTurnCount,
                $"An autonomous conversation maximum must be between 1 and {MaximumAllowedTurnCount}.");
        }

        if (completedTurnCount < 0 || completedTurnCount > maximumTurnCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedTurnCount),
                completedTurnCount,
                "Completed autonomous-conversation turns must be nonnegative and no greater "
                    + "than the configured maximum.");
        }

        CompletedTurnCount = completedTurnCount;
        MaximumTurnCount = maximumTurnCount;
        HasPlayerInterrupted = hasPlayerInterrupted;
    }

    /// <summary>Gets the number of character turns already completed in this chain.</summary>
    public int CompletedTurnCount { get; }

    /// <summary>Gets the configured maximum number of completed character turns.</summary>
    public int MaximumTurnCount { get; }

    /// <summary>Gets whether the player has interrupted this chain.</summary>
    public bool HasPlayerInterrupted { get; }
}
