namespace Allrpcs.Domain.Characters;

/// <summary>Selects exact indexed memories by deterministic all-tag matching.</summary>
public static class CharacterMemoryRetrievalPolicy
{
    /// <summary>Returns the first bounded exact matching entries in stable index order.</summary>
    public static CharacterMemoryIndex Retrieve(
        CharacterMemoryQuery query,
        CharacterMemoryIndex index)
    {
        ArgumentNullException.ThrowIfNull(query);
        ArgumentNullException.ThrowIfNull(index);
        if (query.CharacterId != index.CharacterId)
        {
            throw new ArgumentException(
                "The memory query and index must belong to the same character.",
                nameof(index));
        }

        IEnumerable<CharacterMemoryIndexEntry> matchingEntries = index.Entries
            .Where(entry => query.Tags.All(entry.Tags.Contains))
            .Take(query.MaximumResultCount);
        return new CharacterMemoryIndex(index.CharacterId, matchingEntries);
    }
}
