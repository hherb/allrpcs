using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains bounded tagged memory references for one character.</summary>
public sealed class CharacterMemoryIndex
{
    /// <summary>Gets the maximum indexed memories retained per character.</summary>
    public const int MaximumEntryCount = 576;

    /// <summary>Creates one immutable per-character memory index.</summary>
    public CharacterMemoryIndex(
        CharacterId characterId,
        IEnumerable<CharacterMemoryIndexEntry> entries)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(entries);

        List<CharacterMemoryIndexEntry> retainedEntries = new();
        HashSet<(CharacterMemoryCategory Category, string Identity)> identities = new();
        foreach (CharacterMemoryIndexEntry entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Character-memory indexes cannot contain null entries.",
                    nameof(entries));
            }

            CharacterMemoryReference reference = entry.MemoryReference;
            if (reference.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every indexed memory must belong to the index character.",
                    nameof(entries));
            }

            if (!identities.Add((reference.Category, reference.IdentityValue)))
            {
                throw new ArgumentException(
                    "An index cannot contain the same category-specific memory identity twice.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
            if (retainedEntries.Count > MaximumEntryCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(entries),
                    retainedEntries.Count,
                    $"A character accepts at most {MaximumEntryCount} indexed memories.");
            }
        }

        CharacterId = characterId;
        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this index.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact tagged entries in stable supplied order.</summary>
    public IReadOnlyList<CharacterMemoryIndexEntry> Entries { get; }
}
