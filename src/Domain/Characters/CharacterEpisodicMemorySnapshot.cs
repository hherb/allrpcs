using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Contains one immutable bounded ordered set of episodic memories for a character.</summary>
public sealed class CharacterEpisodicMemorySnapshot
{
    /// <summary>Gets the maximum number of significant memories retained per character.</summary>
    public const int MaximumMemoryCount = 128;

    /// <summary>Creates a per-character snapshot in stable supplied order.</summary>
    public CharacterEpisodicMemorySnapshot(
        CharacterId characterId,
        IEnumerable<CharacterEpisodicMemory> memories)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(memories);

        List<CharacterEpisodicMemory> retainedMemories = new();
        HashSet<EpisodicMemoryId> memoryIds = new();
        foreach (CharacterEpisodicMemory memory in memories)
        {
            if (memory is null)
            {
                throw new ArgumentException(
                    "Episodic-memory snapshots cannot contain null values.",
                    nameof(memories));
            }

            if (memory.CharacterId != characterId)
            {
                throw new ArgumentException(
                    "Every episodic memory must belong to the snapshot character.",
                    nameof(memories));
            }

            if (!memoryIds.Add(memory.MemoryId))
            {
                throw new ArgumentException(
                    "Episodic-memory identifiers must be unique within a character snapshot.",
                    nameof(memories));
            }

            retainedMemories.Add(memory);
            if (retainedMemories.Count > MaximumMemoryCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(memories),
                    retainedMemories.Count,
                    $"A character accepts at most {MaximumMemoryCount} episodic memories.");
            }
        }

        CharacterId = characterId;
        Memories = retainedMemories.AsReadOnly();
    }

    /// <summary>Gets the exact character identity supplied for this snapshot.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets exact memory instances in stable supplied order.</summary>
    public IReadOnlyList<CharacterEpisodicMemory> Memories { get; }
}
