using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Reports whether a validated summary was recorded or the bounded store was full.</summary>
public enum CharacterConversationSummaryRecordingOutcome
{
    /// <summary>The exact summary was appended.</summary>
    Recorded,
    /// <summary>No source snapshot changed because summary capacity was reached.</summary>
    CapacityRejected,
}

/// <summary>Retains updated summaries alongside unchanged authoritative conversation sources.</summary>
public sealed class CharacterConversationSummaryRecordingResult
{
    internal CharacterConversationSummaryRecordingResult(
        CharacterConversationSummaryRecordingOutcome outcome,
        CharacterConversationSummarySnapshot summaries,
        CharacterRecentConversationWindow recentConversation,
        CharacterTurnTranscriptSnapshot transcript)
    {
        Outcome = outcome;
        Summaries = summaries;
        RecentConversation = recentConversation;
        Transcript = transcript;
    }

    /// <summary>Gets the explicit recording outcome.</summary>
    public CharacterConversationSummaryRecordingOutcome Outcome { get; }

    /// <summary>Gets the updated snapshot, or the exact original snapshot on capacity rejection.</summary>
    public CharacterConversationSummarySnapshot Summaries { get; }

    /// <summary>Gets the exact unchanged recent-conversation window.</summary>
    public CharacterRecentConversationWindow RecentConversation { get; }

    /// <summary>Gets the exact unchanged authoritative transcript.</summary>
    public CharacterTurnTranscriptSnapshot Transcript { get; }
}
