using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Checks one complete ordinary motivated sage interjection and generation flow.</summary>
public partial class OrdinarySageInterjectionFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_SAGE_INTERJECTION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Can either of you challenge my conclusion?";
    private const string SageText = "Which observation would disprove it?";
    private const int MaximumProcessFrames = 300;
    private static readonly Vector2 ShiftedSeekerPosition = new(1080.0f, 450.0f);

    private readonly SageInterjectionParticipationModel participationModel = new();
    private readonly SageSpeechCharacterModel characterModel = new();
    private CharacterTurnActivityController turnController = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private TemporaryOrdinaryCharacterParticipantSource participantSource = null!;
    private CharacterBody2D seeker = null!;
    private LineEdit input = null!;
    private object? initialWorldObjects;
    private object? initialPerception;
    private Vector2 initialSeekerPosition;
    private int processFrames;
    private bool seekerShifted;
    private bool submitted;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool isEnabled = OS.GetEnvironment(CheckEnvironmentVariable) == EnabledEnvironmentValue;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        if (!GetNode<TemporaryCharacterTurnParticipationModelSource>(
                "../TemporaryCharacterTurnParticipationModelSource")
                .TrySupplyModel(participationModel)
            || !GetNode<TemporaryCharacterModelSource>("../TemporaryCharacterModelSource")
                .TrySupplyModel(characterModel))
        {
            Fail("The sage-interjection smoke could not supply its credential-free models.");
            return;
        }

        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        participantSource = GetNode<TemporaryOrdinaryCharacterParticipantSource>(
            "../TemporaryOrdinaryCharacterParticipantSource");
        seeker = GetNode<CharacterBody2D>("../Seeker");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("The ordinary sage interjection exceeded its bounded smoke duration.");
            return;
        }

        if (!submitted)
        {
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            initialSeekerPosition = seeker.Position;
            input.Text = PlayerText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            submitted = true;
            return;
        }

        Character sage = participantSource.Participants[1];
        CharacterTurnActivity? activity = turnController.Activity;
        if (!seekerShifted
            && activity is not null
            && ReferenceEquals(activity.State, CharacterTurnActivityState.AwaitingResponse)
            && ReferenceEquals(activity.Request.RespondingCharacterId, sage.Identity.Id))
        {
            seeker.Position = ShiftedSeekerPosition;
            seekerShifted = true;
            return;
        }

        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed))
        {
            return;
        }

        SetProcess(false);
        bool passed = ProvesExactCompletedInterjection(activity, sage);
        seeker.Position = initialSeekerPosition;
        if (!passed)
        {
            return;
        }

        GD.Print(
            "Ordinary sage interjection smoke passed: stable healer-first participation produced "
            + "meaningful healer silence and one exact motivated sage interjection; the sage's "
            + "bounded input generated and recorded one speech occurrence from its scene position, "
            + "completed and advanced once, and preserved world and perception state.");
        GetTree().Quit();
    }

    private bool ProvesExactCompletedInterjection(CharacterTurnActivity activity, Character sage)
    {
        IReadOnlyList<Character> participants = participantSource.Participants;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        DialogueEvent? speechEvent = eventController.LastOccurredEvent;
        CharacterModelGenerationRequest? generation = characterModel.Request;
        bool matches = seekerShifted
            && participationModel.Requests.Count == 2
            && ReferenceEquals(
                participationModel.Requests[0].ScheduledCharacter,
                participants[0])
            && participationModel.Requests[0].IsDirectlyAddressed
            && ReferenceEquals(
                participationModel.Requests[1].ScheduledCharacter,
                sage)
            && !participationModel.Requests[1].IsDirectlyAddressed
            && generation is not null
            && ReferenceEquals(generation.TurnRequest, activity.Request)
            && ReferenceEquals(activity.Request.RespondingCharacterId, sage.Identity.Id)
            && activity.Response?.Speech?.Text == SageText
            && speechEvent is not null
            && ReferenceEquals(speechEvent.Utterance, activity.Response.Speech)
            && speechEvent.Audience.ListenerCharacterIds.Count == 1
            && ReferenceEquals(
                speechEvent.Audience.ListenerCharacterIds[0],
                GetNode<DialogueInputController>("../DialogueInputController").SpeakerCharacterId)
            && transcript is { Entries.Count: 2 }
            && ReferenceEquals(transcript.Entries[1].SpokenStatement, activity.Response.Speech)
            && budgetController.Budget is
            { CompletedTurnCount: 1, MaximumTurnCount: 2, HasPlayerInterrupted: false }
            && ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            && ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception);
        if (!matches)
        {
            Fail("The ordinary scene did not complete the exact bounded sage interjection.");
        }

        return matches;
    }

    private void Fail(string message)
    {
        SetProcess(false);
        seeker?.SetDeferred(Node2D.PropertyName.Position, initialSeekerPosition);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class SageInterjectionParticipationModel : ICharacterTurnParticipationModel
    {
        public List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);
            return ValueTask.FromResult(new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                request.IsDirectlyAddressed
                    ? CharacterTurnParticipationKind.RemainSilent
                    : CharacterTurnParticipationKind.Interject,
                request.ScheduledCharacter.Goals.Entries[0]));
        }
    }

    private sealed class SageSpeechCharacterModel : ICharacterModel
    {
        public CharacterModelGenerationRequest? Request { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Request = request;
            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.Clarify,
                new DialogueDelivery(
                    new DialogueUtterance(request.TurnRequest.RespondingCharacterId, SageText),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(SageText);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }
}
