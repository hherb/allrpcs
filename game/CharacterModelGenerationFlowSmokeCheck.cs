using System.Runtime.CompilerServices;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Proves scene-owned model generation reaches visible timeout and cancellation.</summary>
public partial class CharacterModelGenerationFlowSmokeCheck : Node
{
    private const string CancelMode = "cancel";
    private const string CheckEnvironmentVariable = "ALLRPCS_CHARACTER_MODEL_FLOW_CHECK";
    private const string CompleteMode = "complete";
    private const string FailedMode = "failed";
    private const string TimeoutMode = "timeout";
    private const int MaximumProcessFrames = 120;
    private static readonly TimeSpan CancelDeadline = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan TimeoutDeadline = TimeSpan.FromMilliseconds(50);

    private readonly CharacterModelSmokeTimeProvider timeoutClock = new();
    private CharacterModelGenerationFlowController flow = null!;
    private Main main = null!;
    private CharacterModelStreamController stream = null!;
    private TemporaryCharacterModelGenerationStatusLabel statusLabel = null!;
    private TemporaryCharacterModelStreamTextLabel textLabel = null!;
    private CharacterTurnActivityController turnController = null!;
    private CharacterTurnTranscriptController transcriptController = null!;
    private WorldEventController worldEventController = null!;
    private string mode = string.Empty;
    private int processFrames;
    private int stage;
    private object? turn;
    private object? transcript;
    private object? eventLog;
    private object? worldTime;

    /// <inheritdoc />
    public override void _Ready()
    {
        mode = OS.GetEnvironment(CheckEnvironmentVariable);
        bool isEnabled = mode is CancelMode or CompleteMode or FailedMode or TimeoutMode;
        SetProcess(isEnabled);
        if (!isEnabled)
        {
            return;
        }

        flow = GetNode<CharacterModelGenerationFlowController>(
            "../CharacterModelGenerationFlowController");
        main = GetNode<Main>("..");
        stream = GetNode<CharacterModelStreamController>("../CharacterModelStreamController");
        statusLabel = GetNode<TemporaryCharacterModelGenerationStatusLabel>(
            "../DialogueOverlay/Panel/ModelStatus");
        textLabel = GetNode<TemporaryCharacterModelStreamTextLabel>(
            "../DialogueOverlay/Panel/ModelStreamText");
        turnController = GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        transcriptController = GetNode<CharacterTurnTranscriptController>(
            "../CharacterTurnTranscriptController");
        worldEventController = GetNode<WorldEventController>("../WorldEventController");
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        processFrames++;
        if (processFrames > MaximumProcessFrames)
        {
            Fail("Character-model flow did not finish within its bounded smoke duration.");
            return;
        }

        if (stage == 0)
        {
            StartGeneration();
        }
        else if (stage == 1)
        {
            ObserveTextAndCancelWhenRequested();
        }
        else
        {
            ObserveTermination();
        }
    }

    private void StartGeneration()
    {
        CharacterModelGenerationRequest request = Request();
        bool arrangedTurn = turnController.TryStart(request.TurnRequest);
        if (mode == CancelMode)
        {
            // Hold an unrecorded spoken response so cancellation exercises a genuinely active turn.
            GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder").SetProcess(false);
            arrangedTurn = arrangedTurn
                && turnController.TryReceiveResponse(Response(request.TurnRequest));
        }

        if (!arrangedTurn)
        {
            Fail("Character-model flow smoke could not arrange its exact active turn.");
            return;
        }

        TimeSpan deadline = mode == TimeoutMode ? TimeoutDeadline : CancelDeadline;
        ICharacterModel model = mode == CompleteMode
            ? new CompletingModel(request)
            : mode == FailedMode
                ? new FailingModel()
                : new WaitingModel();

        if (!flow.TryStart(
                model,
                request,
                new WorldObjectSnapshot([]),
                new CharacterIntentionAffordanceSnapshot([]),
                deadline,
                mode == TimeoutMode ? timeoutClock : null)
            || !flow.IsGenerating
            || !flow.HasStarted
            || !ReferenceEquals(
                statusLabel.Presentation,
                CharacterModelGenerationStatusPresentation.Generating)
            || !statusLabel.Visible
            || textLabel.Visible)
        {
            Fail("Character-model flow did not enter the visible generating state.");
            return;
        }

        turn = turnController.Activity;
        transcript = transcriptController.Transcript;
        eventLog = worldEventController.EventLog;
        worldTime = worldEventController.WorldTime;
        stage = 1;
    }

    private void ObserveTextAndCancelWhenRequested()
    {
        if (mode == FailedMode)
        {
            if (flow.Result is not null)
            {
                stage = 2;
            }

            return;
        }

        if (stream.Snapshot is not { Updates.Count: >= 1 } snapshot
            || snapshot.Updates[0] is not CharacterModelTextDelta text
            || text.Text != WaitingModel.PartialText)
        {
            return;
        }

        CharacterModelGenerationStatusPresentation expectedStatus =
            mode == CompleteMode && snapshot.Updates[^1] is CharacterModelStreamTermination
                ? CharacterModelGenerationStatusPresentation.Completed
                : CharacterModelGenerationStatusPresentation.Generating;
        if (!textLabel.Visible
            || textLabel.Text != WaitingModel.PartialText
            || !ReferenceEquals(
                statusLabel.Presentation,
                expectedStatus))
        {
            Fail("Character-model flow did not present accepted streamed text while generating.");
            return;
        }

        if (mode == TimeoutMode)
        {
            // Expire the unchanged deadline only after the main thread has proved visible progress.
            timeoutClock.Advance(TimeoutDeadline);
        }

        if (mode == CancelMode)
        {
            main._UnhandledInput(new InputEventAction
            {
                Action = new StringName("character_turn_cancel"),
                Pressed = true,
            });
            if (flow.TryCancel())
            {
                Fail("Character-turn input did not prioritize active model cancellation.");
                return;
            }
        }

        stage = 2;
    }

    private void ObserveTermination()
    {
        CharacterModelStreamSnapshot? result = flow.Result;
        if (result is null)
        {
            return;
        }

        CharacterModelStreamTerminationStatus expectedStatus = mode switch
        {
            CancelMode => CharacterModelStreamTerminationStatus.Cancelled,
            CompleteMode => CharacterModelStreamTerminationStatus.Completed,
            FailedMode => CharacterModelStreamTerminationStatus.Failed,
            _ => CharacterModelStreamTerminationStatus.TimedOut,
        };
        CharacterModelGenerationStatusPresentation expectedPresentation = mode switch
        {
            CancelMode => CharacterModelGenerationStatusPresentation.Cancelled,
            CompleteMode => CharacterModelGenerationStatusPresentation.Completed,
            FailedMode => CharacterModelGenerationStatusPresentation.Failed,
            _ => CharacterModelGenerationStatusPresentation.TimedOut,
        };
        CharacterModelStreamTermination termination =
            result.Updates[^1] as CharacterModelStreamTermination
            ?? throw new InvalidOperationException("Model flow result was not terminal.");
        CharacterTurnActivity? activeTurn = turn as CharacterTurnActivity;

        if (flow.IsGenerating
            || flow.TryCancel()
            || stream.Snapshot is not { } sceneSnapshot
            || sceneSnapshot.Updates.Count != result.Updates.Count
            || !sceneSnapshot.Updates.Zip(result.Updates)
                .All(pair => ReferenceEquals(pair.First, pair.Second)))
        {
            Fail("Character-model flow did not preserve its exact completed stream result.");
            return;
        }

        if (termination.Status != expectedStatus
            || (mode == CompleteMode) != (termination.Response is not null)
            || (mode == FailedMode) != (termination.Failure is not null)
            || (mode == FailedMode
                && termination.Failure?.Code != CharacterModelFailureCode.Unavailable))
        {
            Fail("Character-model flow produced the wrong safe terminal update.");
            return;
        }

        string expectedText = mode == FailedMode ? string.Empty : WaitingModel.PartialText;
        if (!ReferenceEquals(statusLabel.Presentation, expectedPresentation)
            || statusLabel.Text != expectedPresentation.Text
            || textLabel.Text != expectedText
            || textLabel.Visible != (expectedText.Length > 0))
        {
            Fail("Character-model flow did not preserve its canonical terminal presentation.");
            return;
        }

        if (mode == CompleteMode
            && !ReferenceEquals(
                turnController.Activity?.State,
                CharacterTurnActivityState.Completed))
        {
            return;
        }

        if ((mode is TimeoutMode or FailedMode)
            && ReferenceEquals(
                turnController.Activity?.State,
                CharacterTurnActivityState.AwaitingResponse))
        {
            return;
        }

        if (mode == CancelMode
            && (activeTurn is null
                || turnController.Activity is not { } cancelledTurn
                || ReferenceEquals(cancelledTurn, activeTurn)
                || !ReferenceEquals(cancelledTurn.Request, activeTurn.Request)
                || !ReferenceEquals(cancelledTurn.Response, activeTurn.Response)
                || !ReferenceEquals(cancelledTurn.State, CharacterTurnActivityState.Cancelled)))
        {
            Fail("Character-turn input did not cancel the exact active model-backed turn.");
            return;
        }

        if ((mode is TimeoutMode or FailedMode)
            && (turn is not CharacterTurnActivity pendingTurn
                || turnController.Activity is not { } cancelledPendingTurn
                || ReferenceEquals(cancelledPendingTurn, pendingTurn)
                || !ReferenceEquals(cancelledPendingTurn.Request, pendingTurn.Request)
                || cancelledPendingTurn.Response is not null
                || !ReferenceEquals(
                    cancelledPendingTurn.State,
                    CharacterTurnActivityState.Cancelled)))
        {
            Fail("Non-successful model completion did not cancel its exact pending turn.");
            return;
        }

        if (mode == CompleteMode
            && (termination.Response is not { } completedResponse
                || turnController.Activity is not { } completedTurn
                || !ReferenceEquals(completedTurn.Request, completedResponse.Request)
                || !ReferenceEquals(completedTurn.Response, completedResponse)
                || !ReferenceEquals(completedTurn.State, CharacterTurnActivityState.Completed)
                || transcript is not CharacterTurnTranscriptSnapshot priorTranscript
                || transcriptController.Transcript is not { } completedTranscript
                || completedTranscript.Entries.Count != priorTranscript.Entries.Count + 1
                || !ReferenceEquals(
                    completedTranscript.Entries[^1].SpokenStatement,
                    completedResponse.Speech)))
        {
            Fail("Validated completed model response did not complete its exact live turn.");
            return;
        }

        if ((mode != CompleteMode
                && !ReferenceEquals(transcriptController.Transcript, transcript))
            || !ReferenceEquals(worldEventController.EventLog, eventLog)
            || !ReferenceEquals(worldEventController.WorldTime, worldTime))
        {
            Fail("Character-model flow changed surrounding authoritative scene state.");
            return;
        }

        if (!flow.TryReset()
            || flow.IsGenerating
            || flow.HasStarted
            || flow.Result is not null
            || stream.Snapshot is not { Updates.Count: 0 }
            || statusLabel.Visible
            || statusLabel.Presentation is not null
            || textLabel.Visible
            || textLabel.Presentation is not null)
        {
            Fail("Completed character-model flow did not reset to an exact hidden empty state.");
            return;
        }

        GD.Print(
            $"Character-model generation flow {mode} smoke passed: exact streamed text, "
            + "canonical visible terminal state, exact reset, independent cancellation, and "
            + "unchanged authoritative scene state.");
        GetTree().Quit();
    }

    private static CharacterModelGenerationRequest Request()
    {
        CharacterId seekerId = new("seeker");
        CharacterId healerId = new("healer");
        const string playerText = "Please wait.";
        DialogueUtterance utterance = new(seekerId, playerText);
        CharacterTurnRequest turnRequest = new(
            healerId,
            new DialogueEvent(
                new DialogueDelivery(utterance, DialogueDeliveryMode.OrdinarySpeech),
                new DialogueAudienceSnapshot(seekerId, [healerId])),
            new DialogueAct(utterance, DialogueActType.Request));
        CharacterModelContextSnapshot context = new(
            [
                new CharacterModelContextEntry(
                    CharacterModelContextEntryKind.UntrustedPlayerDialogue,
                    playerText,
                    DialogueDeliveryMode.OrdinarySpeech),
            ]);
        return new CharacterModelGenerationRequest(turnRequest, context);
    }

    private static CharacterTurnResponse Response(CharacterTurnRequest request) =>
        CompletedResponse(request);

    private static CharacterTurnResponse CompletedResponse(CharacterTurnRequest request) =>
        new(
            request,
            CharacterTurnDisposition.Accept,
            new DialogueDelivery(
                new DialogueUtterance(request.RespondingCharacterId, WaitingModel.PartialText),
                DialogueDeliveryMode.OrdinarySpeech),
            new CharacterIntention(CharacterIntentionType.Wait, null),
            [],
            []);

    private void Fail(string message)
    {
        SetProcess(false);
        GD.PushError(message);
        GetTree().Quit(exitCode: 1);
    }

    private sealed class WaitingModel : ICharacterModel
    {
        public const string PartialText = "I am considering that.";

        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _ = request;
            yield return new CharacterModelTextDelta(PartialText);
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }

    private sealed class CompletingModel(CharacterModelGenerationRequest request) : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest suppliedRequest,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!ReferenceEquals(suppliedRequest, request))
            {
                throw new InvalidOperationException("Generation request identity changed.");
            }

            await Task.CompletedTask;
            yield return new CharacterModelTextDelta(WaitingModel.PartialText);
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Completed,
                CompletedResponse(request.TurnRequest),
                null);
        }
    }

    private sealed class FailingModel : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(
            CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _ = request;
            cancellationToken.ThrowIfCancellationRequested();
            await Task.CompletedTask;
            yield return new CharacterModelStreamTermination(
                CharacterModelStreamTerminationStatus.Failed,
                null,
                new CharacterModelFailure(CharacterModelFailureCode.Unavailable));
        }
    }
}
