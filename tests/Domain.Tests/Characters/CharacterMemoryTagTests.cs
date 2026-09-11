using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryTagTests
{
    [Fact]
    public void KindVocabularyIsClosedCanonicalAndStable()
    {
        Assert.Collection(
            CharacterMemoryTagKind.All,
            kind => Assert.Same(CharacterMemoryTagKind.Topic, kind),
            kind => Assert.Same(CharacterMemoryTagKind.Entity, kind),
            kind => Assert.Same(CharacterMemoryTagKind.Location, kind),
            kind => Assert.Same(CharacterMemoryTagKind.Relationship, kind));
        Assert.Same(CharacterMemoryTagKind.Topic, CharacterMemoryTagKind.Parse("topic"));
        Assert.Same(CharacterMemoryTagKind.Entity, CharacterMemoryTagKind.Parse("entity"));
        Assert.Same(CharacterMemoryTagKind.Location, CharacterMemoryTagKind.Parse("location"));
        Assert.Same(
            CharacterMemoryTagKind.Relationship,
            CharacterMemoryTagKind.Parse("relationship"));
        Assert.Throws<ArgumentException>(() => CharacterMemoryTagKind.Parse("semantic"));
        Assert.Throws<ArgumentNullException>(() => CharacterMemoryTagKind.Parse(null!));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryTagKind>)CharacterMemoryTagKind.All)
                .Add(CharacterMemoryTagKind.Topic));
    }

    [Theory]
    [MemberData(nameof(TagKinds))]
    public void RetainsExactKindAndCanonicalValue(CharacterMemoryTagKind kind)
    {
        CharacterMemoryTag tag = new(kind, "moonwort-safety");

        Assert.Same(kind, tag.Kind);
        Assert.Equal("moonwort-safety", tag.Value);
        Assert.Equal(CanonicalIdentifierText.MaximumLength, CharacterMemoryTag.MaximumValueLength);
    }

    [Fact]
    public void RejectsInvalidValuesAndNullKind()
    {
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryTag(null!, "moonwort"));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryTag(
            CharacterMemoryTagKind.Topic,
            null!));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryTag(
            CharacterMemoryTagKind.Topic,
            "Moonwort"));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryTag(
            CharacterMemoryTagKind.Topic,
            new string('a', CharacterMemoryTag.MaximumValueLength + 1)));
    }

    public static TheoryData<CharacterMemoryTagKind> TagKinds =>
        new()
        {
            CharacterMemoryTagKind.Topic,
            CharacterMemoryTagKind.Entity,
            CharacterMemoryTagKind.Location,
            CharacterMemoryTagKind.Relationship,
        };
}
