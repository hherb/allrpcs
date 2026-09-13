using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Runs focused healer response-speech occurrence checks when enabled.</summary>
public partial class CharacterTurnResponseSpeechFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_CHARACTER_TURN_RESPONSE_SPEECH_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please examine the flowers before collecting them.";
    private const float DialogueHearingRangePixels = 192.0f;
    private const float WhisperHearingRangePixels = 48.0f;

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private DialogueInputController inputController = null!;
    private TemporaryCharacterTurnResponseSource responseSource = null!;
    private int stage;
    private DialogueUtterance? playerUtterance;
    private CharacterTurnTranscriptSnapshot? recordedTranscript;
    private DialogueEvent? recordedHealerEvent;
    private object? initialWorldEventLog;
    private object? initialWorldTime;
    private object? initialWorldObjects;
    private object? initialPerception;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        responseSource = GetNode<TemporaryCharacterTurnResponseSource>(
            "../TemporaryCharacterTurnResponseSource");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (stage == 0)
        {
            CaptureInitialState();
            if (!ArrangeWhisperResponse())
            {
                return;
            }

            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (!ProvesExactHealerSpeechOccurrence())
            {
                return;
            }

            recordedTranscript = transcriptController.Transcript;
            recordedHealerEvent = eventController.LastOccurredEvent;
            stage = 2;
            return;
        }

        SetProcess(false);
        if (!ReferenceEquals(transcriptController.Transcript, recordedTranscript)
            || !ReferenceEquals(eventController.LastOccurredEvent, recordedHealerEvent)
            || !UnrelatedStateIsPreserved())
        {
            Fail("Healer response speech was repeated or changed unrelated state.");
            return;
        }

        GD.Print(
            "Character-turn response speech flow smoke passed: the exact complete healer speech "
            + "whisper occurred once with only the nearby Seeker as listener, appended once after "
            + "player speech, "
            + "remained speaking for an interruptible frame without repeating or executing.");
        GetTree().Quit();
    }

    private bool ArrangeWhisperResponse()
    {
        CharacterBody2D seeker = GetNode<CharacterBody2D>("../Seeker");
        CharacterBody2D healer = GetNode<CharacterBody2D>("../Healer");
        Marker2D sage = GetNode<Marker2D>("../SageHearingPosition");
        IReadOnlyList<Allrpcs.Domain.Characters.Character> participants =
            GetNode<TemporaryOrdinaryCharacterParticipantSource>(
                "../TemporaryOrdinaryCharacterParticipantSource").Participants;
        playerUtterance = new DialogueUtterance(inputController.SpeakerCharacterId!, PlayerText);
        DialogueDelivery playerDelivery = new(
            playerUtterance,
            DialogueDeliveryMode.OrdinarySpeech);
        DialogueHearingCandidate[] candidates =
        [
            new(playerUtterance.SpeakerCharacterId, ToNumerics(seeker.Position)),
            new(participants[0].Identity.Id, ToNumerics(healer.Position)),
            new(participants[1].Identity.Id, ToNumerics(sage.Position)),
        ];
        if (!eventController.TryOccur(
                playerDelivery,
                candidates[0].Position,
                DialogueHearingRangePixels,
                WhisperHearingRangePixels,
                candidates)
            || eventController.LastOccurredEvent is not { } playerEvent
            || !ReferenceEquals(playerEvent.Delivery, playerDelivery)
            || !transcriptController.TryAppendSpokenStatement(playerEvent.Delivery))
        {
            Fail("Focused speech-flow scaffolding could not record its exact player occurrence.");
            return false;
        }

        CharacterTurnRequest? request = GetNode<TemporaryCharacterTurnRequestSource>(
            "../TemporaryCharacterTurnRequestSource").CreateRequest(
                playerEvent,
                participants[0].Identity.Id,
                DialogueActType.Request);
        if (request is null || !turnController.TryStart(request))
        {
            Fail("Focused speech-flow scaffolding could not start its exact healer request.");
            return false;
        }

        CharacterTurnResponse selected = responseSource.CreateResponse(
            request,
            GetNode<TemporaryWorldObjectSource>(
                "../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id);
        CharacterTurnResponse response = new(
            request,
            selected.Disposition,
            new DialogueDelivery(selected.Speech!, DialogueDeliveryMode.Whispering),
            selected.ImmediateIntention,
            selected.Commitments,
            selected.StateProposals);
        seeker.Position = healer.Position + new Vector2(32.0f, 0.0f);
        if (!turnController.TryReceiveResponse(response))
        {
            Fail("Focused speech-flow scaffolding could not install its whisper response.");
            return false;
        }

        return true;
    }

    private bool ProvesExactHealerSpeechOccurrence()
    {
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueDelivery? delivery = activity?.Response?.Delivery;
        DialogueUtterance? speech = delivery?.Utterance;
        DialogueEvent? occurred = eventController.LastOccurredEvent;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        bool matches = playerUtterance is not null
            && activity is not null
            && speech is not null
            && delivery is not null
            && occurred is not null
            && !ReferenceEquals(occurred.Utterance, playerUtterance)
            && ReferenceEquals(occurred.Delivery, delivery)
            && ReferenceEquals(occurred.Utterance, speech)
            && ReferenceEquals(delivery.Mode, DialogueDeliveryMode.Whispering)
            && ReferenceEquals(occurred.Audience.SpeakerCharacterId, speech.SpeakerCharacterId)
            && occurred.Audience.ListenerCharacterIds.Count == 1
            && ReferenceEquals(
                occurred.Audience.ListenerCharacterIds[0],
                playerUtterance.SpeakerCharacterId)
            && ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            && ReferenceEquals(
                activity.Response!.ImmediateIntention.Type,
                CharacterIntentionType.ExamineObject)
            && ReferenceEquals(
                activity.Response.ImmediateIntention.TargetObjectId,
                GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource")
                    .WorldObjects.Entries[0].Identity.Id)
            && transcript is { Entries.Count: 2 }
            && ReferenceEquals(transcript.Entries[0].SpokenStatement, playerUtterance)
            && ReferenceEquals(transcript.Entries[1].Delivery, delivery)
            && ReferenceEquals(transcript.Entries[1].SpokenStatement, speech)
            && transcript.Entries.All(entry => entry.PhysicalAction is null);
        if (!matches)
        {
            Fail("Exact healer speech did not occur and append after the player speech.");
        }

        return matches;
    }

    private void CaptureInitialState()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        initialWorldEventLog = worldEvents.EventLog;
        initialWorldTime = worldEvents.WorldTime;
        initialWorldObjects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
        initialPerception = GetNode<FocusedSearchActivityController>(
            "../Seeker/FocusedSearchActivityController").Perception;
    }

    private bool UnrelatedStateIsPreserved()
    {
        WorldEventController worldEvents = GetNode<WorldEventController>("../WorldEventController");
        return ReferenceEquals(worldEvents.EventLog, initialWorldEventLog)
            && ReferenceEquals(worldEvents.WorldTime, initialWorldTime)
            && ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            && ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception);
    }

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }
}
