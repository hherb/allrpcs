using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Records each fresh complete response delivery as one scene occurrence.</summary>
public partial class TemporaryCharacterTurnResponseSpeechRecorder : Node
{
    private const float DialogueHearingRangePixels = 192.0f;
    private const float WhisperHearingRangePixels = 48.0f;

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private DialogueInputController inputController = null!;
    private CharacterBody2D healer = null!;
    private CharacterBody2D seeker = null!;
    private Marker2D sageHearingPosition = null!;
    private TemporaryOrdinaryCharacterParticipantSource participantSource = null!;
    private DialogueDelivery? lastRecordedDelivery;

    /// <inheritdoc />
    public override void _Ready()
    {
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        participantSource = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        healer = GetNode<CharacterBody2D>("../Healer");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        sageHearingPosition = GetNode<Marker2D>("../SageHearingPosition");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueDelivery? delivery = activity?.Response?.Delivery;
        DialogueUtterance? speech = delivery?.Utterance;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            || delivery is null
            || speech is null
            || ReferenceEquals(delivery, lastRecordedDelivery))
        {
            return;
        }

        CharacterId seekerId = inputController.SpeakerCharacterId
            ?? throw new InvalidOperationException("The scene Seeker identifier was absent.");
        IReadOnlyList<Character> participants = participantSource.Participants;
        Character speaker = participants.SingleOrDefault(
            participant => participant.Identity.Id.Equals(speech.SpeakerCharacterId))
            ?? throw new InvalidOperationException(
                "The response speaker was absent from ordinary scene participants.");
        Character otherParticipant = ReferenceEquals(
            speaker,
            participants[0])
            ? participants[1]
            : participants[0];
        Vector2 speakerPosition = GetParticipantPosition(speaker, participants);
        DialogueHearingCandidate[] candidates =
        [
            new(speech.SpeakerCharacterId, ToNumerics(speakerPosition)),
            new(seekerId, ToNumerics(seeker.Position)),
            new(
                otherParticipant.Identity.Id,
                ToNumerics(GetParticipantPosition(otherParticipant, participants))),
        ];
        if (!eventController.TryOccur(
                delivery,
                candidates[0].Position,
                DialogueHearingRangePixels,
                WhisperHearingRangePixels,
                candidates)
            || !transcriptController.TryAppendSpokenStatement(delivery))
        {
            throw new InvalidOperationException(
                "The complete response delivery could not occur and append once.");
        }

        lastRecordedDelivery = delivery;
    }

    private Vector2 GetParticipantPosition(
        Character participant,
        IReadOnlyList<Character> participants)
    {
        if (ReferenceEquals(participant, participants[0]))
        {
            return healer.Position;
        }

        if (ReferenceEquals(participant, participants[1]))
        {
            return sageHearingPosition.Position;
        }

        throw new InvalidOperationException(
            "The ordinary response participant had no scene hearing position.");
    }

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
}
