using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterPostExaminationCollectionOccurrenceTransitionTests
{
    [Fact]
    public void RecordsTheExactSafeCollectionAfterTheExactExamination()
    {
        CharacterExaminationResult result =
            CharacterPostExaminationCollectionOccurrencePolicyTests.SafeResult();
        WorldEvent collectionEvent =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Event(
                "healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Collection(
                result,
                collectionEvent);
        WorldEventSnapshot worldEvents =
            CharacterPostExaminationCollectionOccurrencePolicyTests.WorldEvents(
                result,
                collectionEvent);
        WorldEventId examinationEventId = result.Occurrence.WorldEvent.EventId;
        WorldEventLogSnapshot eventLog = new([examinationEventId]);
        WorldTimeSnapshot worldTime = new(examinationEventId);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterPostExaminationCollectionOccurrencePolicyTests
                .TranscriptWithExamination(result);

        CharacterTurnWorldEventOccurrenceResult transitionResult =
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result,
                collection,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        Assert.Same(collection, transitionResult.Occurrence);
        Assert.Collection(
            transitionResult.WorldEventResult.EventLog.Entries,
            entry => Assert.Same(examinationEventId, entry),
            entry => Assert.Same(collectionEvent.EventId, entry));
        Assert.Same(
            collectionEvent.EventId,
            transitionResult.WorldEventResult.WorldTime.CurrentEventId);
        Assert.Collection(
            transitionResult.Transcript.Entries,
            entry => Assert.Same(result.Occurrence.TranscriptEntry, entry),
            entry => Assert.Same(collection.TranscriptEntry, entry));
        Assert.Same(CharacterExaminationSafety.SafeToCollect, result.Safety);
    }

    [Fact]
    public void RejectsUnsafeCollectionWithoutChangingInputs()
    {
        CharacterExaminationResult safe =
            CharacterPostExaminationCollectionOccurrencePolicyTests.SafeResult();
        CharacterExaminationResult unsafeResult = new(
            safe.Occurrence,
            CharacterExaminationSafety.UnsafeToCollect);
        WorldEvent collectionEvent =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Event(
                "healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Collection(
                unsafeResult,
                collectionEvent);
        WorldEventSnapshot worldEvents =
            CharacterPostExaminationCollectionOccurrencePolicyTests.WorldEvents(
                unsafeResult,
                collectionEvent);
        WorldEventLogSnapshot eventLog =
            CharacterPostExaminationCollectionOccurrencePolicyTests
                .EventLogWithExamination(unsafeResult);
        WorldTimeSnapshot worldTime = new(unsafeResult.Occurrence.WorldEvent.EventId);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterPostExaminationCollectionOccurrencePolicyTests
                .TranscriptWithExamination(unsafeResult);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                unsafeResult,
                collection,
                worldEvents,
                eventLog,
                worldTime,
                transcript));
        Assert.Collection(
            worldEvents.Entries,
            entry => Assert.Same(unsafeResult.Occurrence.WorldEvent, entry),
            entry => Assert.Same(collectionEvent, entry));
        Assert.Collection(
            eventLog.Entries,
            entry => Assert.Same(unsafeResult.Occurrence.WorldEvent.EventId, entry));
        Assert.Same(unsafeResult.Occurrence.WorldEvent.EventId, worldTime.CurrentEventId);
        Assert.Collection(
            transcript.Entries,
            entry => Assert.Same(unsafeResult.Occurrence.TranscriptEntry, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughTheEligibilityAndOccurrenceBoundaries()
    {
        CharacterExaminationResult result =
            CharacterPostExaminationCollectionOccurrencePolicyTests.SafeResult();
        WorldEvent collectionEvent =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Event(
                "healer-collected-flowers");
        CharacterTurnWorldEventOccurrence collection =
            CharacterPostExaminationCollectionOccurrencePolicyTests.Collection(
                result,
                collectionEvent);
        WorldEventSnapshot worldEvents = new([collectionEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterPostExaminationCollectionOccurrencePolicyTests
                .TranscriptWithExamination(result);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                null!, collection, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result, null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result, collection, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result, collection, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result, collection, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterPostExaminationCollectionOccurrenceTransition.Occur(
                result, collection, worldEvents, eventLog, worldTime, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureOccurrenceOperation()
    {
        Type transitionType = typeof(CharacterPostExaminationCollectionOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Occur", method.Name);
        Assert.Equal(typeof(CharacterTurnWorldEventOccurrenceResult), method.ReturnType);
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
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }
}
