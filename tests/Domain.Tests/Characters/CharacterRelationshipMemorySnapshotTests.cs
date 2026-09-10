using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterRelationshipMemorySnapshotTests
{
    [Fact]
    public void PreservesOrderExactInstancesAndCopiesCollection()
    {
        CharacterId characterId = new("healer");
        CharacterRelationshipMemory first = Relationship(characterId, "seeker");
        CharacterRelationshipMemory second = Relationship(characterId, "sage");
        CharacterRelationshipMemory[] source = [first, second];

        CharacterRelationshipMemorySnapshot snapshot = new(characterId, source);
        source[0] = second;

        Assert.Same(characterId, snapshot.CharacterId);
        Assert.Collection(
            snapshot.Relationships,
            relationship => Assert.Same(first, relationship),
            relationship => Assert.Same(second, relationship));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterRelationshipMemory>)snapshot.Relationships).Add(first));
    }

    [Fact]
    public void AcceptsEmptyAndExactCountBound()
    {
        CharacterId characterId = new("healer");
        CharacterRelationshipMemory[] maximum = Enumerable
            .Range(0, CharacterRelationshipMemorySnapshot.MaximumRelationshipCount)
            .Select(index => Relationship(characterId, $"character-{index}"))
            .ToArray();

        Assert.Empty(new CharacterRelationshipMemorySnapshot(characterId, []).Relationships);
        Assert.Equal(
            CharacterRelationshipMemorySnapshot.MaximumRelationshipCount,
            new CharacterRelationshipMemorySnapshot(characterId, maximum).Relationships.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterRelationshipMemorySnapshot(
            characterId,
            maximum.Append(Relationship(characterId, "extra"))));
    }

    [Fact]
    public void RejectsInvalidInputs()
    {
        CharacterId characterId = new("healer");
        CharacterRelationshipMemory first = Relationship(characterId, "seeker");
        CharacterRelationshipMemory duplicate = Relationship(characterId, "seeker");
        CharacterRelationshipMemory other = Relationship(new CharacterId("hermit"), "seeker");

        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemorySnapshot(
            characterId,
            [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemorySnapshot(
            characterId,
            [other]));
        Assert.Throws<ArgumentException>(() => new CharacterRelationshipMemorySnapshot(
            characterId,
            [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemorySnapshot(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterRelationshipMemorySnapshot(
            characterId,
            null!));
    }

    private static CharacterRelationshipMemory Relationship(
        CharacterId characterId,
        string relatedCharacterId) =>
        CharacterRelationshipMemoryTests.Relationship(characterId, relatedCharacterId);
}
