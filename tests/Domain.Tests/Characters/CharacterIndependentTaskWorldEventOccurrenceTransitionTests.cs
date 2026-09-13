using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Characters;

public sealed class CharacterIndependentTaskWorldEventOccurrenceTransitionTests
{
    [Fact]
    public void RecordsOccurrenceAndCompletesExactContainedActivityAtomically()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = new([]);

        CharacterIndependentTaskWorldEventOccurrenceResult result =
            CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
                candidate,
                activities,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        CharacterIndependentTaskActivity completed = Assert.Single(result.Activities.Activities);
        Assert.Same(activity.Assignment, completed.Assignment);
        Assert.Same(CharacterIndependentTaskActivityState.Completed, completed.State);
        Assert.Same(candidate, result.Candidate);
        Assert.Same(candidate.Occurrence, result.WorldEventResult.Occurrence);
        Assert.Collection(
            result.WorldEventResult.WorldEventResult.EventLog.Entries,
            eventId => Assert.Same(worldEvent.EventId, eventId));
        Assert.Same(
            worldEvent.EventId,
            result.WorldEventResult.WorldEventResult.WorldTime.CurrentEventId);
        Assert.Collection(
            result.WorldEventResult.Transcript.Entries,
            entry => Assert.Same(candidate.Occurrence.TranscriptEntry, entry));
        Assert.Same(activity, activities.Activities[0]);
        Assert.Empty(eventLog.Entries);
        Assert.Null(worldTime.CurrentEventId);
        Assert.Empty(transcript.Entries);
    }

    [Fact]
    public void RejectsDistinctValueEqualActivityBeforeApplyingAnything()
    {
        CharacterIndependentTaskActivity contained =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        CharacterIndependentTaskActivity equal =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            equal,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(equal, worldEvent));
        CharacterIndependentTaskActivitySnapshot activities = new([contained]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = new([]);

        Assert.Equal(contained, equal);
        Assert.NotSame(contained, equal);
        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
                candidate,
                activities,
                new WorldEventSnapshot([worldEvent]),
                eventLog,
                worldTime,
                transcript));
        Assert.Same(contained, activities.Activities[0]);
        Assert.Empty(eventLog.Entries);
        Assert.Null(worldTime.CurrentEventId);
        Assert.Empty(transcript.Entries);
    }

    [Fact]
    public void RejectsIneligibleWorldEventBeforeCompletingTask()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        WorldEventLogSnapshot eventLog = new([worldEvent.EventId]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
                candidate,
                activities,
                new WorldEventSnapshot([worldEvent]),
                eventLog,
                new WorldTimeSnapshot(null),
                new CharacterTurnTranscriptSnapshot([])));
        Assert.Same(activity, activities.Activities[0]);
        Assert.Collection(eventLog.Entries, eventId => Assert.Same(worldEvent.EventId, eventId));
    }

    [Fact]
    public void RejectsNullsAndExposesOnlyTheAtomicTransition()
    {
        CharacterIndependentTaskActivity activity =
            CharacterIndependentTaskWorldEventOccurrenceTests.ActiveActivity();
        WorldEvent worldEvent = CharacterIndependentTaskWorldEventOccurrenceTests.Event();
        CharacterIndependentTaskWorldEventOccurrence candidate = new(
            activity,
            CharacterIndependentTaskWorldEventOccurrenceTests.Occurrence(activity, worldEvent));
        CharacterIndependentTaskActivitySnapshot activities = new([activity]);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = new([]);

        Assert.Throws<ArgumentNullException>(() => Occur(
            null!, activities, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() => Occur(
            candidate, null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() => Occur(
            candidate, activities, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() => Occur(
            candidate, activities, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() => Occur(
            candidate, activities, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() => Occur(
            candidate, activities, worldEvents, eventLog, worldTime, null!));

        Type transitionType = typeof(CharacterIndependentTaskWorldEventOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(item => item.DeclaringType == transitionType));
        Assert.Equal("Occur", method.Name);
        Assert.Equal(typeof(CharacterIndependentTaskWorldEventOccurrenceResult), method.ReturnType);
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract && transitionType.IsSealed);
    }

    private static CharacterIndependentTaskWorldEventOccurrenceResult Occur(
        CharacterIndependentTaskWorldEventOccurrence candidate,
        CharacterIndependentTaskActivitySnapshot activities,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        CharacterTurnTranscriptSnapshot transcript) =>
        CharacterIndependentTaskWorldEventOccurrenceTransition.Occur(
            candidate,
            activities,
            worldEvents,
            eventLog,
            worldTime,
            transcript);
}
