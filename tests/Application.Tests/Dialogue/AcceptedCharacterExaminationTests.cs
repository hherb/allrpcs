using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Application.Tests.Dialogue;

public sealed class AcceptedCharacterExaminationTests
{
    private static readonly CharacterId Actor = new("healer");
    private static readonly ObjectId Target = new("herb-patch");

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void AcceptsSilentOrExactlyRecordedSpeechAndRetainsActionOrigin(bool spoken)
    {
        var activity = Activity(spoken);
        var transcript = new CharacterTurnTranscriptSnapshot(spoken
            ? [new CharacterTurnTranscriptEntry(activity.Response!.Delivery!)] : []);
        var accepted = AcceptedCharacterExamination.TryAccept(activity, Actor, Target, transcript);
        Assert.NotNull(accepted);
        Assert.Same(activity.Response, accepted.Response);
        var entry = accepted.CreateTranscriptEntry();
        Assert.Same(Actor, entry.CharacterId);
        Assert.Same(activity.Response!.ImmediateIntention, entry.PhysicalAction);
        Assert.Null(entry.SpokenStatement);
    }

    [Fact]
    public void RejectsUnrecordedOrReconstructedSpeech()
    {
        var activity = Activity(true);
        Assert.Null(AcceptedCharacterExamination.TryAccept(activity, Actor, Target, new([])));
        var reconstructed = new DialogueDelivery(new(Actor, activity.Response!.Speech!.Text),
            DialogueDeliveryMode.OrdinarySpeech);
        Assert.Null(AcceptedCharacterExamination.TryAccept(activity, Actor, Target,
            new([new CharacterTurnTranscriptEntry(reconstructed)])));
    }

    [Fact]
    public void RejectsForeignActorTargetWaitRefusalAndTerminalActivity()
    {
        var activity = Activity(false);
        Assert.Null(AcceptedCharacterExamination.TryAccept(activity, new("healer"), Target, new([])));
        Assert.Null(AcceptedCharacterExamination.TryAccept(activity, Actor, new("other"), new([])));
        Assert.Null(AcceptedCharacterExamination.TryAccept(Activity(false, wait: true), Actor, Target, new([])));
        Assert.Null(AcceptedCharacterExamination.TryAccept(Activity(false, refuse: true), Actor, Target, new([])));
        Assert.Null(AcceptedCharacterExamination.TryAccept(new(activity.Request, activity.Response,
            CharacterTurnActivityState.Completed), Actor, Target, new([])));
    }

    [Fact]
    public void RepeatedExaminationsHaveUniqueAuthoritativeEventsAndRestoreWithoutLiveCatalog()
    {
        WorldEvent initial = new(new("ready"), WorldEventTimeEffect.NoAdvance);
        WorldEventSnapshot definitions = new([initial]);
        WorldEventLogSnapshot log = new([initial.EventId]);
        WorldEvent first = HerbExaminationEventCatalog.Next(definitions, log);
        definitions = new(definitions.Entries.Append(first));
        log = new(log.Entries.Append(first.EventId));
        WorldEvent second = HerbExaminationEventCatalog.Next(definitions, log);
        log = new(log.Entries.Append(second.EventId));
        var restored = HerbExaminationEventCatalog.Expand(new([initial]), log);
        Assert.Equal("herb-patch-examination-1", first.EventId.Value);
        Assert.Equal("herb-patch-examination-2", second.EventId.Value);
        Assert.Equal(WorldEventTimeEffect.Advance, restored.Entries[^1].TimeEffect);
        Assert.True(Allrpcs.Domain.Persistence.WorldEventSaveStateRestorationPolicy.CanRestore(
            new(log, new(second.EventId)), restored));
        Assert.Equal("herb-patch-examination-3", HerbExaminationEventCatalog.Next(restored, log).EventId.Value);
        // Loading an older save may leave future definitions present; reuse their exact event authority.
        Assert.Same(first, HerbExaminationEventCatalog.Next(definitions, new([initial.EventId])));
    }

    [Theory]
    [InlineData("herb-patch-examination-2")]
    [InlineData("herb-patch-examination-01")]
    [InlineData("herb-patch-examination-0")]
    public void RejectsNoncanonicalOrOutOfOrderSavedExaminations(string id)
    {
        Assert.Throws<ArgumentException>(() => HerbExaminationEventCatalog.Expand(new([]), new([new(id)])));
    }

    private static CharacterTurnActivity Activity(bool spoken, bool wait = false, bool refuse = false)
    {
        CharacterId player = new("seeker");
        DialogueUtterance input = new(player, "Please examine the herb patch.");
        DialogueEvent heard = new(new(input, DialogueDeliveryMode.OrdinarySpeech), new(player, [Actor]));
        CharacterTurnRequest request = new(Actor, heard, new(input, DialogueActType.Request));
        DialogueDelivery? delivery = spoken ? new(new(Actor, "I will examine it."),
            DialogueDeliveryMode.OrdinarySpeech) : null;
        CharacterTurnResponse response = new(request,
            refuse ? CharacterTurnDisposition.Refuse : CharacterTurnDisposition.Accept,
            delivery, wait ? new(CharacterIntentionType.Wait, null)
                : new(CharacterIntentionType.ExamineObject, new("herb-patch")), [], []);
        return new(request, response, CharacterTurnActivityState.Speaking);
    }
}
