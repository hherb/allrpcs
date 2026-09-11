using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Retains one bounded local summary referencing exact completed source turns.</summary>
public sealed record CharacterConversationSummary
{
    /// <summary>Gets the maximum source turns referenced by one summary.</summary>
    public const int MaximumSourceTurnCount = 32;

    /// <summary>Gets the maximum summary-text length in UTF-16 code units.</summary>
    public const int MaximumTextLength = 2_000;

    /// <summary>Creates one immutable older-conversation summary.</summary>
    public CharacterConversationSummary(
        CharacterConversationSummaryId summaryId,
        CharacterId characterId,
        IEnumerable<CharacterTurnId> sourceTurnIds,
        string text)
    {
        ArgumentNullException.ThrowIfNull(summaryId);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(sourceTurnIds);

        List<CharacterTurnId> retainedSourceTurnIds = new();
        HashSet<CharacterTurnId> sourceTurnIdSet = new();
        foreach (CharacterTurnId sourceTurnId in sourceTurnIds)
        {
            if (sourceTurnId is null || !sourceTurnIdSet.Add(sourceTurnId))
            {
                throw new ArgumentException(
                    "Conversation-summary source-turn identities must be non-null and unique.",
                    nameof(sourceTurnIds));
            }

            retainedSourceTurnIds.Add(sourceTurnId);
            if (retainedSourceTurnIds.Count > MaximumSourceTurnCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(sourceTurnIds),
                    retainedSourceTurnIds.Count,
                    $"A conversation summary accepts at most {MaximumSourceTurnCount} source turns.");
            }
        }

        if (retainedSourceTurnIds.Count == 0)
        {
            throw new ArgumentException(
                "A conversation summary requires at least one source turn.",
                nameof(sourceTurnIds));
        }

        SummaryId = summaryId;
        CharacterId = characterId;
        SourceTurnIds = retainedSourceTurnIds.AsReadOnly();
        Text = AuthoredCharacterText.RequireNonBlank(text, MaximumTextLength, nameof(text));
    }

    /// <summary>Gets the exact canonical summary identity.</summary>
    public CharacterConversationSummaryId SummaryId { get; }

    /// <summary>Gets the exact identity of the character retaining this summary.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact completed source-turn identities in stable supplied order.</summary>
    public IReadOnlyList<CharacterTurnId> SourceTurnIds { get; }

    /// <summary>Gets bounded summary text exactly as supplied.</summary>
    public string Text { get; }
}
