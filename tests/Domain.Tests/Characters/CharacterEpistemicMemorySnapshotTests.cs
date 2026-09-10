using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterEpistemicMemorySnapshotTests
{
    [Fact]
    public void EmptySnapshotRetainsExactCharacter()
    {
        CharacterId characterId = new("healer");

        CharacterEpistemicMemorySnapshot snapshot = new(characterId, []);

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Empty(snapshot.Memories);
    }

    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterId characterId = new("healer");
        CharacterEpistemicMemory fact = Memory(
            characterId,
            "fact",
            CharacterEpistemicMemoryCategory.ConfirmedFact);
        CharacterEpistemicMemory claim = Memory(
            characterId,
            "claim",
            CharacterEpistemicMemoryCategory.AttributedClaim);
        CharacterEpistemicMemory belief = Memory(
            characterId,
            "belief",
            CharacterEpistemicMemoryCategory.UncertainBelief);
        CharacterEpistemicMemory[] source = [fact, claim, belief];

        CharacterEpistemicMemorySnapshot snapshot = new(characterId, source);
        source[0] = belief;

        Assert.Collection(
            snapshot.Memories,
            memory => Assert.Same(fact, memory),
            memory => Assert.Same(claim, memory),
            memory => Assert.Same(belief, memory));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterEpistemicMemory>)snapshot.Memories).Add(fact));
    }

    [Fact]
    public void AcceptsExactCountBoundAndRejectsInvalidEntries()
    {
        CharacterId characterId = new("healer");
        CharacterEpistemicMemory[] maximum = Enumerable
            .Range(0, CharacterEpistemicMemorySnapshot.MaximumMemoryCount)
            .Select(index => Memory(
                characterId,
                $"memory-{index}",
                CharacterEpistemicMemoryCategory.ConfirmedFact))
            .ToArray();
        CharacterEpistemicMemory duplicate = Memory(
            characterId,
            "memory-0",
            CharacterEpistemicMemoryCategory.UncertainBelief);
        CharacterEpistemicMemory other = Memory(
            new CharacterId("hermit"),
            "other",
            CharacterEpistemicMemoryCategory.ConfirmedFact);

        Assert.Equal(
            CharacterEpistemicMemorySnapshot.MaximumMemoryCount,
            new CharacterEpistemicMemorySnapshot(characterId, maximum).Memories.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterEpistemicMemorySnapshot(
            characterId,
            maximum.Append(Memory(characterId, "extra", CharacterEpistemicMemoryCategory.ConfirmedFact))));
        Assert.Throws<ArgumentException>(() => new CharacterEpistemicMemorySnapshot(
            characterId,
            [maximum[0], duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterEpistemicMemorySnapshot(characterId, [other]));
        Assert.Throws<ArgumentException>(() => new CharacterEpistemicMemorySnapshot(characterId, [null!]));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemorySnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterEpistemicMemorySnapshot(
            new CharacterId("healer"),
            null!));
    }

    private static CharacterEpistemicMemory Memory(
        CharacterId characterId,
        string memoryId,
        CharacterEpistemicMemoryCategory category) =>
        CharacterEpistemicMemoryTests.Memory(
            category,
            ReferenceEquals(category, CharacterEpistemicMemoryCategory.AttributedClaim)
                ? new CharacterId("seeker")
                : null,
            memoryId: memoryId,
            characterId: characterId);
}
