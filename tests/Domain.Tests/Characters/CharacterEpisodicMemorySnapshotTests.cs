using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterEpisodicMemorySnapshotTests
{
    [Fact]
    public void EmptySnapshotIsValidAndRetainsExactCharacter()
    {
        CharacterId characterId = new("healer");

        CharacterEpisodicMemorySnapshot snapshot = new(characterId, []);

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Empty(snapshot.Memories);
    }

    [Fact]
    public void PreservesOrderExactInstancesAndCopiesTheCollection()
    {
        CharacterId characterId = new("healer");
        CharacterEpisodicMemory first = Memory(characterId, "first");
        CharacterEpisodicMemory second = Memory(characterId, "second");
        CharacterEpisodicMemory[] source = [first, second];

        CharacterEpisodicMemorySnapshot snapshot = new(characterId, source);
        source[0] = second;

        Assert.Collection(
            snapshot.Memories,
            memory => Assert.Same(first, memory),
            memory => Assert.Same(second, memory));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterEpisodicMemory>)snapshot.Memories).Add(first));
    }

    [Fact]
    public void AcceptsExactCountBoundAndRejectsInvalidEntries()
    {
        CharacterId healerId = new("healer");
        CharacterEpisodicMemory[] maximum = Enumerable
            .Range(0, CharacterEpisodicMemorySnapshot.MaximumMemoryCount)
            .Select(index => Memory(healerId, $"memory-{index}"))
            .ToArray();
        CharacterEpisodicMemory duplicate = Memory(healerId, "memory-0");
        CharacterEpisodicMemory other = Memory(new CharacterId("hermit"), "other");

        Assert.Equal(
            CharacterEpisodicMemorySnapshot.MaximumMemoryCount,
            new CharacterEpisodicMemorySnapshot(healerId, maximum).Memories.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterEpisodicMemorySnapshot(
            healerId,
            maximum.Append(Memory(healerId, "extra"))));
        Assert.Throws<ArgumentException>(() => new CharacterEpisodicMemorySnapshot(
            healerId,
            [maximum[0], duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterEpisodicMemorySnapshot(healerId, [other]));
        Assert.Throws<ArgumentException>(() => new CharacterEpisodicMemorySnapshot(healerId, [null!]));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemorySnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpisodicMemorySnapshot(
            new CharacterId("healer"),
            null!));
    }

    private static CharacterEpisodicMemory Memory(CharacterId characterId, string memoryId) =>
        CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId, new CharacterId("seeker")],
            "A significant local memory.",
            memoryId);
}
