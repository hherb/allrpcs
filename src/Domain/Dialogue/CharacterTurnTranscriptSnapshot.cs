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
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<CharacterTurnTranscriptEntry> retainedEntries = new();
        foreach (CharacterTurnTranscriptEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Transcript entries cannot contain null values.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
        }

        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>Gets transcript entries in their stable supplied order.</summary>
    public IReadOnlyList<CharacterTurnTranscriptEntry> Entries { get; }
}
