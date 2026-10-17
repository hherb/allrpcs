using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryIndexTests
{
    [Fact]
    public void EntryCopiesTagsAndRetainsExactReferenceInStableOrder()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryReference reference = EpisodicReference(characterId, "memory");
        CharacterMemoryTag topic = new(CharacterMemoryTagKind.Topic, "moonwort");
        CharacterMemoryTag entity = new(CharacterMemoryTagKind.Entity, "seeker");
        CharacterMemoryTag location = new(CharacterMemoryTagKind.Location, "herb-patch");
        CharacterMemoryTag relationship = new(CharacterMemoryTagKind.Relationship, "seeker");
        CharacterMemoryTag[] tags = [topic, entity, location, relationship];

        CharacterMemoryIndexEntry entry = new(reference, tags);
        ListIdentityContractTests.AssertIdentity(entry);
        tags[0] = entity;

        Assert.Same(reference, entry.MemoryReference);
        Assert.Collection(
            entry.Tags,
            tag => Assert.Same(topic, tag),
            tag => Assert.Same(entity, tag),
            tag => Assert.Same(location, tag),
            tag => Assert.Same(relationship, tag));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryTag>)entry.Tags).Add(topic));
    }

    [Fact]
    public void EntryAcceptsExactTagBoundAndRejectsInvalidTags()
    {
        CharacterMemoryReference reference = EpisodicReference(new CharacterId("healer"), "memory");
        CharacterMemoryTag[] maximum = Enumerable
            .Range(0, CharacterMemoryIndexEntry.MaximumTagCount)
            .Select(index => new CharacterMemoryTag(CharacterMemoryTagKind.Topic, $"topic-{index}"))
            .ToArray();

        Assert.Equal(
            CharacterMemoryIndexEntry.MaximumTagCount,
            new CharacterMemoryIndexEntry(reference, maximum).Tags.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterMemoryIndexEntry(
            reference,
            maximum.Append(new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "extra"))));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndexEntry(reference, []));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndexEntry(
            reference,
            [maximum[0], new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "topic-0")]));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndexEntry(reference, [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryIndexEntry(null!, maximum));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryIndexEntry(reference, null!));
    }

    [Fact]
    public void IndexPreservesExactEntriesAndCategorySpecificIdentityOrder()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryIndexEntry episodic = Entry(EpisodicReference(characterId, "shared"));
        CharacterMemoryIndexEntry epistemic = Entry(EpistemicReference(characterId, "shared"));
        CharacterMemoryIndexEntry relationship = Entry(RelationshipReference(characterId, "seeker"));
        CharacterMemoryIndexEntry commitment = Entry(CommitmentReference(characterId, "shared"));
        CharacterMemoryIndexEntry[] entries = [episodic, epistemic, relationship, commitment];

        CharacterMemoryIndex index = new(characterId, entries);
        entries[0] = commitment;

        Assert.Same(characterId, index.CharacterId);
        Assert.Collection(
            index.Entries,
            entry => Assert.Same(episodic, entry),
            entry => Assert.Same(epistemic, entry),
            entry => Assert.Same(relationship, entry),
            entry => Assert.Same(commitment, entry));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryIndexEntry>)index.Entries).Add(episodic));
    }

    [Fact]
    public void IndexRejectsDuplicateMemoryIdentityAndMismatchedOwner()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryIndexEntry first = Entry(EpisodicReference(characterId, "memory"));
        CharacterMemoryIndexEntry duplicate = Entry(EpisodicReference(characterId, "memory"));
        CharacterMemoryIndexEntry other = Entry(EpisodicReference(new CharacterId("hermit"), "other"));

        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndex(characterId, [first, duplicate]));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndex(characterId, [other]));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryIndex(characterId, [null!]));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryIndex(null!, []));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryIndex(characterId, null!));
    }

    [Fact]
    public void IndexAcceptsEmptyAndExactEntryBound()
    {
        CharacterId characterId = new("healer");
        CharacterMemoryIndexEntry[] maximum = Enumerable
            .Range(0, CharacterMemoryIndex.MaximumEntryCount)
            .Select(index => Entry(EpistemicReference(characterId, $"memory-{index}")))
            .ToArray();

        Assert.Empty(new CharacterMemoryIndex(characterId, []).Entries);
        Assert.Equal(
            CharacterMemoryIndex.MaximumEntryCount,
            new CharacterMemoryIndex(characterId, maximum).Entries.Count);
        Assert.Throws<ArgumentOutOfRangeException>(() => new CharacterMemoryIndex(
            characterId,
            maximum.Append(Entry(EpistemicReference(characterId, "extra")))));
    }

    private static CharacterMemoryIndexEntry Entry(CharacterMemoryReference reference) =>
        new(reference, [new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "moonwort")]);

    private static CharacterMemoryReference EpisodicReference(CharacterId characterId, string memoryId)
    {
        CharacterEpisodicMemory memory = CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId],
            "A memory.",
            memoryId);
        return new(CharacterMemoryCategory.Episodic, memory, null, null, null);
    }

    private static CharacterMemoryReference EpistemicReference(CharacterId characterId, string memoryId)
    {
        CharacterEpistemicMemory memory = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            memoryId: memoryId,
            characterId: characterId);
        return new(CharacterMemoryCategory.Epistemic, null, memory, null, null);
    }

    private static CharacterMemoryReference RelationshipReference(
        CharacterId characterId,
        string relatedCharacterId)
    {
        CharacterRelationshipMemory memory =
            CharacterRelationshipMemoryTests.Relationship(characterId, relatedCharacterId);
        return new(CharacterMemoryCategory.Relationship, null, null, memory, null);
    }

    private static CharacterMemoryReference CommitmentReference(CharacterId characterId, string memoryId)
    {
        CharacterCommitmentMemory memory = CharacterCommitmentMemoryTests.Commitment(
            characterId: characterId,
            commitmentId: memoryId);
        return new(CharacterMemoryCategory.Commitment, null, null, null, memory);
    }
}
