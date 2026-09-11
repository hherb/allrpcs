using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Tests.Dialogue;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterMemoryAggregateTests
{
    [Fact]
    public void EmptyAggregateRetainsExactCharacterAndConstituents()
    {
        CharacterId characterId = new("healer");
        CharacterRecentConversationWindow recentConversation = new(new CharacterId("healer"), []);
        CharacterConversationSummarySnapshot summaries = new(new CharacterId("healer"), []);
        CharacterEpisodicMemorySnapshot episodicMemories = new(new CharacterId("healer"), []);
        CharacterEpistemicMemorySnapshot epistemicMemories = new(new CharacterId("healer"), []);
        CharacterBeliefAssessmentSnapshot beliefAssessments = new(new CharacterId("healer"), []);
        CharacterRelationshipMemorySnapshot relationships = new(new CharacterId("healer"), []);
        CharacterCommitmentMemorySnapshot commitments = new(new CharacterId("healer"), []);
        CharacterMemoryIndex memoryIndex = new(new CharacterId("healer"), []);

        CharacterMemoryAggregate aggregate = new(
            characterId,
            recentConversation,
            summaries,
            episodicMemories,
            epistemicMemories,
            beliefAssessments,
            relationships,
            commitments,
            memoryIndex);

        Assert.Same(characterId, aggregate.CharacterId);
        Assert.Same(recentConversation, aggregate.RecentConversation);
        Assert.Same(summaries, aggregate.ConversationSummaries);
        Assert.Same(episodicMemories, aggregate.EpisodicMemories);
        Assert.Same(epistemicMemories, aggregate.EpistemicMemories);
        Assert.Same(beliefAssessments, aggregate.BeliefAssessments);
        Assert.Same(relationships, aggregate.Relationships);
        Assert.Same(commitments, aggregate.Commitments);
        Assert.Same(memoryIndex, aggregate.MemoryIndex);
    }

    [Fact]
    public void CompleteAggregateRetainsEveryExactIndexedMemory()
    {
        CharacterId characterId = new("healer");
        CharacterRecentConversationWindow recentConversation = new(
            new CharacterId("healer"),
            [CharacterRecentConversationWindowTests.Turn("turn-one", "Question?", "Answer.")]);
        CharacterConversationSummarySnapshot summaries = new(
            new CharacterId("healer"),
            [CharacterConversationSummaryTests.Summary(characterId: new CharacterId("healer"))]);
        CharacterEpisodicMemory episodic = CharacterEpisodicMemoryTests.Create(
            new CharacterId("healer"),
            [new CharacterId("healer")],
            "An episode.",
            "shared");
        CharacterEpistemicMemory epistemic = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            memoryId: "shared",
            characterId: new CharacterId("healer"));
        CharacterBeliefAssessment assessment = new(
            CharacterBeliefAssessmentTests.Belief(new CharacterId("healer")),
            6_000,
            [],
            []);
        CharacterRelationshipMemory relationship =
            CharacterRelationshipMemoryTests.Relationship(new CharacterId("healer"), "shared");
        CharacterCommitmentMemory commitment = CharacterCommitmentMemoryTests.Commitment(
            characterId: new CharacterId("healer"),
            commitmentId: "shared");
        CharacterMemoryIndexEntry episodicEntry = Entry(
            new CharacterMemoryReference(CharacterMemoryCategory.Episodic, episodic, null, null, null));
        CharacterMemoryIndexEntry epistemicEntry = Entry(
            new CharacterMemoryReference(CharacterMemoryCategory.Epistemic, null, epistemic, null, null));
        CharacterMemoryIndexEntry relationshipEntry = Entry(
            new CharacterMemoryReference(CharacterMemoryCategory.Relationship, null, null, relationship, null));
        CharacterMemoryIndexEntry commitmentEntry = Entry(
            new CharacterMemoryReference(CharacterMemoryCategory.Commitment, null, null, null, commitment));
        CharacterEpisodicMemorySnapshot episodicMemories = new(new CharacterId("healer"), [episodic]);
        CharacterEpistemicMemorySnapshot epistemicMemories = new(new CharacterId("healer"), [epistemic]);
        CharacterBeliefAssessmentSnapshot beliefAssessments = new(
            new CharacterId("healer"),
            [assessment]);
        CharacterRelationshipMemorySnapshot relationships = new(
            new CharacterId("healer"),
            [relationship]);
        CharacterCommitmentMemorySnapshot commitments = new(
            new CharacterId("healer"),
            [commitment]);
        CharacterMemoryIndex memoryIndex = new(
            new CharacterId("healer"),
            [episodicEntry, epistemicEntry, relationshipEntry, commitmentEntry]);

        CharacterMemoryAggregate aggregate = new(
            characterId,
            recentConversation,
            summaries,
            episodicMemories,
            epistemicMemories,
            beliefAssessments,
            relationships,
            commitments,
            memoryIndex);

        Assert.Same(episodic, aggregate.MemoryIndex.Entries[0].MemoryReference.EpisodicMemory);
        Assert.Same(epistemic, aggregate.MemoryIndex.Entries[1].MemoryReference.EpistemicMemory);
        Assert.Same(relationship, aggregate.MemoryIndex.Entries[2].MemoryReference.RelationshipMemory);
        Assert.Same(commitment, aggregate.MemoryIndex.Entries[3].MemoryReference.CommitmentMemory);
    }

    [Fact]
    public void RejectsMismatchedConstituentOwnership()
    {
        CharacterId characterId = new("healer");
        CharacterId otherCharacterId = new("hermit");
        CharacterMemoryAggregate baseline = Empty(characterId);

        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            new CharacterRecentConversationWindow(otherCharacterId, []),
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            new CharacterConversationSummarySnapshot(otherCharacterId, []),
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            new CharacterEpisodicMemorySnapshot(otherCharacterId, []),
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            new CharacterEpistemicMemorySnapshot(otherCharacterId, []),
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            new CharacterBeliefAssessmentSnapshot(otherCharacterId, []),
            baseline.Relationships,
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            new CharacterRelationshipMemorySnapshot(otherCharacterId, []),
            baseline.Commitments,
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            new CharacterCommitmentMemorySnapshot(otherCharacterId, []),
            baseline.MemoryIndex));
        Assert.Throws<ArgumentException>(() => new CharacterMemoryAggregate(
            characterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            baseline.EpisodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            new CharacterMemoryIndex(otherCharacterId, [])));
    }

    [Fact]
    public void RejectsMissingValueEqualDistinctAndCrossCategoryIndexTargets()
    {
        CharacterId characterId = new("healer");
        CharacterEpisodicMemory retained = CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId],
            "Retained.",
            "shared");
        CharacterEpisodicMemory distinct = CharacterEpisodicMemoryTests.Create(
            characterId,
            [characterId],
            "Retained.",
            "shared");
        CharacterEpistemicMemory wrongCategory = CharacterEpistemicMemoryTests.Memory(
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            null,
            memoryId: "shared",
            characterId: characterId);
        CharacterMemoryAggregate baseline = Empty(characterId);
        CharacterEpisodicMemorySnapshot episodicMemories = new(characterId, [retained]);

        Assert.Throws<ArgumentException>(() => WithIndex(
            baseline,
            episodicMemories,
            Entry(new CharacterMemoryReference(
                CharacterMemoryCategory.Episodic,
                distinct,
                null,
                null,
                null))));
        Assert.Throws<ArgumentException>(() => WithIndex(
            baseline,
            episodicMemories,
            Entry(new CharacterMemoryReference(
                CharacterMemoryCategory.Epistemic,
                null,
                wrongCategory,
                null,
                null))));
        Assert.Throws<ArgumentException>(() => WithIndex(
            baseline,
            new CharacterEpisodicMemorySnapshot(characterId, []),
            Entry(new CharacterMemoryReference(
                CharacterMemoryCategory.Episodic,
                retained,
                null,
                null,
                null))));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyImmutableAggregateSurface()
    {
        CharacterMemoryAggregate aggregate = Empty(new CharacterId("healer"));

        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            null!, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, aggregate.EpistemicMemories, aggregate.BeliefAssessments,
            aggregate.Relationships, aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, null!, aggregate.ConversationSummaries, aggregate.EpisodicMemories,
            aggregate.EpistemicMemories, aggregate.BeliefAssessments, aggregate.Relationships,
            aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, null!, aggregate.EpisodicMemories,
            aggregate.EpistemicMemories, aggregate.BeliefAssessments, aggregate.Relationships,
            aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            null!, aggregate.EpistemicMemories, aggregate.BeliefAssessments, aggregate.Relationships,
            aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, null!, aggregate.BeliefAssessments, aggregate.Relationships,
            aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, aggregate.EpistemicMemories, null!, aggregate.Relationships,
            aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, aggregate.EpistemicMemories, aggregate.BeliefAssessments,
            null!, aggregate.Commitments, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, aggregate.EpistemicMemories, aggregate.BeliefAssessments,
            aggregate.Relationships, null!, aggregate.MemoryIndex));
        Assert.Throws<ArgumentNullException>(() => new CharacterMemoryAggregate(
            aggregate.CharacterId, aggregate.RecentConversation, aggregate.ConversationSummaries,
            aggregate.EpisodicMemories, aggregate.EpistemicMemories, aggregate.BeliefAssessments,
            aggregate.Relationships, aggregate.Commitments, null!));

        Type aggregateType = typeof(CharacterMemoryAggregate);
        string[] expectedProperties =
        [
            "BeliefAssessments",
            "CharacterId",
            "Commitments",
            "ConversationSummaries",
            "EpisodicMemories",
            "EpistemicMemories",
            "MemoryIndex",
            "RecentConversation",
            "Relationships",
        ];
        Assert.Equal(
            expectedProperties,
            aggregateType.GetProperties().Select(property => property.Name).Order().ToArray());
        Assert.All(aggregateType.GetProperties(), property => Assert.False(property.CanWrite));
        Assert.Empty(aggregateType.GetEvents());
        Assert.True(aggregateType.IsSealed);
    }

    private static CharacterMemoryAggregate Empty(CharacterId characterId) => new(
        characterId,
        new CharacterRecentConversationWindow(new CharacterId(characterId.Value), []),
        new CharacterConversationSummarySnapshot(new CharacterId(characterId.Value), []),
        new CharacterEpisodicMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterEpistemicMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterBeliefAssessmentSnapshot(new CharacterId(characterId.Value), []),
        new CharacterRelationshipMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterCommitmentMemorySnapshot(new CharacterId(characterId.Value), []),
        new CharacterMemoryIndex(new CharacterId(characterId.Value), []));

    private static CharacterMemoryAggregate WithIndex(
        CharacterMemoryAggregate baseline,
        CharacterEpisodicMemorySnapshot episodicMemories,
        CharacterMemoryIndexEntry entry) => new(
            baseline.CharacterId,
            baseline.RecentConversation,
            baseline.ConversationSummaries,
            episodicMemories,
            baseline.EpistemicMemories,
            baseline.BeliefAssessments,
            baseline.Relationships,
            baseline.Commitments,
            new CharacterMemoryIndex(baseline.CharacterId, [entry]));

    private static CharacterMemoryIndexEntry Entry(CharacterMemoryReference reference) =>
        new(reference, [new CharacterMemoryTag(CharacterMemoryTagKind.Topic, "moonwort")]);
}
