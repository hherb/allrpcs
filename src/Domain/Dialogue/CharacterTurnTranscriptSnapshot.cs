using Allrpcs.Domain.Collections;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Contains an immutable ordered snapshot of structurally distinguished transcript entries.
/// </summary>
public sealed class CharacterTurnTranscriptSnapshot
{
    /// <summary>
    /// Creates a transcript snapshot. Repeated entries remain valid in-world occurrences.
    /// </summary>
    public CharacterTurnTranscriptSnapshot(IEnumerable<CharacterTurnTranscriptEntry> entries)
        : this(entries, null) { }

    internal CharacterTurnTranscriptSnapshot(IEnumerable<CharacterTurnTranscriptEntry> entries, AppendWork? work)
    {
        ArgumentNullException.ThrowIfNull(entries);

        AppendSequence<CharacterTurnTranscriptEntry> retainedEntries = new(work);
        foreach (CharacterTurnTranscriptEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Transcript entries cannot contain null values.",
                    nameof(entries));
            }

            retainedEntries = retainedEntries.Append(entry);
        }

        Entries = retainedEntries;
    }

    private CharacterTurnTranscriptSnapshot(AppendSequence<CharacterTurnTranscriptEntry> entries) => Entries = entries;

    internal CharacterTurnTranscriptSnapshot Append(CharacterTurnTranscriptEntry entry) =>
        new(((AppendSequence<CharacterTurnTranscriptEntry>)Entries).Append(entry));

    /// <summary>Gets transcript entries in their stable supplied order.</summary>
    public IReadOnlyList<CharacterTurnTranscriptEntry> Entries { get; }
}
