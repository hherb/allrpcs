using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryQueryTests
{
    [Fact]
    public void RetainsExactCharacterAndCopiesTagsInStableOrder()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryTag topic = new(CharacterMemoryTagKind.Topic, "moonwort");
        CharacterMemoryTag entity = new(CharacterMemoryTagKind.Entity, "seeker");
        CharacterMemoryTag[] tags = [topic, entity];

        CharacterMemoryQuery query = new(characterId, tags, 12);
        tags[0] = entity;

        Assert.Same(characterId, query.CharacterId);
        Assert.Equal(12, query.MaximumResultCount);
        Assert.Collection(
            query.Tags,
            tag => Assert.Same(topic, tag),
            tag => Assert.Same(entity, tag));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryTag>)query.Tags).Add(topic));
    }

    [Fact]
    public void AcceptsExactBoundsAndRejectsInvalidTags()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryTag[] maximum = Enumerable
            .Range(0, CharacterMemoryQuery.MaximumTagCount)
            .Select(index => new CharacterMemoryTag(CharacterMemoryTagKind.Topic, $"topic-{index}"))
            .ToArray();

        CharacterMemoryQuery query = new(
            characterId,
            maximum,
            CharacterMemoryQuery.MaximumAllowedResultCount);

        Assert.Equal(CharacterMemoryQuery.MaximumTagCount, query.Tags.Count);
        Assert.Equal(CharacterMemoryQuery.MaximumAllowedResultCount, query.MaximumResultCount);
        Assert.Throws<ArgumentException>(() => new CharacterMemoryQuery(characterId, [], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterMemoryQuery(
            characterId,
            maximum.Append(new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "extra")),
            1));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryQuery(
            characterId,
            [maximum[0], new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "topic-0")],
            1));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryQuery(characterId, [null!], 1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(CharacterMemoryQuery.MaximumAllowedResultCount + 1)]
    public void RejectsInvalidResultBounds(int maximumResultCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterMemoryQuery(
            new CharacterId("healer"),
            [new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "moonwort")],
            maximumResultCount));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryQuery(
            null!,
            [new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "moonwort")],
            1));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryQuery(
            new CharacterId("healer"),
            null!,
            1));
    }
}
