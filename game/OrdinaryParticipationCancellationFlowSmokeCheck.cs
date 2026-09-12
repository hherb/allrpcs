using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Runs ordinary player cancellation during one active participation round.</summary>
public partial class OrdinaryParticipationCancellationFlowSmokeCheck : Node
{
    private const string CheckEnvironmentVariable =
        "ALLRPCS_ORDINARY_PARTICIPATION_CANCELLATION_CHECK";
    private const string EnabledEnvironmentValue = "1";
    private const string PlayerText = "Please wait before anyone answers.";
    private const int MaximumProcessFrames = 300;
    private static readonly StringName CharacterTurnCancelAction = new("character_turn_cancel");

    private readonly WaitingModel model = new();
    private Main main = null!;
    private CharacterTurnParticipationRoundTaskController roundController = null!;
    private AutonomousConversationBudgetController budgetController = null!;
    private CharacterTurnActivityController turnController = null!;
    private DialogueEventController eventController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private LineEdit input = null!;
    private DialogueEvent? playerEvent;
    private object? initialWorldObjects;
    private object? initialPerception;
    private int processFrames;
    private int stage;

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
                "../TemporaryCharacterTurnParticipationModelSource").TrySupplyModel(model))
        {
            Fail("The cancellation smoke could not supply its waiting participation model.");
            return;
        }

        main = GetNode<Main>("..");
        roundController = GetNode<CharacterTurnParticipationRoundTaskController>(
            "../CharacterTurnParticipationRoundTaskController");
        budgetController = GetNode<AutonomousConversationBudgetController>(
            "../AutonomousConversationBudgetController");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        eventController = GetNode<DialogueEventController>("../DialogueEventController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Ordinary participation cancellation exceeded its bounded smoke duration.");
            return;
        }

        if (stage == 0)
        {
            initialWorldObjects = GetNode<WorldObjectController>(
                "../WorldObjectController").WorldObjects;
            initialPerception = GetNode<FocusedSearchActivityController>(
                "../Seeker/FocusedSearchActivityController").Perception;
            input.Text = PlayerText;
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, PlayerText);
            stage = 1;
            return;
        }

        if (stage == 1)
        {
            if (roundController.Status != CharacterTurnParticipationRoundTaskStatus.Running
                || model.Request is null)
            {
                return;
            }

            playerEvent = eventController.LastOccurredEvent;
            main._UnhandledInput(new InputEventAction
            {
                Action = CharacterTurnCancelAction,
                Pressed = true,
            });
            stage = 2;
            return;
        }

        if (roundController.Status != CharacterTurnParticipationRoundTaskStatus.Idle)
        {
            return;
        }

        SetProcess(false);
        CharacterTurnTranscriptSnapshot? transcript = transcriptController.Transcript;
        if (!model.WasCancelled
            || model.Request is null
            || playerEvent is null
            || !ReferenceEquals(model.Request.HeardDialogueEvent, playerEvent)
            || !model.Request.IsDirectlyAddressed
            || turnController.Activity is not null
            || roundController.Result is not null
            || budgetController.Budget is not
            { CompletedTurnCount: 0, MaximumTurnCount: 2, HasPlayerInterrupted: true }
            || transcript is not { Entries.Count: 1 }
            || !ReferenceEquals(transcript.Entries[0].SpokenStatement, playerEvent.Utterance)
            || !ReferenceEquals(eventController.LastOccurredEvent, playerEvent)
            || !ReferenceEquals(
                GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
                initialWorldObjects)
            || !ReferenceEquals(
                GetNode<FocusedSearchActivityController>(
                    "../Seeker/FocusedSearchActivityController").Perception,
                initialPerception))
        {
            Fail("Player cancellation did not close only the exact active autonomous chain.");
            return;
        }

        GD.Print(
            "Ordinary participation cancellation smoke passed: player input cancelled the exact "
            + "waiting participation request, interrupted its fresh budget, reset terminal task "
            + "state, started no turn or generation, retained only accepted player speech, and "
            + "preserved world and perception state.");
        GetTree().Quit();
    }

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class WaitingModel : ICharacterTurnParticipationModel
    {
        public CharacterTurnParticipationConsiderationRequest? Request { get; private set; }

        public bool WasCancelled { get; private set; }

        public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            Request = request;
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                WasCancelled = true;
                throw;
            }

            throw new InvalidOperationException("The waiting participation model completed.");
        }
    }
}
