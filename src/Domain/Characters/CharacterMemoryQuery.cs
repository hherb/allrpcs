using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Defines one bounded deterministic all-tags query for a character memory index.</summary>
public sealed class CharacterMemoryQuery
{
    /// <summary>Gets the maximum tags accepted by one query.</summary>
    public const int MaximumTagCount = 8;

    /// <summary>Gets the largest result count one query can request.</summary>
    public const int MaximumAllowedResultCount = 32;

    /// <summary>Creates one immutable per-character tag query.</summary>
    public CharacterMemoryQuery(
        CharacterId characterId,
        IEnumerable<CharacterMemoryTag> tags,
        int maximumResultCount)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(tags);
        if (maximumResultCount is < 1 or > MaximumAllowedResultCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumResultCount),
                maximumResultCount,
                $"A memory query result limit must be between 1 and {MaximumAllowedResultCount}.");
        }

        List<CharacterMemoryTag> retainedTags = new();
        HashSet<CharacterMemoryTag> tagSet = new();
        foreach (CharacterMemoryTag tag in tags)
        {
            if (tag is null || !tagSet.Add(tag))
            {
                throw new ArgumentException(
                    "Memory-query tags must be non-null and unique.",
                    nameof(tags));
            }

            retainedTags.Add(tag);
            if (retainedTags.Count > MaximumTagCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tags),
                    retainedTags.Count,
                    $"A memory query accepts at most {MaximumTagCount} tags.");
            }
        }

        if (retainedTags.Count == 0)
        {
            throw new ArgumentException("A memory query requires at least one tag.", nameof(tags));
        }

        CharacterId = characterId;
        Tags = retainedTags.AsReadOnly();
        MaximumResultCount = maximumResultCount;
    }

    /// <summary>Gets the exact character identity supplied for this query.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact required tags in stable supplied order.</summary>
    public IReadOnlyList<CharacterMemoryTag> Tags { get; }

    /// <summary>Gets the positive bounded maximum number of returned entries.</summary>
    public int MaximumResultCount { get; }
}
