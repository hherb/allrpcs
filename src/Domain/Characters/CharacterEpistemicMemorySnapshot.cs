using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains one immutable bounded ordered epistemic-memory set for a character.</summary>
public sealed class CharacterEpistemicMemorySnapshot
{
    /// <summary>Gets the maximum number of epistemic memories retained per character.</summary>
    public const int MaximumMemoryCount = 256;

    /// <summary>Creates a per-character snapshot in stable supplied order.</summary>
    public CharacterEpistemicMemorySnapshot(
        CharacterId characterId,
        IEnumerable<CharacterEpistemicMemory> memories)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(memories);

        List<CharacterEpistemicMemory> retainedMemories = new();
        HashSet<EpistemicMemoryId> memoryIds = new();
        foreach (CharacterEpistemicMemory memory in memories)
        {
            if (memory is null)
            {
                throw new ArgumentException(
                    "Epistemic-memory snapshots cannot contain null values.",
                    nameof(memories));
            }

            if (memory.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every epistemic memory must belong to the snapshot character.",
                    nameof(memories));
            }

            if (!memoryIds.Add(memory.MemoryId))
            {
                throw new ArgumentException(
                    "Epistemic-memory identifiers must be unique within a character snapshot.",
                    nameof(memories));
            }

            retainedMemories.Add(memory);
            if (retainedMemories.Count > MaximumMemoryCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(memories),
                    retainedMemories.Count,
                    $"A character accepts at most {MaximumMemoryCount} epistemic memories.");
            }
        }

        CharacterId = characterId;
        Memories = retainedMemories.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact memory instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterEpistemicMemory> Memories { get; }
}
