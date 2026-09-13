using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterPostExaminationCollectionOccurrencePolicyTests
{
    [Fact]
    public void AllowsExactCollectionAfterTheLatestSafeExaminationResult()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent collectionEvent = Event("healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection = Collection(result, collectionEvent);

        bool canOccur = CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            WorldEvents(result, collectionEvent),
            EventLogWithExamination(result),
            new WorldTimeSnapshot(result.Occurrence.WorldEvent.EventId),
            TranscriptWithExamination(result));

        Assert.True(canOccur);
    }

    [Fact]
    public void RejectsCollectionAfterAnUnsafeResult()
    {
        CharacterExaminationResult safe = SafeResult();
        CharacterExaminationResult unsafeResult = new(
            safe.Occurrence,
            CharacterExaminationSafety.UnsafeToCollect);
        WorldEvent collectionEvent = Event("healer-collected-flowers");

        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            unsafeResult,
            Collection(unsafeResult, collectionEvent),
            WorldEvents(unsafeResult, collectionEvent),
            EventLogWithExamination(unsafeResult),
            new WorldTimeSnapshot(unsafeResult.Occurrence.WorldEvent.EventId),
            TranscriptWithExamination(unsafeResult)));
    }

    [Fact]
    public void RejectsCollectionBeforeOrAfterAnythingOtherThanTheExactExaminationOccurrence()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent collectionEvent = Event("healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection = Collection(result, collectionEvent);
        CharacterTurnTranscriptEntry distinctEqualExamination = new(
            result.Occurrence.TranscriptEntry.CharacterId,
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                result.TargetObjectId));
        CharacterTurnTranscriptEntry laterSpeech = new(
            new DialogueDelivery(
                new DialogueUtterance(new CharacterId("seeker"), "Collect them now."),
                DialogueDeliveryMode.OrdinarySpeech));
        WorldEventSnapshot worldEvents = WorldEvents(result, collectionEvent);

        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([])));
        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([distinctEqualExamination])));
        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot(
                [result.Occurrence.TranscriptEntry, laterSpeech])));
    }

    [Fact]
    public void RejectsADistinctTargetActorOrNonCollectionAction()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent collectionEvent = Event("healer-collected-flowers");
        CharacterId actor = result.Occurrence.TranscriptEntry.CharacterId;
        WorldEventSnapshot worldEvents = WorldEvents(result, collectionEvent);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithExamination(result);
        CharacterTurnWorldEventOccurrence distinctTarget = new(
            collectionEvent,
            new CharacterTurnTranscriptEntry(
                actor,
                new CharacterIntention(
                    CharacterIntentionType.CollectObject,
                    new ObjectId(result.TargetObjectId.Value))));
        CharacterTurnWorldEventOccurrence distinctActor = new(
            collectionEvent,
            new CharacterTurnTranscriptEntry(
                new CharacterId(actor.Value),
                new CharacterIntention(
                    CharacterIntentionType.CollectObject,
                    result.TargetObjectId)));
        CharacterTurnWorldEventOccurrence examineAgain = new(
            collectionEvent,
            new CharacterTurnTranscriptEntry(
                actor,
                new CharacterIntention(
                    CharacterIntentionType.ExamineObject,
                    result.TargetObjectId)));

        Assert.False(CanOccur(result, distinctTarget, worldEvents, transcript));
        Assert.False(CanOccur(result, distinctActor, worldEvents, transcript));
        Assert.False(CanOccur(result, examineAgain, worldEvents, transcript));
    }

    [Fact]
    public void RejectsAnUnrecordedOrDistinctValueEqualExaminationEvent()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent collectionEvent = Event("healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection = Collection(result, collectionEvent);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithExamination(result);
        WorldEvent distinctExamination = Event(result.Occurrence.WorldEvent.EventId.Value);

        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            WorldEvents(result, collectionEvent),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            transcript));
        Assert.Equal(result.Occurrence.WorldEvent, distinctExamination);
        Assert.NotSame(result.Occurrence.WorldEvent, distinctExamination);
        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            new WorldEventSnapshot([distinctExamination, collectionEvent]),
            new WorldEventLogSnapshot([distinctExamination.EventId]),
            new WorldTimeSnapshot(distinctExamination.EventId),
            transcript));
    }

    [Fact]
    public void DelegatesAuthoritativeWorldEventEligibilityAndChangesNothing()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent authoritative = Event("healer-collected-flowers");
        WorldEvent distinctEqual = Event("healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection = Collection(result, distinctEqual);
        WorldEventSnapshot worldEvents = WorldEvents(result, authoritative);
        WorldEventId examinationEventId = result.Occurrence.WorldEvent.EventId;
        WorldEventLogSnapshot eventLog = new([examinationEventId]);
        WorldTimeSnapshot worldTime = new(examinationEventId);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithExamination(result);

        Assert.False(CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            worldEvents,
            eventLog,
            worldTime,
            transcript));
        Assert.Collection(
            worldEvents.Entries,
            entry => Assert.Same(result.Occurrence.WorldEvent, entry),
            entry => Assert.Same(authoritative, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(examinationEventId, entry));
        Assert.Same(examinationEventId, worldTime.CurrentEventId);
        Assert.Collection(
            transcript.Entries,
            entry => Assert.Same(result.Occurrence.TranscriptEntry, entry));
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterExaminationResult result = SafeResult();
        WorldEvent collectionEvent = Event("healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection = Collection(result, collectionEvent);
        WorldEventSnapshot worldEvents = new([collectionEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithExamination(result);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                null!, collection, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result, null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result, collection, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result, collection, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result, collection, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
                result, collection, worldEvents, eventLog, worldTime, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(CharacterPostExaminationCollectionOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            new[]
            {
                typeof(CharacterExaminationResult),
                typeof(CharacterTurnWorldEventOccurrence),
                typeof(WorldEventSnapshot),
                typeof(WorldEventLogSnapshot),
                typeof(WorldTimeSnapshot),
                typeof(CharacterTurnTranscriptSnapshot),
            },
            method.GetParameters().Select(parameter => parameter.ParameterType));
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterExaminationResult SafeResult()
    {
        CharacterTurnWorldEventOccurrence examination =
            CharacterExaminationResultTests.ExaminationOccurrence();
        return new CharacterExaminationResult(
            examination,
            CharacterExaminationSafety.SafeToCollect);
    }

    internal static CharacterTurnWorldEventOccurrence Collection(
        CharacterExaminationResult result,
        WorldEvent worldEvent) =>
        new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                result.Occurrence.TranscriptEntry.CharacterId,
                new CharacterIntention(
                    CharacterIntentionType.CollectObject,
                    result.TargetObjectId)));

    internal static CharacterTurnTranscriptSnapshot TranscriptWithExamination(
        CharacterExaminationResult result) =>
        new([result.Occurrence.TranscriptEntry]);

    internal static WorldEvent Event(string id) =>
        new(new WorldEventId(id), WorldEventTimeEffect.Advance);

    internal static WorldEventSnapshot WorldEvents(
        CharacterExaminationResult result,
        WorldEvent collectionEvent) =>
        new([result.Occurrence.WorldEvent, collectionEvent]);

    internal static WorldEventLogSnapshot EventLogWithExamination(
        CharacterExaminationResult result) =>
        new([result.Occurrence.WorldEvent.EventId]);

    private static bool CanOccur(
        CharacterExaminationResult result,
        CharacterTurnWorldEventOccurrence collection,
        WorldEventSnapshot worldEvents,
        CharacterTurnTranscriptSnapshot transcript) =>
        CharacterPostExaminationCollectionOccurrencePolicy.CanOccur(
            result,
            collection,
            worldEvents,
            EventLogWithExamination(result),
            new WorldTimeSnapshot(result.Occurrence.WorldEvent.EventId),
            transcript);
}
