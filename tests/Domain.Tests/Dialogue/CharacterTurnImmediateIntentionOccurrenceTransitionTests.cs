using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnImmediateIntentionOccurrenceTransitionTests
{
    [Fact]
    public void RecordsTheExactImmediateActionAfterObservedSpeechAndAdvancesTime()
    {
        CharacterTurnActivity activity =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.SpeakingActivity();
        WorldEvent worldEvent =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Event(
                "healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Occurrence(
                activity,
                worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventId retainedEventId = new("journey-began");
        WorldEventLogSnapshot eventLog = new([retainedEventId]);
        WorldTimeSnapshot worldTime = new(retainedEventId);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterTurnImmediateIntentionOccurrencePolicyTests
                .TranscriptWithResponseSpeech(activity);

        CharacterTurnWorldEventOccurrenceResult result =
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        Assert.Same(occurrence, result.Occurrence);
        Assert.Collection(
            result.WorldEventResult.EventLog.Entries,
            entry => Assert.Same(retainedEventId, entry),
            entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Same(worldEvent.EventId, result.WorldEventResult.WorldTime.CurrentEventId);
        Assert.Collection(
            result.Transcript.Entries,
            entry => Assert.Same(activity.Response!.Speech, entry.SpokenStatement),
            entry =>
            {
                Assert.Same(occurrence.TranscriptEntry, entry);
                Assert.Same(CharacterTurnTranscriptEntryKind.PhysicalAction, entry.Kind);
                Assert.Same(activity.Response!.ImmediateIntention, entry.PhysicalAction);
            });
        Assert.Same(CharacterTurnActivityState.Speaking, activity.State);
    }

    [Fact]
    public void RejectsAnIneligibleOccurrenceWithoutChangingInputs()
    {
        CharacterTurnActivity activity =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.SpeakingActivity();
        WorldEvent authoritative =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Event(
                "healer-examined-flowers");
        WorldEvent distinctEqual =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Event(
                "healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Occurrence(
                activity,
                distinctEqual);
        WorldEventSnapshot worldEvents = new([authoritative]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterTurnImmediateIntentionOccurrencePolicyTests
                .TranscriptWithResponseSpeech(activity);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity,
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(authoritative, entry));
        Assert.Empty(eventLog.Entries);
        Assert.Null(worldTime.CurrentEventId);
        Assert.Collection(
            transcript.Entries,
            entry => Assert.Same(activity.Response!.Speech, entry.SpokenStatement));
        Assert.Same(CharacterTurnActivityState.Speaking, activity.State);
    }

    [Fact]
    public void RejectsNullInputsThroughTheEligibilityAndOccurrenceBoundaries()
    {
        CharacterTurnActivity activity =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.SpeakingActivity();
        WorldEvent worldEvent =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Event(
                "healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence =
            CharacterTurnImmediateIntentionOccurrencePolicyTests.Occurrence(
                activity,
                worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript =
            CharacterTurnImmediateIntentionOccurrencePolicyTests
                .TranscriptWithResponseSpeech(activity);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                null!, occurrence, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity, null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity, occurrence, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity, occurrence, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity, occurrence, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrenceTransition.Occur(
                activity, occurrence, worldEvents, eventLog, worldTime, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureOccurrenceOperation()
    {
        Type transitionType = typeof(CharacterTurnImmediateIntentionOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Occur", method.Name);
        Assert.Equal(typeof(CharacterTurnWorldEventOccurrenceResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Equal(
            new[]
            {
                typeof(CharacterTurnActivity),
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
