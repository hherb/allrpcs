namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains a bounded stable sequence of provider-neutral character-model context entries.
/// </summary>
public sealed class CharacterModelContextSnapshot
{
    /// <summary>Gets the maximum number of entries in one model context.</summary>
    public const int MaximumEntryCount = 128;

    /// <summary>Gets the maximum combined context length in UTF-16 code units.</summary>
    public const int MaximumTotalTextLength = 32_000;

    /// <summary>
    /// Creates an immutable context snapshot in stable supplied order.
    /// </summary>
    public CharacterModelContextSnapshot(IEnumerable<CharacterModelContextEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<CharacterModelContextEntry> retainedEntries = new();
        int totalTextLength = 0;

        foreach (CharacterModelContextEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Model context cannot contain null entries.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
            totalTextLength += entry.Text.Length;

            if (retainedEntries.Count > MaximumEntryCount
                || totalTextLength > MaximumTotalTextLength)
            {
                throw new ArgumentException(
                    $"Model context cannot exceed {MaximumEntryCount} entries or "
                        + $"{MaximumTotalTextLength} UTF-16 code units.",
                    nameof(entries));
            }
        }

        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets exact context entries in stable supplied order.
    /// </summary>
    public IReadOnlyList<CharacterModelContextEntry> Entries { get; }
}
