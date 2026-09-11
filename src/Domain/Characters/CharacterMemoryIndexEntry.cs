namespace Allrpcs.Domain.Characters;

/// <summary>Associates one exact existing local memory with bounded deterministic tags.</summary>
public sealed record CharacterMemoryIndexEntry
{
    /// <summary>Gets the maximum tags accepted by one indexed memory.</summary>
    public const int MaximumTagCount = 16;

    /// <summary>Creates one immutable tagged memory-index entry.</summary>
    public CharacterMemoryIndexEntry(
        CharacterMemoryReference memoryReference,
        IEnumerable<CharacterMemoryTag> tags)
    {
        ArgumentNullException.ThrowIfNull(memoryReference);
        ArgumentNullException.ThrowIfNull(tags);

        List<CharacterMemoryTag> retainedTags = new();
        HashSet<CharacterMemoryTag> tagSet = new();
        foreach (CharacterMemoryTag tag in tags)
        {
            if (tag is null || !tagSet.Add(tag))
            {
                throw new ArgumentException(
                    "Memory-index tags must be non-null and unique.",
                    nameof(tags));
            }

            retainedTags.Add(tag);
            if (retainedTags.Count > MaximumTagCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(tags),
                    retainedTags.Count,
                    $"A memory-index entry accepts at most {MaximumTagCount} tags.");
            }
        }

        if (retainedTags.Count == 0)
        {
            throw new ArgumentException("A memory-index entry requires at least one tag.", nameof(tags));
        }

        MemoryReference = memoryReference;
        Tags = retainedTags.AsReadOnly();
    }

    /// <summary>Gets the exact category-safe existing-memory reference.</summary>
    public CharacterMemoryReference MemoryReference { get; }

    /// <summary>Gets exact tags in stable supplied order.</summary>
    public IReadOnlyList<CharacterMemoryTag> Tags { get; }
}
