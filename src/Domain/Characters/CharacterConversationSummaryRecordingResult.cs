using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains updated summaries alongside unchanged authoritative conversation sources.</summary>
public sealed class CharacterConversationSummaryRecordingResult
{
    internal CharacterConversationSummaryRecordingResult(
        CharacterConversationSummarySnapshot summaries,
        CharacterRecentConversationWindow recentConversation,
        CharacterTurnTranscriptSnapshot transcript)
    {
        Summaries = summaries;
        RecentConversation = recentConversation;
        Transcript = transcript;
    }

    /// <summary>Gets the newly constructed summary snapshot.</summary>
    public CharacterConversationSummarySnapshot Summaries { get; }

    /// <summary>Gets the exact unchanged recent-conversation window.</summary>
    public CharacterRecentConversationWindow RecentConversation { get; }

    /// <summary>Gets the exact unchanged authoritative transcript.</summary>
    public CharacterTurnTranscriptSnapshot Transcript { get; }
}
