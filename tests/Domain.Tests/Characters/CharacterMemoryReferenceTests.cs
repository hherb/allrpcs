using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryReferenceTests
{
    [Fact]
    public void CategoryVocabularyIsClosedCanonicalAndStable()
    {
        Assert.Collection(
            CharacterMemoryCategory.All,
            category => Assert.Same(CharacterMemoryCategory.Episodic, category),
            category => Assert.Same(CharacterMemoryCategory.Epistemic, category),
            category => Assert.Same(CharacterMemoryCategory.Relationship, category),
            category => Assert.Same(CharacterMemoryCategory.Commitment, category));
        Assert.Same(CharacterMemoryCategory.Episodic, CharacterMemoryCategory.Parse("episodic"));
        Assert.Same(CharacterMemoryCategory.Epistemic, CharacterMemoryCategory.Parse("epistemic"));
        Assert.Same(
            CharacterMemoryCategory.Relationship,
            CharacterMemoryCategory.Parse("relationship"));
        Assert.Same(CharacterMemoryCategory.Commitment, CharacterMemoryCategory.Parse("commitment"));
        Assert.Throws<ArgumentException>(() => CharacterMemoryCategory.Parse("conversation"));
        Assert.Throws<ArgumentNullException>(() => CharacterMemoryCategory.Parse(null!));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<CharacterMemoryCategory>)CharacterMemoryCategory.All)
                .Add(CharacterMemoryCategory.Episodic));
    }

    [Fact]
    public void RetainsExactEpisodicMemory()
    {
        CharacterId characterId = new("healer");
        CharacterEpisodicMemory memory = CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId],
            "A memory.");

        CharacterMemoryReference reference = new(
            CharacterMemoryCategory.Episodic,
            memory,
            null,
            null,
            null);

        Assert.Same(CharacterMemoryCategory.Episodic, reference.Category);
        Assert.Same(characterId, reference.CharacterId);
        Assert.Same(memory, reference.EpisodicMemory);
        Assert.Null(reference.EpistemicMemory);
        Assert.Null(reference.RelationshipMemory);
        Assert.Null(reference.CommitmentMemory);
    }

    [Fact]
    public void RetainsExactEpistemicRelationshipAndCommitmentMemories()
    {
        CharacterId characterId = new("healer");
        CharacterEpistemicMemory epistemic = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            characterId: characterId);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipMemoryTests.Relationship(characterId, "seeker");
        CharacterCommitmentMemory commitment =
            CharacterCommitmentMemoryTests.Commitment(characterId: characterId);

        CharacterMemoryReference epistemicReference = new(
            CharacterMemoryCategory.Epistemic,
            null,
            epistemic,
            null,
            null);
        CharacterMemoryReference relationshipReference = new(
            CharacterMemoryCategory.Relationship,
            null,
            null,
            relationship,
            null);
        CharacterMemoryReference commitmentReference = new(
            CharacterMemoryCategory.Commitment,
            null,
            null,
            null,
            commitment);

        Assert.Same(characterId, epistemicReference.CharacterId);
        Assert.Same(epistemic, epistemicReference.EpistemicMemory);
        Assert.Same(characterId, relationshipReference.CharacterId);
        Assert.Same(relationship, relationshipReference.RelationshipMemory);
        Assert.Same(characterId, commitmentReference.CharacterId);
        Assert.Same(commitment, commitmentReference.CommitmentMemory);
    }

    [Fact]
    public void RejectsMissingExtraAndCrossCategoryReferences()
    {
        CharacterId characterId = new("healer");
        CharacterEpisodicMemory episodic = CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId],
            "A memory.");
        CharacterEpistemicMemory epistemic = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            characterId: characterId);

        Assert.Throws<ArgumentException>(() => new CharacterMemoryReference(
            CharacterMemoryCategory.Episodic,
            null,
            null,
            null,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryReference(
            CharacterMemoryCategory.Episodic,
            episodic,
            epistemic,
            null,
            null));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryReference(
            CharacterMemoryCategory.Epistemic,
            episodic,
            null,
            null,
            null));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryReference(
            null!,
            episodic,
            null,
            null,
            null));
    }
}
