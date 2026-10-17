using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Records a validated supplied summary without replacing either conversation source.</summary>
public static class CharacterConversationSummaryRecordingTransition
{
    /// <summary>Appends the exact summary when its ordered source turns remain in the recent window.</summary>
    public static CharacterConversationSummaryRecordingResult Record(
        CharacterConversationSummary summary,
        CharacterConversationSummarySnapshot summaries,
        CharacterRecentConversationWindow recentConversation,
        CharacterTurnTranscriptSnapshot transcript)
    {
        ArgumentNullException.ThrowIfNull(summary);
        ArgumentNullException.ThrowIfNull(summaries);
        ArgumentNullException.ThrowIfNull(recentConversation);
        ArgumentNullException.ThrowIfNull(transcript);
        if (summary.CharacterId != summaries.CharacterId
            || summary.CharacterId != recentConversation.CharacterId)
        {
            throw new ArgumentException(
                "The summary, summary snapshot, and recent conversation must belong to one character.",
                nameof(summary));
        }

        if (!ContainsInRelativeOrder(summary.SourceTurnIds, recentConversation.Turns))
        {
            throw new ArgumentException(
                "Every summary source turn must exist in recent-conversation order.",
                nameof(summary));
        }

        if (summaries.Summaries.Any(existing => existing.SummaryId == summary.SummaryId))
        {
            throw new ArgumentException("Conversation-summary identities must be unique.", nameof(summary));
        }
        if (summaries.Summaries.Count == CharacterConversationSummarySnapshot.MaximumSummaryCount)
        {
            return new(CharacterConversationSummaryRecordingOutcome.CapacityRejected,
                summaries, recentConversation, transcript);
        }

        CharacterConversationSummarySnapshot updatedSummaries = new(
            summaries.CharacterId,
            summaries.Summaries.Append(summary));
        return new CharacterConversationSummaryRecordingResult(
            CharacterConversationSummaryRecordingOutcome.Recorded,
            updatedSummaries,
            recentConversation,
            transcript);
    }

    private static bool ContainsInRelativeOrder(
        IReadOnlyList<CharacterTurnId> sourceTurnIds,
        IReadOnlyList<CharacterRecentConversationTurn> recentTurns)
    {
        int sourceIndex = 0;
        foreach (CharacterRecentConversationTurn recentTurn in recentTurns)
        {
            if (recentTurn.TurnId == sourceTurnIds[sourceIndex])
            {
                sourceIndex++;
                if (sourceIndex == sourceTurnIds.Count)
                {
                    return true;
                }
            }
        }

        return false;
    }
}
