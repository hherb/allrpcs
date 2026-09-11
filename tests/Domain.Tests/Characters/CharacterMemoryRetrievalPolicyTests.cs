using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryRetrievalPolicyTests
{
    [Fact]
    public void RetrievesEveryTagKindByAllTagMatchingInIndexOrder()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryTag topic = Tag(CharacterMemoryTagKind.Topic, "moonwort");
        CharacterMemoryTag entity = Tag(CharacterMemoryTagKind.Entity, "seeker");
        CharacterMemoryTag location = Tag(CharacterMemoryTagKind.Location, "herb-patch");
        CharacterMemoryTag relationship = Tag(CharacterMemoryTagKind.Relationship, "seeker");
        CharacterMemoryIndexEntry topicOnly = Entry(characterId, "topic-only", [topic]);
        CharacterMemoryIndexEntry exact = Entry(
            characterId,
            "exact",
            [topic, entity, location, relationship]);
        CharacterMemoryIndexEntry later = Entry(
            characterId,
            "later",
            [relationship, location, entity, topic]);
        CharacterMemoryIndex index = new(characterId, [topicOnly, exact, later]);
        CharacterMemoryQuery query = new(
            new CharacterId("healer"),
            [topic, entity, location, relationship],
            8);

        CharacterMemoryIndex result = CharacterMemoryRetrievalPolicy.Retrieve(query, index);

        Assert.Same(characterId, result.CharacterId);
        Assert.Collection(
            result.Entries,
            entry => Assert.Same(exact, entry),
            entry => Assert.Same(later, entry));
        Assert.Same(exact.MemoryReference.EpistemicMemory, result.Entries[0].MemoryReference.EpistemicMemory);
        Assert.Collection(
            index.Entries,
            entry => Assert.Same(topicOnly, entry),
            entry => Assert.Same(exact, entry),
            entry => Assert.Same(later, entry));
        Assert.Collection(
            query.Tags,
            tag => Assert.Same(topic, tag),
            tag => Assert.Same(entity, tag),
            tag => Assert.Same(location, tag),
            tag => Assert.Same(relationship, tag));
    }

    [Fact]
    public void DoesNotMatchSameValueWithDifferentKind()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryIndexEntry entity = Entry(
            characterId,
            "entity",
            [Tag(CharacterMemoryTagKind.Entity, "seeker")]);
        CharacterMemoryIndex index = new(characterId, [entity]);
        CharacterMemoryQuery query = new(
            characterId,
            [Tag(CharacterMemoryTagKind.Relationship, "seeker")],
            1);

        CharacterMemoryIndex result = CharacterMemoryRetrievalPolicy.Retrieve(query, index);

        Assert.Empty(result.Entries);
    }

    [Fact]
    public void StopsAtBoundWithoutRankingOrReordering()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryTag topic = Tag(CharacterMemoryTagKind.Topic, "moonwort");
        CharacterMemoryIndexEntry first = Entry(characterId, "first", [topic]);
        CharacterMemoryIndexEntry second = Entry(characterId, "second", [topic]);
        CharacterMemoryIndexEntry third = Entry(characterId, "third", [topic]);
        CharacterMemoryIndex index = new(characterId, [first, second, third]);
        CharacterMemoryQuery query = new(characterId, [topic], 2);

        CharacterMemoryIndex result = CharacterMemoryRetrievalPolicy.Retrieve(query, index);

        Assert.Collection(
            result.Entries,
            entry => Assert.Same(first, entry),
            entry => Assert.Same(second, entry));
    }

    [Fact]
    public void RejectsMismatchedOwnershipAndNulls()
    {
        CharacterMemoryTag topic = Tag(CharacterMemoryTagKind.Topic, "moonwort");
        CharacterMemoryQuery query = new(new CharacterId("healer"), [topic], 1);
        CharacterMemoryIndex index = new(new CharacterId("hermit"), []);

        Assert.Throws<ArgumentException>(() => CharacterMemoryRetrievalPolicy.Retrieve(query, index));
        Assert.Throws<ArgumentNullException>(() => CharacterMemoryRetrievalPolicy.Retrieve(null!, index));
        Assert.Throws<ArgumentNullException>(() => CharacterMemoryRetrievalPolicy.Retrieve(query, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureRetrievalOperation()
    {
        System.Reflection.MethodInfo method = Assert.Single(typeof(CharacterMemoryRetrievalPolicy)
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static));

        Assert.Equal(nameof(CharacterMemoryRetrievalPolicy.Retrieve), method.Name);
        Assert.Equal(typeof(CharacterMemoryIndex), method.ReturnType);
        Assert.Equal(
            new[] { typeof(CharacterMemoryQuery), typeof(CharacterMemoryIndex) },
            method.GetParameters().Select(parameter => parameter.ParameterType));
    }

    private static CharacterMemoryTag Tag(CharacterMemoryTagKind kind, string value) => new(kind, value);

    private static CharacterMemoryIndexEntry Entry(
        CharacterId characterId,
        string memoryId,
        IEnumerable<CharacterMemoryTag> tags)
    {
        CharacterEpistemicMemory memory = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            memoryId: memoryId,
            characterId: characterId);
        CharacterMemoryReference reference = new(
            CharacterMemoryCategory.Epistemic,
            null,
            memory,
            null,
            null);
        return new(reference, tags);
    }
}
