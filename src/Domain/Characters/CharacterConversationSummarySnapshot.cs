using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded older-conversation summaries for one character.</summary>
public sealed class CharacterConversationSummarySnapshot
{
    /// <summary>Gets the maximum summaries retained per character.</summary>
    public const int MaximumSummaryCount = 128;

    /// <summary>Creates one immutable per-character conversation-summary snapshot.</summary>
    public CharacterConversationSummarySnapshot(
        CharacterId characterId,
        IEnumerable<CharacterConversationSummary> summaries)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(summaries);

        List<CharacterConversationSummary> retainedSummaries = new();
        HashSet<CharacterConversationSummaryId> summaryIds = new();
        foreach (CharacterConversationSummary summary in summaries)
        {
            if (summary is null)
            {
                throw new ArgumentException(
                    "Conversation-summary snapshots cannot contain null values.",
                    nameof(summaries));
            }

            if (summary.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every conversation summary must belong to the snapshot character.",
                    nameof(summaries));
            }

            if (!summaryIds.Add(summary.SummaryId))
            {
                throw new ArgumentException(
                    "Conversation-summary identities must be unique within a character snapshot.",
                    nameof(summaries));
            }

            retainedSummaries.Add(summary);
            if (retainedSummaries.Count > MaximumSummaryCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(summaries),
                    retainedSummaries.Count,
                    $"A character accepts at most {MaximumSummaryCount} conversation summaries.");
            }
        }

        CharacterId = characterId;
        Summaries = retainedSummaries.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact summary instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterConversationSummary> Summaries { get; }
}
