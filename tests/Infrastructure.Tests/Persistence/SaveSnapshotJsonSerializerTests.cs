using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
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
}
