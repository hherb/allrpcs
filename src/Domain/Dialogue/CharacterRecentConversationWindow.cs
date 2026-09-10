using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Contains one immutable bounded window of completed local turns for an exact character.
/// </summary>
public sealed class CharacterRecentConversationWindow
{
    /// <summary>Gets the maximum number of completed turns retained in one window.</summary>
    public const int MaximumTurnCount = 16;

    /// <summary>Gets the maximum aggregate dialogue text retained, in UTF-16 code units.</summary>
    public const int MaximumTextLength = 16_000;

    /// <summary>Creates a bounded window in stable oldest-to-newest order.</summary>
    public CharacterRecentConversationWindow(
        CharacterId characterId,
        IEnumerable<CharacterRecentConversationTurn> turns)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(turns);

        List<CharacterRecentConversationTurn> retainedTurns = new();
        HashSet<CharacterTurnId> turnIds = new();
        int textLength = 0;

        foreach (CharacterRecentConversationTurn turn in turns)
        {
            if (turn is null)
            {
                throw new ArgumentException(
                    "Recent-conversation turns cannot contain null values.",
                    nameof(turns));
            }

            if (turn.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every recent-conversation turn must belong to the window character.",
                    nameof(turns));
            }

            if (!turnIds.Add(turn.TurnId))
            {
                throw new ArgumentException(
                    "Recent-conversation turn identifiers must be unique within a window.",
                    nameof(turns));
            }

            retainedTurns.Add(turn);
            if (retainedTurns.Count > MaximumTurnCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(turns),
                    retainedTurns.Count,
                    $"A recent-conversation window accepts at most {MaximumTurnCount} turns.");
            }

            textLength += GetTextLength(turn);
            if (textLength > MaximumTextLength)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(turns),
                    textLength,
                    "A recent-conversation window accepts at most "
                        + $"{MaximumTextLength} UTF-16 code units of dialogue text.");
            }
        }

        CharacterId = characterId;
        Turns = retainedTurns.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this window.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets completed turns in stable oldest-to-newest order.</summary>
    public IReadOnlyList<CharacterRecentConversationTurn> Turns { get; }

    internal static int GetTextLength(CharacterRecentConversationTurn turn)
    {
        string heardText = turn.Activity.Request.DialogueEvent.Utterance.Text;
        string? responseText = turn.Activity.Response?.Speech?.Text;
        return heardText.Length + (responseText?.Length ?? 0);
    }
}
