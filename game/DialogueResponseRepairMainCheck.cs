using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>Verifies repeated ordinary submissions across speech, silence and rejected model responses.</summary>
public partial class DialogueResponseRepairMainCheck : Node
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private static readonly string[] FictionalInputs =
    [
        "Healer, please say hello and wait here.",
        "Healer, please say hello again and wait here.",
        "Healer, please tell me about herbs, but remain here and wait.",
        "Healer, please tell me what you know, but remain here and wait.",
        "Healer, please say hello and wait here.",
        "Healer, thank you. Please wait here.",
        "Healer, please say hello again and wait here.",
        "Healer, please say hello and wait here."
    ];
    private readonly List<string> checks = [];
    private readonly List<object> turns = [];
    private Node main = null!;

    /// <inheritdoc />
    public override async void _Ready()
    {
        string mode = OS.GetEnvironment("ALLRPCS_RESPONSE_REPAIR_MODE");
        string report = OS.GetEnvironment("ALLRPCS_RESPONSE_REPAIR_REPORT");
        try
        {
            Require(mode is "controlled" or "live", "Known diagnostic mode");
            Engine.MaxFps = 60;
            main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
            OllamaParticipationHttpHandler handler = new(mode == "live" ? "live" : "repeated");
            HttpClient client = new(handler) { Timeout = Timeout.InfiniteTimeSpan };
            TemporaryCharacterModelCompositionController composition = main.GetNode<TemporaryCharacterModelCompositionController>(
                "TemporaryCharacterModelCompositionController");
            Require(composition.TryInitializeTransport(client, Allrpcs.Infrastructure.Dialogue.OllamaEndpoint.Default), "Isolated transport");
            AddChild(main);
            await Frames(8);
            CharacterModelSettingsController settings = main.GetNode<CharacterModelSettingsController>("CharacterModelSettingsController");
            Require(settings.TryInitialize(new([new(CharacterModelProvider.Ollama,
                mode == "live" ? "gemma4:e4b" : "repeat-fixture")], CharacterModelProvider.Ollama))
                && composition.TryActivateCurrentBindings(), "Actual Ollama composition");
            await CheckCompletionWithoutBudget();

            CharacterBody2D healer = main.GetNode<CharacterBody2D>("Healer");
            CharacterBody2D seeker = main.GetNode<CharacterBody2D>("Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false);
            main.GetNode<Button>("SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(3);
            main.GetNode<CheckButton>("DialogueOverlay/Panel/Whispering").ButtonPressed = true;
            LineEdit input = main.GetNode<LineEdit>("DialogueOverlay/Panel/Input");
            CharacterModelGenerationFlowController generation = main.GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
            CharacterTurnActivityController activity = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
            CharacterTurnTranscriptController transcript = main.GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
            AutonomousConversationBudgetController budget = main.GetNode<AutonomousConversationBudgetController>("AutonomousConversationBudgetController");
            CharacterTurnParticipationRoundTaskController round = main.GetNode<CharacterTurnParticipationRoundTaskController>("CharacterTurnParticipationRoundTaskController");
            for (int index = 0; index < FictionalInputs.Length; index++)
            {
                await Idle(generation, activity, round);
                CharacterModelStreamSnapshot? previous = generation.Result;
                int beforeCalls = handler.ReplyCalls;
                int beforeStatements = SpokenCount(transcript);
                seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
                input.Text = FictionalInputs[index];
                main.GetNode<Button>("DialogueOverlay/Panel/Submit").EmitSignal(BaseButton.SignalName.Pressed);
                Require(input.Text.Length == 0, "Explicit submission accepted: " + (index + 1));
                ulong started = Time.GetTicksMsec();
                await Until(() => generation.Result is not null && !ReferenceEquals(previous, generation.Result),
                    mode == "live" ? 70_000UL : 10_000UL, "Fresh generation: " + (index + 1));
                CharacterModelStreamTermination terminal = generation.Result!.Updates.OfType<CharacterModelStreamTermination>().Last();
                bool rejected = mode == "controlled" && index == 3;
                Require(terminal.Status == (rejected ? CharacterModelStreamTerminationStatus.Failed : CharacterModelStreamTerminationStatus.Completed),
                    "Expected fresh generation outcome: " + (index + 1));
                if (rejected) { Require(terminal.Failure?.Code == CharacterModelFailureCode.InvalidResponse, "Malformed pair stays rejected"); }
                await Idle(generation, activity, round);
                Require(handler.ReplyCalls == beforeCalls + 1, "Exactly one model request per explicit submission");
                bool silent = terminal.Response?.Speech is null;
                int expectedStatements = beforeStatements + 1 + (silent ? 0 : 1);
                Require(SpokenCount(transcript) == expectedStatements, "Exact player/reply statement count");
                Require(budget.Budget?.CompletedTurnCount == (rejected ? 0 : 1), "Exact fresh-chain completion count");
                if (mode == "controlled" && index is 2 or 5) { Require(silent && !rejected, "Valid silent response completed"); }
                if (mode == "controlled" && index == 1)
                { Require(terminal.Response?.Delivery?.Mode == DialogueDeliveryMode.Whispering, "Whisper reply metadata retained"); }
                if (mode == "controlled" && index == 0)
                { Require(terminal.Response?.Delivery?.Mode == DialogueDeliveryMode.OrdinarySpeech, "Ordinary reply metadata retained"); }
                int count = SpokenCount(transcript);
                await Frames(6);
                Require(SpokenCount(transcript) == count && budget.Budget?.CompletedTurnCount == (rejected ? 0 : 1),
                    "No duplicate speech or budget completion");
                turns.Add(new
                {
                    turn = index + 1,
                    newResult = true,
                    elapsedMilliseconds = Time.GetTicksMsec() - started,
                    termination = terminal.Status.ToString(),
                    failure = terminal.Failure?.Code.ToString(),
                    speechPresent = !silent,
                    state = activity.Activity!.State.Value,
                    replyCalls = handler.ReplyCalls - beforeCalls,
                    completedTurnCount = budget.Budget?.CompletedTurnCount,
                    spokenStatements = SpokenCount(transcript) - beforeStatements
                });
            }
            await Idle(generation, activity, round);
            main.SetProcess(false);
            main.QueueFree();
            await Frames(10);
            await Task.Delay(100);
            await Frames(3);
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                mode,
                status = "passed",
                checks,
                turns,
                fictionalInputsOnly = true,
                personalAudioCaptured = false,
                speechPlaybackTested = false
            }, JsonOptions));
            GD.Print("PASS: dialogue response repair Main: " + mode + " · distinct generations=" + turns.Count);
            GetTree().Quit();
        }
        catch (Exception exception)
        {
            File.WriteAllText(report, JsonSerializer.Serialize(new { mode, status = "failed", checks, turns, failure = exception.Message }, JsonOptions));
            GD.PushError("Dialogue response repair Main: " + exception);
            if (IsInstanceValid(main)) { main.QueueFree(); await Frames(5); }
            GetTree().Quit(1);
        }
    }

    private async Task CheckCompletionWithoutBudget()
    {
        CharacterTurnActivityController activity = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
        AutonomousConversationBudgetController budget = main.GetNode<AutonomousConversationBudgetController>("AutonomousConversationBudgetController");
        CharacterTurnTranscriptController transcript = main.GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
        DialogueEventController events = main.GetNode<DialogueEventController>("DialogueEventController");
        TemporaryCharacterTurnResponseSpeechRecorder recorder = main.GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("TemporaryCharacterTurnResponseSpeechRecorder");
        recorder.SetProcess(false);
        CharacterId player = new("manual-player"), healer = new("manual-healer");
        DialogueUtterance utterance = new(player, "A fictional silent turn.");
        DialogueEvent heard = new(new(utterance, DialogueDeliveryMode.Whispering), new(player, [healer]));
        CharacterTurnRequest request = new(healer, heard, new(utterance, DialogueActType.Request));
        var before = events.LastOccurredEvent;
        int count = SpokenCount(transcript);
        Require(budget.Budget is null && activity.TryStart(request), "Unbudgeted manual turn started");
        Require(activity.TryReceiveResponse(new(request, CharacterTurnDisposition.Accept, null,
            new(CharacterIntentionType.Wait, null), [], [])), "Silent response received");
        await Frames(5);
        Require(activity.Activity?.State == CharacterTurnActivityState.Completed && budget.Budget is null,
            "Silent response completes without a budget");
        Require(ReferenceEquals(events.LastOccurredEvent, before) && SpokenCount(transcript) == count,
            "Silence creates no speech event or transcript statement");
        Require(activity.TryStart(request), "Next turn admitted after unbudgeted silence");
        DialogueDelivery speech = new(new(healer, "A fictional unrecorded reply."), DialogueDeliveryMode.OrdinarySpeech);
        Require(activity.TryReceiveResponse(new(request, CharacterTurnDisposition.Accept, speech,
            new(CharacterIntentionType.Wait, null), [], [])), "Spoken response received without occurrence");
        await Frames(5);
        Require(activity.Activity?.State == CharacterTurnActivityState.Speaking, "Spoken turn still requires its exact speech occurrence");
        Require(activity.TryCancel(), "Manual spoken guard turn cancelled");
        recorder.SetProcess(true);
    }

    private async Task Idle(CharacterModelGenerationFlowController generation,
        CharacterTurnActivityController activity, CharacterTurnParticipationRoundTaskController round)
    {
        await Frames(6);
        await Until(() => !generation.IsGenerating && CharacterTurnStartPolicy.CanStart(activity.Activity)
            && round.Status == CharacterTurnParticipationRoundTaskStatus.Idle,
            35_000UL, "Ordinary chain becomes idle");
        await Frames(4);
    }

    private static int SpokenCount(CharacterTurnTranscriptController transcript) =>
        transcript.Transcript!.Entries.Count(entry => entry.SpokenStatement is not null);

    private async Task Until(Func<bool> condition, ulong timeout, string check)
    {
        ulong started = Time.GetTicksMsec();
        while (!condition() && Time.GetTicksMsec() - started < timeout)
        { await Task.Delay(10); await Frames(1); }
        Require(condition(), check);
    }

    private async Task Frames(int count)
    { for (int index = 0; index < count; index++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }

    private void Require(bool condition, string check)
    { if (!condition) { throw new InvalidOperationException(check); } checks.Add(check); }
}
