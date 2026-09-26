using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;

namespace Allrpcs.Infrastructure.Tests.Persistence;

public sealed class SaveSnapshotJsonDeserializerTests
{
    private const string EmptyVersionOneJson =
        "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null}}";

    private const string EmptyVersionTwoJson =
        "{\"schemaVersion\":2,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null},"
        + "\"characterMemory\":{\"characters\":[]}}";

    [Fact]
    public void DeserializeMaterializesCompleteCanonicalDocumentInStableOrder()
    {
        SaveSnapshot source = CreateCompleteSnapshot();
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.NotSame(source, result);
        Assert.Equal(1, result.SchemaVersion.Value);
        Assert.Equal("seeker", result.Discovery.Perception.CharacterId.Value);
        Assert.Collection(
            result.Discovery.Perception.Entries,
            first =>
            {
                Assert.Equal("fallen-log", first.ObjectId.Value);
                Assert.Same(VisibilityState.Noticed, first.VisibilityState);
            },
            second =>
            {
                Assert.Equal("stone-marker", second.ObjectId.Value);
                Assert.Same(VisibilityState.Identified, second.VisibilityState);
            });
        Assert.Equal(
            "fallen-log",
            Assert.Single(result.Discovery.FocusedSearchActivities.Entries)
                .Request.TargetObjectId.Value);
        Assert.Same(
            VisibilityState.Understood,
            Assert.Single(result.Discovery.FocusedSearchOutcomes.Entries)
                .ResultingVisibilityState);
        Assert.Equal(
            "fallen-log",
            Assert.Single(result.Discovery.PassiveNoticingCandidates.Entries)
                .TargetObjectId.Value);
        Assert.Equal(
            "stone-marker-meaning",
            Assert.Single(result.Discovery.ObservationReinterpretationCandidates.Entries)
                .TriggerKnowledgeId.Value);
        Assert.Equal(
            ["greybox-ready", "stone-marker-identified"],
            result.WorldEvents.EventLog.Entries.Select(eventId => eventId.Value));
        Assert.Equal(
            "stone-marker-identified",
            result.WorldEvents.WorldTime.CurrentEventId?.Value);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Fact]
    public void DeserializeMaterializesExplicitEmptyState()
    {
        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(EmptyVersionOneJson);

        Assert.Empty(result.Discovery.Perception.Entries);
        Assert.Empty(result.Discovery.FocusedSearchActivities.Entries);
        Assert.Empty(result.Discovery.FocusedSearchOutcomes.Entries);
        Assert.Empty(result.Discovery.PassiveNoticingCandidates.Entries);
        Assert.Empty(result.Discovery.ObservationReinterpretationCandidates.Entries);
        Assert.Empty(result.WorldEvents.EventLog.Entries);
        Assert.Null(result.WorldEvents.WorldTime.CurrentEventId);
    }

    [Fact]
    public void DeserializeCurrentExecutesVersionOneMigrationAndRetainsMaterializedState()
    {
        FirstPatientSaveState initialFirstPatient = InitialFirstPatient();

        SaveSnapshot result = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            EmptyVersionOneJson,
            initialFirstPatient);

        Assert.Same(SaveSchemaVersion.Map, result.SchemaVersion);
        Assert.Empty(result.CharacterMemory.Characters);
        Assert.Empty(result.Discovery.Perception.Entries);
        Assert.Empty(result.WorldEvents.EventLog.Entries);
        Assert.Same(initialFirstPatient, result.FirstPatient);
        Assert.NotNull(result.Journal);
        Assert.NotNull(result.Map);
    }

    [Fact]
    public void DeserializeCurrentExecutesEveryMigrationToTheCurrentDocument()
    {
        FirstPatientSaveState initialFirstPatient = InitialFirstPatient();

        SaveSnapshot result = SaveSnapshotJsonDeserializer.DeserializeCurrent(
            SaveSnapshotJsonSerializerTests.CompleteCharacterMemoryJson,
            initialFirstPatient);

        Assert.Same(SaveSchemaVersion.Map, result.SchemaVersion);
        Assert.Single(result.CharacterMemory.Characters);
        Assert.Same(initialFirstPatient, result.FirstPatient);
        Assert.NotNull(result.Journal);
        Assert.NotNull(result.Map);
    }

    [Fact]
    public void DeserializeMaterializesCompleteCharacterMemoryWithExactInternalReferences()
    {
        SaveSnapshot baseline = CreateCompleteSnapshot();
        SaveSnapshot source = new(
            SaveSchemaVersion.CharacterMemory,
            baseline.Discovery,
            baseline.WorldEvents,
            SaveSnapshotJsonSerializerTests.CreateCompleteCharacterMemory());
        string serialized = SaveSnapshotJsonSerializer.Serialize(source);

        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(serialized);

        Assert.Same(SaveSchemaVersion.CharacterMemory, result.SchemaVersion);
        CharacterMemoryAggregate aggregate = Assert.Single(result.CharacterMemory.Characters);
        Assert.Equal("healer", aggregate.CharacterId.Value);
        Assert.Equal(2, aggregate.RecentConversation.Turns.Count);
        Assert.Equal(3, aggregate.EpistemicMemories.Memories.Count);
        CharacterBeliefAssessment assessment = Assert.Single(
            aggregate.BeliefAssessments.Assessments);
        Assert.Same(aggregate.EpistemicMemories.Memories[2], assessment.Belief);
        Assert.Same(
            aggregate.EpisodicMemories.Memories[0],
            aggregate.MemoryIndex.Entries[0].MemoryReference.EpisodicMemory);
        Assert.Same(
            aggregate.EpistemicMemories.Memories[2],
            aggregate.MemoryIndex.Entries[1].MemoryReference.EpistemicMemory);
        Assert.Same(
            aggregate.Relationships.Relationships[0],
            aggregate.MemoryIndex.Entries[2].MemoryReference.RelationshipMemory);
        Assert.Same(
            aggregate.Commitments.Commitments[0],
            aggregate.MemoryIndex.Entries[3].MemoryReference.CommitmentMemory);
        Assert.Equal(serialized, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Fact]
    public void DeserializeMaterializesExplicitEmptyCharacterMemory()
    {
        SaveSnapshot result = SaveSnapshotJsonDeserializer.Deserialize(EmptyVersionTwoJson);

        Assert.Same(SaveSchemaVersion.CharacterMemory, result.SchemaVersion);
        Assert.Empty(result.CharacterMemory.Characters);
        Assert.Equal(EmptyVersionTwoJson, SaveSnapshotJsonSerializer.Serialize(result));
    }

    [Theory]
    [InlineData("")]
    [InlineData("[]")]
    [InlineData("{\"schemaVersion\":1}")]
    [InlineData(
        "{\"schemaVersion\":1,\"discovery\":{},\"worldEvents\":{},\"unknown\":true}")]
    [InlineData(
        "{\"schemaVersion\":1,\"schemaVersion\":1,\"discovery\":{},\"worldEvents\":{}}")]
    [InlineData(
        "{\"schemaVersion\":2,\"discovery\":{},\"worldEvents\":{}}")]
    [InlineData(
        "{\"schemaVersion\":3,\"discovery\":{},\"worldEvents\":{},"
        + "\"characterMemory\":{\"characters\":[]}}")]
    [InlineData(
        "{\"schemaVersion\":1.0,\"discovery\":{},\"worldEvents\":{}}")]
    [InlineData(
        "{\"schemaVersion\":1,\"discovery\":null,\"worldEvents\":{}}")]
    public void DeserializeRejectsMalformedUnsupportedOrUnexpectedRoot(string serialized)
    {
        Assert.Throws<ArgumentException>(
            () => SaveSnapshotJsonDeserializer.Deserialize(serialized));
    }

    [Fact]
    public void DeserializeRejectsInvalidCharacterMemoryShapeAndReferences()
    {
        string complete = SaveSnapshotJsonSerializerTests.CompleteCharacterMemoryJson;
        string missingReference = complete.Replace(
            "\"memoryIdentity\":\"found-moonwort\"",
            "\"memoryIdentity\":\"missing-memory\"",
            StringComparison.Ordinal);
        string wrongConfidenceKind = complete.Replace(
            "\"confidenceBasisPoints\":6000",
            "\"confidenceBasisPoints\":\"6000\"",
            StringComparison.Ordinal);
        string duplicateCharacterProperty = complete.Replace(
            "\"characterId\":\"healer\",\"recentConversation\"",
            "\"characterId\":\"healer\",\"characterId\":\"healer\","
                + "\"recentConversation\"",
            StringComparison.Ordinal);
        string versionOneWithMemory = EmptyVersionTwoJson.Replace(
            "\"schemaVersion\":2",
            "\"schemaVersion\":1",
            StringComparison.Ordinal);

        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(missingReference));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(wrongConfidenceKind));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(duplicateCharacterProperty));
        Assert.Throws<ArgumentException>(() =>
            SaveSnapshotJsonDeserializer.Deserialize(versionOneWithMemory));
    }

    [Theory]
    [InlineData(
        "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"Seeker\","
        + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null}}")]
    [InlineData(
        "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[{\"objectId\":\"stone-marker\",\"visibilityState\":\"hidden\"}]},"
        + "\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null}}")]
    [InlineData(
        "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
        + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
        + "\"worldEvents\":{\"eventLog\":[\"greybox-ready\",\"greybox-ready\"],"
        + "\"currentTimeEventId\":null}}")]
    public void DeserializeRejectsInvalidNestedValues(string serialized)
    {
        Assert.Throws<ArgumentException>(
            () => SaveSnapshotJsonDeserializer.Deserialize(serialized));
    }

    [Fact]
    public void DeserializeRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => SaveSnapshotJsonDeserializer.Deserialize(null!));
    }

    [Fact]
    public void DeserializeCurrentRejectsEveryNullInput()
    {
        FirstPatientSaveState initialFirstPatient = InitialFirstPatient();

        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotJsonDeserializer.DeserializeCurrent(null!, initialFirstPatient));
        Assert.Throws<ArgumentNullException>(() =>
            SaveSnapshotJsonDeserializer.DeserializeCurrent(EmptyVersionOneJson, null!));
    }

    private static FirstPatientSaveState InitialFirstPatient() => new(
        RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth,
        new PreparedIngredientBatchSnapshot([]),
        new IngredientExperimentJournalSnapshot([]),
        new ExperimentJournalSnapshot([]));

    private static SaveSnapshot CreateCompleteSnapshot()
    {
        CharacterId owner = new("seeker");
        ObjectId firstTarget = new("fallen-log");
        ObjectId secondTarget = new("stone-marker");
        WorldEventId firstEvent = new("greybox-ready");
        WorldEventId currentTime = new("stone-marker-identified");

        return new SaveSnapshot(
            SaveSchemaVersion.FirstSupported,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(
                    owner,
                    [
                        new WorldObjectVisibility(firstTarget, VisibilityState.Noticed),
                        new WorldObjectVisibility(secondTarget, VisibilityState.Identified),
                    ]),
                new FocusedSearchActivitySnapshot(
                    [new FocusedSearchActivity(new FocusedSearchRequest(owner, firstTarget))]),
                new FocusedSearchOutcomeSnapshot(
                    [
                        new FocusedSearchOutcome(
                            new FocusedSearchRequest(owner, secondTarget),
                            VisibilityState.Understood),
                    ]),
                new PassiveNoticingCandidateSnapshot(
                    [new PassiveNoticingCandidate(owner, firstTarget)]),
                new ObservationReinterpretationCandidateSnapshot(
                    [
                        new ObservationReinterpretationCandidate(
                            owner,
                            secondTarget,
                            new KnowledgeId("stone-marker-meaning")),
                    ])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([firstEvent, currentTime]),
                new WorldTimeSnapshot(currentTime)));
    }
}
