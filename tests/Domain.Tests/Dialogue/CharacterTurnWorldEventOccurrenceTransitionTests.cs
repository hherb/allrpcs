using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnWorldEventOccurrenceTransitionTests
{
    [Fact]
    public void RecordsAnExactSpokenOccurrenceAndPreservesNoAdvanceTime()
    {
        WorldEvent worldEvent = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnTranscriptEntry retained = Speech("I will examine it.");
        CharacterTurnTranscriptEntry spoken = Speech("The flowers appear safe.");
        CharacterTurnWorldEventOccurrence occurrence = new(worldEvent, spoken);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventId priorEventId = new("seeker-spoke");
        WorldEventLogSnapshot eventLog = new([priorEventId]);
        WorldEventId currentTimeId = new("journey-began");
        WorldTimeSnapshot worldTime = new(currentTimeId);
        CharacterTurnTranscriptSnapshot transcript = new([retained]);

        CharacterTurnWorldEventOccurrenceResult result =
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        Assert.Same(occurrence, result.Occurrence);
        Assert.NotSame(eventLog, result.WorldEventResult.EventLog);
        Assert.Same(worldTime, result.WorldEventResult.WorldTime);
        Assert.Collection(
            result.WorldEventResult.EventLog.Entries,
            entry => Assert.Same(priorEventId, entry),
            entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Collection(
            result.Transcript.Entries,
            entry => Assert.Same(retained, entry),
            entry =>
            {
                Assert.Same(spoken, entry);
                Assert.Same(CharacterTurnTranscriptEntryKind.SpokenStatement, entry.Kind);
            });
    }

    [Fact]
    public void RecordsAnExactPhysicalActionAndAdvancesTime()
    {
        WorldEvent worldEvent = Event(
            "healer-examined-moonwort",
            WorldEventTimeEffect.Advance);
        CharacterTurnTranscriptEntry action = new(
            new CharacterId("healer"),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("moonwort")));
        CharacterTurnWorldEventOccurrence occurrence = new(worldEvent, action);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = new([]);

        CharacterTurnWorldEventOccurrenceResult result =
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript);

        Assert.Same(occurrence, result.Occurrence);
        Assert.Collection(
            result.WorldEventResult.EventLog.Entries,
            entry => Assert.Same(worldEvent.EventId, entry));
        Assert.Same(worldEvent.EventId, result.WorldEventResult.WorldTime.CurrentEventId);
        Assert.Collection(
            result.Transcript.Entries,
            entry =>
            {
                Assert.Same(action, entry);
                Assert.Same(CharacterTurnTranscriptEntryKind.PhysicalAction, entry.Kind);
            });
    }

    [Fact]
    public void RejectsIneligibleOccurrencesWithoutChangingInputs()
    {
        WorldEvent authoritative = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = new(
            Event("healer-spoke", WorldEventTimeEffect.NoAdvance),
            Speech("The flowers appear safe."));
        WorldEventSnapshot worldEvents = new([authoritative]);
        WorldEventId retainedId = new("seeker-spoke");
        WorldEventLogSnapshot eventLog = new([retainedId]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptEntry retainedEntry = Speech("Please examine them.");
        CharacterTurnTranscriptSnapshot transcript = new([retainedEntry]);

        Assert.Throws<InvalidOperationException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence,
                worldEvents,
                eventLog,
                worldTime,
                transcript));
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(authoritative, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(retainedId, entry));
        Assert.Null(worldTime.CurrentEventId);
        Assert.Collection(transcript.Entries, entry => Assert.Same(retainedEntry, entry));
    }

    [Fact]
    public void RejectsNullInputsThroughEligibilityPolicyAndTranscriptTransition()
    {
        WorldEvent worldEvent = Event("healer-spoke", WorldEventTimeEffect.NoAdvance);
        CharacterTurnWorldEventOccurrence occurrence = new(worldEvent, Speech("It is safe."));
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnWorldEventOccurrenceTransition.Occur(
                occurrence, worldEvents, eventLog, worldTime, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureOccurrenceOperation()
    {
        Type transitionType = typeof(CharacterTurnWorldEventOccurrenceTransition);
        System.Reflection.MethodInfo method = Assert.Single(
            transitionType.GetMethods().Where(candidate => candidate.DeclaringType == transitionType));

        Assert.Equal("Occur", method.Name);
        Assert.Equal(typeof(CharacterTurnWorldEventOccurrenceResult), method.ReturnType);
        Assert.True(method.IsStatic);
        Assert.Collection(
            method.GetParameters(),
            parameter => Assert.Equal(
                typeof(CharacterTurnWorldEventOccurrence),
                parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldEventLogSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(typeof(WorldTimeSnapshot), parameter.ParameterType),
            parameter => Assert.Equal(
                typeof(CharacterTurnTranscriptSnapshot),
                parameter.ParameterType));
        Assert.Empty(transitionType.GetConstructors());
        Assert.Empty(transitionType.GetProperties());
        Assert.Empty(transitionType.GetEvents());
        Assert.True(transitionType.IsAbstract);
        Assert.True(transitionType.IsSealed);
    }

    private static WorldEvent Event(string id, WorldEventTimeEffect effect) =>
        new(new WorldEventId(id), effect);

    private static CharacterTurnTranscriptEntry Speech(string text) =>
        new(new DialogueUtterance(new CharacterId("healer"), text));
}
