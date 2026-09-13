using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs the first complete ordinary player-rooted autonomous dialogue chain check.</summary>
public partial class OrdinaryAutonomousDialogueFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable = "ALLRPCS_ORDINARY_AUTONOMOUS_FLOW_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Can you help me reason about this?";
    private const string HealerText = "I can help with what we have observed.";
    private const int MaximumProcessFrames = 300;

    private readonly DirectResponseParticipationModel participationModel = new();
    private readonly SpeechOnlyCharacterModel characterModel = new();
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnParticipationRoundTaskController roundController = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private DialogueEventController eventController = null!;
    private LineEdit input = null!;
    private object? initialWorldObjects;
    private object? initialPerception;
    private int processFrames;
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
            Fail("The ordinary autonomous flow could not supply its credential-free models.");
            return;
        }

        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        roundController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "../CharacterTurnParticipationRoundTaskController");
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("The ordinary autonomous dialogue flow exceeded its bounded smoke duration.");
            return;
        }

        if (!submitted)
        {
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            input.Text = PlayerText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            submitted = true;
            return;
        }

        CharacterTurnActivity? activity = turnController.Activity;
        if (activity is null
            || !ReferenceEquals(activity.State, CharacterTurnActivityState.Completed))
        {
            return;
        }

        SetProcess(false);
        IReadOnlyList<CharacterTurnParticipationConsiderationRequest> considerations =
            participationModel.Requests;
        CharacterModelGenerationRequest? generation = characterModel.Request;
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (generation is null
            || participationModel.CallCount != 2
            || characterModel.CallCount != 1
            || considerations.Count != 2
            || !considerations[0].IsDirectlyAddressed
            || considerations[1].IsDirectlyAddressed
            || !ReferenceEquals(
                considerations[0].HeardDialogueEvent,
                activity.Request.DialogueEvent)
            || !ReferenceEquals(
                considerations[1].HeardDialogueEvent,
                activity.Request.DialogueEvent)
            || !ReferenceEquals(considerations[0].ScheduledCharacter.Identity.Id,
                activity.Request.RespondingCharacterId)
            || !ReferenceEquals(generation.TurnRequest, activity.Request)
            || activity.Response?.Speech?.Text != HealerText
            || roundController.Status != CharacterTurnParticipationRoundTaskStatus.Idle
            || roundController.Result is not null
            || budgetController.Budget is not
            { CompletedTurnCount: 1, MaximumTurnCount: 2, HasPlayerInterrupted: false }
            || transcript is not { Entries.Count: 2 }
            || transcript.Entries[0].SpokenStatement?.Text != PlayerText
            || !ReferenceEquals(
                transcript.Entries[1].SpokenStatement,
                activity.Response.Speech)
            || !ReferenceEquals(eventController.LastOccurredEvent?.Utterance,
                activity.Response.Speech)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception))
        {
            Fail("The ordinary player event did not traverse the exact bounded autonomous chain.");
            return;
        }

        GD.Print(
            "Ordinary autonomous dialogue flow smoke passed: one exact player event initialized "
            + "a finite budget, obtained one direct participation decision, started and generated "
            + "the exact selected healer follow-up, recorded its speech once, completed it once, "
            + "advanced the budget once, and preserved world and perception state.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class DirectResponseParticipationModel : ICharacterTurnParticipationModel
    {
        public int CallCount { get; private set; }

        public List<CharacterTurnParticipationConsiderationRequest> Requests { get; } = [];

        public ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Requests.Add(request);
            return ValueTask.FromResult(new CharacterTurnParticipationDecision(
                request.ScheduledCharacter.Identity.Id,
                request.IsDirectlyAddressed
                    ? CharacterTurnParticipationKind.Respond
                    : CharacterTurnParticipationKind.RemainSilent,
                request.IsDirectlyAddressed
                    ? null
                    : request.ScheduledCharacter.Goals.Entries[0]));
        }
    }

    private sealed class SpeechOnlyCharacterModel : ICharacterModel
    {
        public int CallCount { get; private set; }

        public CharacterModelGenerationRequest? Request { get; private set; }

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            Request = request;
            CharacterTurnResponse response = new(
                request.TurnRequest,
                CharacterTurnDisposition.Accept,
                new DialogueDelivery(
                    new DialogueUtterance(request.TurnRequest.RespondingCharacterId, HealerText),
                    DialogueDeliveryMode.OrdinarySpeech),
                new CharacterIntention(CharacterIntentionType.Wait, null),
                [],
                []);
            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(HealerText);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                response,
                null);
        }
    }
}
