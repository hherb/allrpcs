using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Tests.Dialogue;

public sealed class CharacterTurnImmediateIntentionOccurrencePolicyTests
{
    [Fact]
    public void AllowsTheExactSpeakingResponseActionAfterItsExactSpeechWasObserved()
    {
        CharacterTurnActivity activity = SpeakingActivity();
        WorldEvent worldEvent = Event("healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity, worldEvent);

        bool canOccur = CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            occurrence,
            new WorldEventSnapshot([worldEvent]),
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            TranscriptWithResponseSpeech(activity));

        Assert.True(canOccur);
    }

    [Fact]
    public void RejectsAnActivityThatIsNotSpeaking()
    {
        CharacterTurnActivity speaking = SpeakingActivity();
        WorldEvent worldEvent = Event("healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(speaking, worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithResponseSpeech(speaking);

        foreach (CharacterTurnActivityState state in new[]
                 {
                     CharacterTurnActivityState.Completed,
                     CharacterTurnActivityState.Cancelled,
                 })
        {
            CharacterTurnActivity activity = new(speaking.Request, speaking.Response, state);

            Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity,
                occurrence,
                worldEvents,
                new WorldEventLogSnapshot([]),
                new WorldTimeSnapshot(null),
                transcript));
        }
    }

    [Fact]
    public void RejectsADistinctActionOrActingCharacter()
    {
        CharacterTurnActivity activity = SpeakingActivity();
        CharacterIntention intention = activity.Response!.ImmediateIntention;
        WorldEvent worldEvent = Event("healer-examined-flowers");
        WorldEventSnapshot worldEvents = new([worldEvent]);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithResponseSpeech(activity);
        CharacterTurnWorldEventOccurrence distinctAction = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                activity.Request.RespondingCharacterId,
                new CharacterIntention(intention.Type, intention.TargetObjectId)));
        CharacterTurnWorldEventOccurrence distinctCharacter = new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                new CharacterId(activity.Request.RespondingCharacterId.Value),
                intention));

        Assert.Equal(intention, distinctAction.TranscriptEntry.PhysicalAction);
        Assert.NotSame(intention, distinctAction.TranscriptEntry.PhysicalAction);
        Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            distinctAction,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            transcript));
        Assert.Equal(activity.Request.RespondingCharacterId, distinctCharacter.TranscriptEntry.CharacterId);
        Assert.NotSame(
            activity.Request.RespondingCharacterId,
            distinctCharacter.TranscriptEntry.CharacterId);
        Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            distinctCharacter,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            transcript));
    }

    [Fact]
    public void RejectsAnUnobservedOrNonlatestResponseSpeech()
    {
        CharacterTurnActivity activity = SpeakingActivity();
        WorldEvent worldEvent = Event("healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity, worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        CharacterTurnTranscriptEntry exactSpeech = new(activity.Response!.Speech!);
        CharacterTurnTranscriptEntry laterSpeech = new(
            new DialogueUtterance(new CharacterId("seeker"), "What did you find?"));

        Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            occurrence,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([])));
        Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            occurrence,
            worldEvents,
            new WorldEventLogSnapshot([]),
            new WorldTimeSnapshot(null),
            new CharacterTurnTranscriptSnapshot([exactSpeech, laterSpeech])));
    }

    [Fact]
    public void DelegatesAuthoritativeWorldEventEligibilityAndChangesNothing()
    {
        CharacterTurnActivity activity = SpeakingActivity();
        WorldEvent authoritative = Event("healer-examined-flowers");
        WorldEvent distinctEqual = Event("healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity, distinctEqual);
        WorldEventSnapshot worldEvents = new([authoritative]);
        WorldEventId retainedId = new("journey-began");
        WorldEventLogSnapshot eventLog = new([retainedId]);
        WorldTimeSnapshot worldTime = new(retainedId);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithResponseSpeech(activity);

        Assert.False(CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
            activity,
            occurrence,
            worldEvents,
            eventLog,
            worldTime,
            transcript));
        Assert.Same(
            activity.Response!.ImmediateIntention,
            occurrence.TranscriptEntry.PhysicalAction);
        Assert.Collection(worldEvents.Entries, entry => Assert.Same(authoritative, entry));
        Assert.Collection(eventLog.Entries, entry => Assert.Same(retainedId, entry));
        Assert.Same(retainedId, worldTime.CurrentEventId);
        Assert.Single(transcript.Entries);
        Assert.Same(activity.Response!.Speech, transcript.Entries[0].SpokenStatement);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        CharacterTurnActivity activity = SpeakingActivity();
        WorldEvent worldEvent = Event("healer-examined-flowers");
        CharacterTurnWorldEventOccurrence occurrence = Occurrence(activity, worldEvent);
        WorldEventSnapshot worldEvents = new([worldEvent]);
        WorldEventLogSnapshot eventLog = new([]);
        WorldTimeSnapshot worldTime = new(null);
        CharacterTurnTranscriptSnapshot transcript = TranscriptWithResponseSpeech(activity);

        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                null!, occurrence, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity, null!, worldEvents, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity, occurrence, null!, eventLog, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity, occurrence, worldEvents, null!, worldTime, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity, occurrence, worldEvents, eventLog, null!, transcript));
        Assert.Throws<ArgumentNullException>(() =>
            CharacterTurnImmediateIntentionOccurrencePolicy.CanOccur(
                activity, occurrence, worldEvents, eventLog, worldTime, null!));
    }

    [Fact]
    public void ExposesOnlyTheSinglePureEligibilityOperation()
    {
        Type policyType = typeof(CharacterTurnImmediateIntentionOccurrencePolicy);
        System.Reflection.MethodInfo method = Assert.Single(
            policyType.GetMethods().Where(candidate => candidate.DeclaringType == policyType));

        Assert.Equal("CanOccur", method.Name);
        Assert.Equal(typeof(bool), method.ReturnType);
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
        Assert.Empty(policyType.GetConstructors());
        Assert.Empty(policyType.GetProperties());
        Assert.Empty(policyType.GetEvents());
        Assert.True(policyType.IsAbstract);
        Assert.True(policyType.IsSealed);
    }

    internal static CharacterTurnActivity SpeakingActivity()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        DialogueUtterance playerSpeech = new(seekerId, "Please examine the flowers.");
        CharacterTurnRequest request = new(
            healerId,
            new DialogueEvent(
                playerSpeech,
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(playerSpeech, DialogueActType.Request));
        CharacterTurnResponse response = new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueUtterance(healerId, "I will examine them first."),
            new CharacterIntention(
                CharacterIntentionType.ExamineObject,
                new ObjectId("flowers")),
            [],
            []);

        return new CharacterTurnActivity(
            request,
            response,
            CharacterTurnActivityState.Speaking);
    }

    internal static CharacterTurnWorldEventOccurrence Occurrence(
        CharacterTurnActivity activity,
        WorldEvent worldEvent) =>
        new(
            worldEvent,
            new CharacterTurnTranscriptEntry(
                activity.Request.RespondingCharacterId,
                activity.Response!.ImmediateIntention));

    internal static CharacterTurnTranscriptSnapshot TranscriptWithResponseSpeech(
        CharacterTurnActivity activity) =>
        new([new CharacterTurnTranscriptEntry(activity.Response!.Speech!)]);

    internal static WorldEvent Event(string id) =>
        new(new WorldEventId(id), WorldEventTimeEffect.Advance);
}
