using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
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

public sealed class SaveSnapshotJsonSerializerTests
{
    [Fact]
    public void SerializeWritesStableCompleteVersionOneDocument()
    {
        SaveSnapshot snapshot = CreateCompleteSnapshot();

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);

        Assert.Equal(
            "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
            + "\"entries\":[{\"objectId\":\"fallen-log\",\"visibilityState\":\"noticed\"},"
            + "{\"objectId\":\"stone-marker\",\"visibilityState\":\"identified\"}]},"
            + "\"focusedSearchActivities\":[{\"searcherCharacterId\":\"seeker\","
            + "\"targetObjectId\":\"fallen-log\"}],\"focusedSearchOutcomes\":[{"
            + "\"searcherCharacterId\":\"seeker\",\"targetObjectId\":\"stone-marker\","
            + "\"resultingVisibilityState\":\"understood\"}],\"passiveNoticingCandidates\":[{"
            + "\"observerCharacterId\":\"seeker\",\"targetObjectId\":\"fallen-log\"}],"
            + "\"observationReinterpretationCandidates\":[{\"observerCharacterId\":\"seeker\","
            + "\"targetObjectId\":\"stone-marker\","
            + "\"triggerKnowledgeId\":\"stone-marker-meaning\"}]},\"worldEvents\":{"
            + "\"eventLog\":[\"greybox-ready\",\"stone-marker-identified\"],"
            + "\"currentTimeEventId\":\"stone-marker-identified\"}}",
            serialized);
        AssertSameSnapshot(snapshot);
    }

    [Fact]
    public void SerializeWritesEmptyCollectionsAndNullWorldTimeExplicitly()
    {
        CharacterId owner = new("seeker");
        SaveSnapshot snapshot = new(
            SaveSchemaVersion.FirstSupported,
            new DiscoverySaveState(
                new CharacterPerceptionSnapshot(owner, []),
                new FocusedSearchActivitySnapshot([]),
                new FocusedSearchOutcomeSnapshot([]),
                new PassiveNoticingCandidateSnapshot([]),
                new ObservationReinterpretationCandidateSnapshot([])),
            new WorldEventSaveState(
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null)));

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);

        Assert.Equal(
            "{\"schemaVersion\":1,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
            + "\"entries\":[]},\"focusedSearchActivities\":[],\"focusedSearchOutcomes\":[],"
            + "\"passiveNoticingCandidates\":[],\"observationReinterpretationCandidates\":[]},"
            + "\"worldEvents\":{\"eventLog\":[],\"currentTimeEventId\":null}}",
            serialized);
    }

    [Fact]
    public void SerializeValueEqualSnapshotsIdentically()
    {
        SaveSnapshot first = CreateCompleteSnapshot();
        SaveSnapshot second = CreateCompleteSnapshot();

        Assert.NotSame(first, second);
        Assert.Equal(
            SaveSnapshotJsonSerializer.Serialize(first),
            SaveSnapshotJsonSerializer.Serialize(second));
    }

    [Fact]
    public void SerializeRejectsNull()
    {
        Assert.Throws<ArgumentNullException>(() => SaveSnapshotJsonSerializer.Serialize(null!));
    }

    [Fact]
    public void SerializeRejectsFirstPatientVersionUntilItsJsonContractExists()
    {
        SaveSnapshot baseline = CreateCompleteSnapshot();
        DiseaseTruthSnapshot diseaseTruth =
            RepresentativeFirstPatientTreatmentContextSource.Create().Scenario.DiseaseTruth;
        FirstPatientSaveState firstPatient = new(
            diseaseTruth,
            new PreparedIngredientBatchSnapshot([]),
            new IngredientExperimentJournalSnapshot([]),
            new ExperimentJournalSnapshot([]));
        SaveSnapshot snapshot = new(
            SaveSchemaVersion.FirstPatient,
            baseline.Discovery,
            baseline.WorldEvents,
            new CharacterMemorySnapshot([]),
            firstPatient);

        Assert.Throws<ArgumentException>(() => SaveSnapshotJsonSerializer.Serialize(snapshot));
        Assert.Same(firstPatient, snapshot.FirstPatient);
        Assert.Same(diseaseTruth, firstPatient.DiseaseTruth);
    }

    [Fact]
    public void SerializeWritesEmptyCharacterMemoryVersionExplicitly()
    {
        SaveSnapshot versionTwo = new(
            SaveSchemaVersion.CharacterMemory,
            CreateCompleteSnapshot().Discovery,
            CreateCompleteSnapshot().WorldEvents,
            new CharacterMemorySnapshot([]));

        Assert.Equal(
            "{\"schemaVersion\":2,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
            + "\"entries\":[{\"objectId\":\"fallen-log\",\"visibilityState\":\"noticed\"},"
            + "{\"objectId\":\"stone-marker\",\"visibilityState\":\"identified\"}]},"
            + "\"focusedSearchActivities\":[{\"searcherCharacterId\":\"seeker\","
            + "\"targetObjectId\":\"fallen-log\"}],\"focusedSearchOutcomes\":[{"
            + "\"searcherCharacterId\":\"seeker\",\"targetObjectId\":\"stone-marker\","
            + "\"resultingVisibilityState\":\"understood\"}],\"passiveNoticingCandidates\":[{"
            + "\"observerCharacterId\":\"seeker\",\"targetObjectId\":\"fallen-log\"}],"
            + "\"observationReinterpretationCandidates\":[{\"observerCharacterId\":\"seeker\","
            + "\"targetObjectId\":\"stone-marker\","
            + "\"triggerKnowledgeId\":\"stone-marker-meaning\"}]},\"worldEvents\":{"
            + "\"eventLog\":[\"greybox-ready\",\"stone-marker-identified\"],"
            + "\"currentTimeEventId\":\"stone-marker-identified\"},"
            + "\"characterMemory\":{\"characters\":[]}}",
            SaveSnapshotJsonSerializer.Serialize(versionTwo));
    }

    [Fact]
    public void SerializeWritesStableCompleteCharacterMemoryDocumentWithoutMutatingInputs()
    {
        CharacterMemorySnapshot characterMemory = CreateCompleteCharacterMemory();
        SaveSnapshot baseline = CreateCompleteSnapshot();
        SaveSnapshot snapshot = new(
            SaveSchemaVersion.CharacterMemory,
            baseline.Discovery,
            baseline.WorldEvents,
            characterMemory);

        string serialized = SaveSnapshotJsonSerializer.Serialize(snapshot);

        Assert.Equal(CompleteCharacterMemoryJson, serialized);
        Assert.Same(characterMemory, snapshot.CharacterMemory);
        CharacterMemoryAggregate aggregate = Assert.Single(characterMemory.Characters);
        Assert.Equal(2, aggregate.RecentConversation.Turns.Count);
        Assert.Equal(4, aggregate.MemoryIndex.Entries.Count);
    }

    [Fact]
    public void SerializeWritesValueEqualCharacterMemorySnapshotsIdentically()
    {
        SaveSnapshot baseline = CreateCompleteSnapshot();
        SaveSnapshot first = new(
            SaveSchemaVersion.CharacterMemory,
            baseline.Discovery,
            baseline.WorldEvents,
            CreateCompleteCharacterMemory());
        SaveSnapshot second = new(
            SaveSchemaVersion.CharacterMemory,
            CreateCompleteSnapshot().Discovery,
            CreateCompleteSnapshot().WorldEvents,
            CreateCompleteCharacterMemory());

        Assert.Equal(
            SaveSnapshotJsonSerializer.Serialize(first),
            SaveSnapshotJsonSerializer.Serialize(second));
    }

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

    private static void AssertSameSnapshot(SaveSnapshot snapshot)
    {
        Assert.Same(SaveSchemaVersion.FirstSupported, snapshot.SchemaVersion);
        Assert.Equal(2, snapshot.Discovery.Perception.Entries.Count);
        Assert.Single(snapshot.Discovery.FocusedSearchActivities.Entries);
        Assert.Single(snapshot.Discovery.FocusedSearchOutcomes.Entries);
        Assert.Single(snapshot.Discovery.PassiveNoticingCandidates.Entries);
        Assert.Single(snapshot.Discovery.ObservationReinterpretationCandidates.Entries);
        Assert.Equal(2, snapshot.WorldEvents.EventLog.Entries.Count);
        Assert.NotNull(snapshot.WorldEvents.WorldTime.CurrentEventId);
    }

    internal static CharacterMemorySnapshot CreateCompleteCharacterMemory()
    {
        CharacterId healer = new("healer");
        CharacterEpisodicMemory episodic = new(
            new EpisodicMemoryId("found-moonwort"),
            healer,
            [healer, new CharacterId("seeker")],
            new LocationId("herb-patch"),
            new WorldTimeSnapshot(null),
            new CharacterTurnId("turn-one"),
            "We found moonwort together.");
        CharacterEpistemicMemory fact = Epistemic(
            healer,
            "moonwort-location",
            CharacterEpistemicMemoryCategory.ConfirmedFact,
            "Moonwort grows by the stream.",
            null);
        CharacterEpistemicMemory claim = Epistemic(
            healer,
            "sage-claim",
            CharacterEpistemicMemoryCategory.AttributedClaim,
            "The sage says moonwort cools fever.",
            new CharacterId("sage"));
        CharacterEpistemicMemory belief = Epistemic(
            healer,
            "moonwort-cooling",
            CharacterEpistemicMemoryCategory.UncertainBelief,
            "Moonwort may cool fever.",
            null);
        CharacterRelationshipMemory relationship = new(
            healer,
            new CharacterId("seeker"),
            [
                new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Trust, 25),
                new CharacterRelationshipDimensionValue(CharacterRelationshipDimension.Respect, 40),
            ],
            [
                new CharacterRelationshipReason(
                    new RelationshipReasonId("careful-listening"),
                    CharacterRelationshipDimension.Trust,
                    new CharacterTurnId("turn-one"),
                    "The seeker listened carefully."),
            ]);
        CharacterCommitmentMemory commitment = new(
            new CharacterCommitmentId("inspect-moonwort"),
            healer,
            CharacterCommitmentKind.Promise,
            new CharacterId("seeker"),
            new CharacterTurnId("turn-one"),
            "Inspect the moonwort before collection.");
        CharacterMemoryIndexEntry[] indexEntries =
        [
            IndexEntry(
                new CharacterMemoryReference(
                    CharacterMemoryCategory.Episodic,
                    episodic,
                    null,
                    null,
                    null),
                CharacterMemoryTagKind.Location,
                "herb-patch"),
            IndexEntry(
                new CharacterMemoryReference(
                    CharacterMemoryCategory.Epistemic,
                    null,
                    belief,
                    null,
                    null),
                CharacterMemoryTagKind.Topic,
                "moonwort"),
            IndexEntry(
                new CharacterMemoryReference(
                    CharacterMemoryCategory.Relationship,
                    null,
                    null,
                    relationship,
                    null),
                CharacterMemoryTagKind.Relationship,
                "seeker"),
            IndexEntry(
                new CharacterMemoryReference(
                    CharacterMemoryCategory.Commitment,
                    null,
                    null,
                    null,
                    commitment),
                CharacterMemoryTagKind.Entity,
                "moonwort"),
        ];
        CharacterMemoryAggregate aggregate = new(
            healer,
            new CharacterRecentConversationWindow(
                healer,
                [
                    RecentTurn("turn-one", "Will you inspect it?", "I will.", true),
                    RecentTurn("turn-two", "Please wait.", null, false),
                ]),
            new CharacterConversationSummarySnapshot(
                healer,
                [
                    new CharacterConversationSummary(
                        new CharacterConversationSummaryId("first-meeting"),
                        healer,
                        [new CharacterTurnId("turn-one"), new CharacterTurnId("turn-two")],
                        "The seeker asked for help with moonwort."),
                ]),
            new CharacterEpisodicMemorySnapshot(healer, [episodic]),
            new CharacterEpistemicMemorySnapshot(healer, [fact, claim, belief]),
            new CharacterBeliefAssessmentSnapshot(
                healer,
                [
                    new CharacterBeliefAssessment(
                        belief,
                        6_000,
                        [new EvidenceId("cool-leaf")],
                        [new EvidenceId("warm-tea")]),
                ]),
            new CharacterRelationshipMemorySnapshot(healer, [relationship]),
            new CharacterCommitmentMemorySnapshot(healer, [commitment]),
            new CharacterMemoryIndex(healer, indexEntries));
        return new CharacterMemorySnapshot([aggregate]);
    }

    private static CharacterEpistemicMemory Epistemic(
        CharacterId characterId,
        string memoryId,
        CharacterEpistemicMemoryCategory category,
        string statement,
        CharacterId? attributedSpeaker) => new(
            new EpistemicMemoryId(memoryId),
            characterId,
            category,
            new CharacterTurnId("turn-one"),
            statement,
            attributedSpeaker);

    private static CharacterRecentConversationTurn RecentTurn(
        string turnId,
        string heardText,
        string? responseText,
        bool includeDetails)
    {
        CharacterId seeker = new("seeker");
        CharacterId healer = new("healer");
        DialogueUtterance utterance = new(seeker, heardText);
        CharacterTurnRequest request = new(
            healer,
            new DialogueEvent(
                utterance,
                new DialogueAudienceSnapshot(seeker, [healer, new CharacterId("hermit")])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            includeDetails
                ? CharacterTurnDisposition.ConditionallyAccept
                : CharacterTurnDisposition.Accept,
            responseText is null ? null : new DialogueUtterance(healer, responseText),
            new CharacterIntention(
                includeDetails ? CharacterIntentionType.ExamineObject : CharacterIntentionType.Wait,
                includeDetails ? new ObjectId("moonwort-patch") : null),
            includeDetails
                ? [
                    new CharacterCommitment(
                        [new CharacterIntention(CharacterIntentionType.Wait, null)]),
                ]
                : [],
            includeDetails ? [new CharacterStateProposal("working-theory", "Moonwort may help.")] : []);
        return new CharacterRecentConversationTurn(
            new CharacterTurnId(turnId),
            new CharacterTurnActivity(request, response, CharacterTurnActivityState.Completed));
    }

    private static CharacterMemoryIndexEntry IndexEntry(
        CharacterMemoryReference reference,
        CharacterMemoryTagKind tagKind,
        string tagValue) => new(
            reference,
            [new CharacterMemoryTag(tagKind, tagValue)]);

    internal const string CompleteCharacterMemoryJson =
        "{\"schemaVersion\":2,\"discovery\":{\"perception\":{\"characterId\":\"seeker\","
        + "\"entries\":[{\"objectId\":\"fallen-log\",\"visibilityState\":\"noticed\"},"
        + "{\"objectId\":\"stone-marker\",\"visibilityState\":\"identified\"}]},"
        + "\"focusedSearchActivities\":[{\"searcherCharacterId\":\"seeker\","
        + "\"targetObjectId\":\"fallen-log\"}],\"focusedSearchOutcomes\":[{"
        + "\"searcherCharacterId\":\"seeker\",\"targetObjectId\":\"stone-marker\","
        + "\"resultingVisibilityState\":\"understood\"}],\"passiveNoticingCandidates\":[{"
        + "\"observerCharacterId\":\"seeker\",\"targetObjectId\":\"fallen-log\"}],"
        + "\"observationReinterpretationCandidates\":[{\"observerCharacterId\":\"seeker\","
        + "\"targetObjectId\":\"stone-marker\","
        + "\"triggerKnowledgeId\":\"stone-marker-meaning\"}]},\"worldEvents\":{"
        + "\"eventLog\":[\"greybox-ready\",\"stone-marker-identified\"],"
        + "\"currentTimeEventId\":\"stone-marker-identified\"},\"characterMemory\":{"
        + "\"characters\":[{\"characterId\":\"healer\",\"recentConversation\":[{"
        + "\"turnId\":\"turn-one\",\"heard\":{\"speakerCharacterId\":\"seeker\","
        + "\"listenerCharacterIds\":[\"healer\",\"hermit\"],"
        + "\"text\":\"Will you inspect it?\",\"dialogueAct\":\"request\"},\"response\":{"
        + "\"disposition\":\"conditionally-accept\",\"speech\":\"I will.\","
        + "\"immediateIntention\":{\"type\":\"examine-object\","
        + "\"targetObjectId\":\"moonwort-patch\"},\"commitments\":[[{\"type\":\"wait\","
        + "\"targetObjectId\":null}]],\"stateProposals\":[{\"key\":\"working-theory\","
        + "\"value\":\"Moonwort may help.\"}]}},{\"turnId\":\"turn-two\",\"heard\":{"
        + "\"speakerCharacterId\":\"seeker\",\"listenerCharacterIds\":[\"healer\",\"hermit\"],"
        + "\"text\":\"Please wait.\",\"dialogueAct\":\"request\"},\"response\":{"
        + "\"disposition\":\"accept\",\"speech\":null,\"immediateIntention\":{\"type\":\"wait\","
        + "\"targetObjectId\":null},\"commitments\":[],\"stateProposals\":[]}}],"
        + "\"conversationSummaries\":[{\"summaryId\":\"first-meeting\","
        + "\"sourceTurnIds\":[\"turn-one\",\"turn-two\"],"
        + "\"text\":\"The seeker asked for help with moonwort.\"}],\"episodicMemories\":[{"
        + "\"memoryId\":\"found-moonwort\",\"participantCharacterIds\":[\"healer\",\"seeker\"],"
        + "\"locationId\":\"herb-patch\",\"currentTimeEventId\":null,"
        + "\"sourceTurnId\":\"turn-one\",\"summary\":\"We found moonwort together.\"}],"
        + "\"epistemicMemories\":[{\"memoryId\":\"moonwort-location\","
        + "\"category\":\"confirmed-fact\",\"sourceTurnId\":\"turn-one\","
        + "\"statement\":\"Moonwort grows by the stream.\",\"attributedSpeakerCharacterId\":null},{"
        + "\"memoryId\":\"sage-claim\",\"category\":\"attributed-claim\","
        + "\"sourceTurnId\":\"turn-one\",\"statement\":\"The sage says moonwort cools fever.\","
        + "\"attributedSpeakerCharacterId\":\"sage\"},{\"memoryId\":\"moonwort-cooling\","
        + "\"category\":\"uncertain-belief\",\"sourceTurnId\":\"turn-one\","
        + "\"statement\":\"Moonwort may cool fever.\",\"attributedSpeakerCharacterId\":null}],"
        + "\"beliefAssessments\":[{\"beliefMemoryId\":\"moonwort-cooling\","
        + "\"confidenceBasisPoints\":6000,\"supportingEvidenceIds\":[\"cool-leaf\"],"
        + "\"contradictingEvidenceIds\":[\"warm-tea\"]}],\"relationships\":[{"
        + "\"relatedCharacterId\":\"seeker\",\"dimensions\":[{\"dimension\":\"trust\",\"value\":25},{"
        + "\"dimension\":\"respect\",\"value\":40}],\"reasons\":[{"
        + "\"reasonId\":\"careful-listening\",\"dimension\":\"trust\","
        + "\"sourceTurnId\":\"turn-one\",\"summary\":\"The seeker listened carefully.\"}]}],"
        + "\"commitments\":[{\"commitmentId\":\"inspect-moonwort\",\"kind\":\"promise\","
        + "\"counterpartyCharacterId\":\"seeker\",\"sourceTurnId\":\"turn-one\","
        + "\"description\":\"Inspect the moonwort before collection.\"}],\"memoryIndex\":[{"
        + "\"category\":\"episodic\",\"memoryIdentity\":\"found-moonwort\","
        + "\"tags\":[{\"kind\":\"location\",\"value\":\"herb-patch\"}]},{"
        + "\"category\":\"epistemic\",\"memoryIdentity\":\"moonwort-cooling\","
        + "\"tags\":[{\"kind\":\"topic\",\"value\":\"moonwort\"}]},{"
        + "\"category\":\"relationship\",\"memoryIdentity\":\"seeker\","
        + "\"tags\":[{\"kind\":\"relationship\",\"value\":\"seeker\"}]},{"
        + "\"category\":\"commitment\",\"memoryIdentity\":\"inspect-moonwort\","
        + "\"tags\":[{\"kind\":\"entity\",\"value\":\"moonwort\"}]}]}]}}";
}
