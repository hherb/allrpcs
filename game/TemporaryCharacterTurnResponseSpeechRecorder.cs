using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Records each fresh complete healer response speech as one temporary scene occurrence.</summary>
public partial class TemporaryCharacterTurnResponseSpeechRecorder : Node
{
    private const float DialogueHearingRangePixels = 192.0f;

    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private DialogueInputController inputController = null!;
    private CharacterBody2D healer = null!;
    private CharacterBody2D seeker = null!;
    private DialogueUtterance? lastRecordedSpeech;

    /// <inheritdoc />
    public override void _Ready()
    {
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        inputController = GetNode<DialogueInputController>("../DialogueInputController");
        healer = GetNode<CharacterBody2D>("../Healer");
        seeker = GetNode<CharacterBody2D>("../Seeker");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        CharacterTurnActivity? activity = turnController.Activity;
        DialogueUtterance? speech = activity?.Response?.Speech;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Speaking)
            || speech is null
            || ReferenceEquals(speech, lastRecordedSpeech))
        {
            return;
        }

        CharacterId seekerId = inputController.SpeakerCharacterId
            ?? throw new InvalidOperationException("The scene Seeker identifier was absent.");
        DialogueHearingCandidate[] candidates =
        [
            new(speech.SpeakerCharacterId, ToNumerics(healer.Position)),
            new(seekerId, ToNumerics(seeker.Position)),
        ];
        if (!eventController.TryOccur(
                speech,
                candidates[0].Position,
                DialogueHearingRangePixels,
                candidates)
            || !transcriptController.TryAppendSpokenStatement(speech))
        {
            throw new InvalidOperationException(
                "The complete healer response speech could not occur and append once.");
        }

        lastRecordedSpeech = speech;
    }

    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
}
