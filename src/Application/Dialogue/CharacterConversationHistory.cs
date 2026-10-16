using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Dialogue;

/// <summary>Retains bounded session speech with the audience captured when it occurred.</summary>
public sealed class CharacterConversationHistory
{
    /// <summary>Gets the maximum retained world speech occurrences.</summary>
    public const int MaximumRetainedEvents = 128;

    /// <summary>Gets the maximum retained utterance text length.</summary>
    public const int MaximumRetainedTextLength = 64_000;

    /// <summary>Gets the maximum historical speech entries supplied to one turn.</summary>
    public const int MaximumRecentEvents = 24;

    /// <summary>Gets the maximum historical utterance text supplied to one turn.</summary>
    public const int MaximumRecentTextLength = 12_000;

    private readonly Queue<DialogueEvent> events = new();
    private int textLength;

    /// <summary>Records one exact occurrence; repeated words remain separate occurrences.</summary>
    public void Record(DialogueEvent dialogueEvent)
    {
        ArgumentNullException.ThrowIfNull(dialogueEvent);
        events.Enqueue(dialogueEvent);
        textLength += dialogueEvent.Utterance.Text.Length;
        while (events.Count > MaximumRetainedEvents || textLength > MaximumRetainedTextLength)
        {
            textLength -= events.Dequeue().Utterance.Text.Length;
        }
    }

    /// <summary>Returns a chronological suffix of heard/own speech, excluding the current event.</summary>
    public IReadOnlyList<DialogueEvent> RecentFor(CharacterId characterId, DialogueEvent current)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(current);
        List<DialogueEvent> selected = new();
        int selectedLength = 0;
        foreach (DialogueEvent entry in events.Reverse())
        {
            if (ReferenceEquals(entry, current)
                || (entry.Utterance.SpeakerCharacterId != characterId
                    && !entry.Audience.ListenerCharacterIds.Contains(characterId)))
            {
                continue;
            }

            if (selected.Count == MaximumRecentEvents
                || selectedLength + entry.Utterance.Text.Length > MaximumRecentTextLength)
            {
                break;
            }

            selected.Add(entry);
            selectedLength += entry.Utterance.Text.Length;
        }

        selected.Reverse();
        return selected.AsReadOnly();
    }

    /// <summary>Discards session context at a successful world restoration boundary.</summary>
    public void Clear()
    {
        events.Clear();
        textLength = 0;
    }
}
