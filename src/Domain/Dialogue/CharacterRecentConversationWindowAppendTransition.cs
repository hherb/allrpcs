namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Produces a new recent-conversation window by appending one exact completed turn.
/// </summary>
public static class CharacterRecentConversationWindowAppendTransition
{
    /// <summary>
    /// Appends a turn and evicts the oldest retained turns until both window bounds are satisfied.
    /// </summary>
    public static CharacterRecentConversationWindow Append(
        CharacterRecentConversationTurn turn,
        CharacterRecentConversationWindow window)
    {
        ArgumentNullException.ThrowIfNull(turn);
        ArgumentNullException.ThrowIfNull(window);

        if (turn.CharacterId != window.CharacterId)
        {
            throw new ArgumentException(
                "The appended turn must belong to the window character.",
                nameof(turn));
        }

        if (window.Turns.Any(existing => existing.TurnId == turn.TurnId))
        {
            throw new ArgumentException(
                "The appended turn identifier must be unique within the window.",
                nameof(turn));
        }

        List<CharacterRecentConversationTurn> retainedTurns = [.. window.Turns, turn];
        int textLength = retainedTurns.Sum(CharacterRecentConversationWindow.GetTextLength);

        while (retainedTurns.Count > CharacterRecentConversationWindow.MaximumTurnCount
            || textLength > CharacterRecentConversationWindow.MaximumTextLength)
        {
            textLength -= CharacterRecentConversationWindow.GetTextLength(retainedTurns[0]);
            retainedTurns.RemoveAt(0);
        }

        return new CharacterRecentConversationWindow(window.CharacterId, retainedTurns);
    }
}
