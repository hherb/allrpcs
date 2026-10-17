using System.Net;
using System.Text;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Infrastructure.Dialogue;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>Exercises ordinary Main dialogue through the selected Ollama participation binding.</summary>
public partial class OllamaParticipationMainCheck : Node
{
    private const string PlayerText = "Healer, please say hello and wait here.";
    private const string ReplyText = "I hear you.";
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private Node main = null!;
    private string mode = "", report = "";
    private readonly List<string> checks = [];

    /// <inheritdoc />
    public override async void _Ready()
    {
        Engine.MaxFps = 60;
        mode = OS.GetEnvironment("ALLRPCS_OLLAMA_PARTICIPATION_MODE");
        report = OS.GetEnvironment("ALLRPCS_OLLAMA_PARTICIPATION_REPORT");
        try
        {
            Require(mode is "respond" or "silence" or "failure" or "live", "Supported fixture mode");
            main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
            TemporaryCharacterModelCompositionController composition = main.GetNode<TemporaryCharacterModelCompositionController>(
                "TemporaryCharacterModelCompositionController");
            OllamaParticipationHttpHandler handler = new(mode);
            HttpClient client = new(handler);
            client.Timeout = Timeout.InfiniteTimeSpan;
            Require(composition.TryInitializeTransport(client, OllamaEndpoint.Default), "Transport setup");
            AddChild(main);
            await Frames(8);
            CharacterModelSettingsController settings = main.GetNode<CharacterModelSettingsController>("CharacterModelSettingsController");
            CharacterModelSettings first = new(CharacterModelProvider.Ollama, "first-fixture");
            Require(settings.TryInitialize(new([first], CharacterModelProvider.Ollama)), "Isolated settings initialization");
            Require(composition.TryActivateCurrentBindings(), "Fresh Ollama composition");
            TemporaryCharacterTurnParticipationModelSource source = main.GetNode<TemporaryCharacterTurnParticipationModelSource>(
                "TemporaryCharacterTurnParticipationModelSource");
            ICharacterTurnParticipationModel original = source.TakeModel();
            Require(original is OllamaCharacterTurnParticipationModel, "Fresh participation adapter");
            Require(settings.TryApply(new(CharacterModelProvider.Ollama, mode == "live" ? "gemma4:e4b" : "replacement-fixture"))
                && composition.TryActivateCurrentBindings(), "Replacement composition");
            ICharacterTurnParticipationModel replacement = source.TakeModel();
            Require(replacement is OllamaCharacterTurnParticipationModel && !ReferenceEquals(original, replacement), "Participation binding replaced");
            ObservedParticipation observed = new(replacement);
            Require(source.TrySupplyModel(observed), "Observed composed adapter");
            using HttpClient lateTransport = new();
            Require(!composition.TryInitializeTransport(lateTransport, OllamaEndpoint.Default), "Late transport replacement rejected");

            CharacterBody2D healer = main.GetNode<CharacterBody2D>("Healer"), seeker = main.GetNode<CharacterBody2D>("Seeker");
            healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false);
            healer.Visible = true; // Explicit present listener in this disposable synthetic dialogue fixture.
            seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
            main.GetNode<Button>("SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed);
            await Frames(3);
            main.GetNode<CheckButton>("DialogueOverlay/Panel/Whispering").ButtonPressed = true;
            LineEdit input = main.GetNode<LineEdit>("DialogueOverlay/Panel/Input");
            input.Text = PlayerText;
            bool listenerPresentAtSubmit = healer.Visible;
            main.GetNode<Button>("DialogueOverlay/Panel/Submit").EmitSignal(BaseButton.SignalName.Pressed);
            Require(input.Text.Length == 0, "Accepted draft cleared");
            Require(main.GetNode<Label>("DialogueOverlay/Panel/Status").Text == TemporaryDialogueInputPanel.AcceptedStatusText, "Explicit Submit accepted");
            DialogueInputController delivery = main.GetNode<DialogueInputController>("DialogueInputController");
            Require(delivery.LastSubmittedDelivery?.Mode == DialogueDeliveryMode.Whispering, "Whisper mode preserved");
            CharacterTurnParticipationRoundTaskController round = main.GetNode<CharacterTurnParticipationRoundTaskController>(
                "CharacterTurnParticipationRoundTaskController");
            CharacterTurnTranscriptController transcript = main.GetNode<CharacterTurnTranscriptController>("CharacterTurnTranscriptController");
            CharacterModelGenerationFlowController generation = main.GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
            bool sawRound = false;
            ulong started = Time.GetTicksMsec();
            List<string> roundStates = [];
            while (Time.GetTicksMsec() - started < (mode == "live" ? 70_000UL : 10_000UL))
            {
                await Task.Delay(10);
                await Frames(1);
                sawRound |= round.Status != CharacterTurnParticipationRoundTaskStatus.Idle;
                string current = round.Status.ToString();
                if (roundStates.Count == 0 || roundStates[^1] != current) { roundStates.Add(current); }
                bool replied = transcript.Transcript!.Entries.Any(e => e.SpokenStatement is { } statement && statement.Text != PlayerText);
                if ((mode == "respond" && replied)
                    || (mode is "silence" or "failure" && handler.ParticipationCalls > 0 && round.Status == CharacterTurnParticipationRoundTaskStatus.Idle)
                    || (mode == "live" && (replied || observed.Failure is not null || observed.Kind == "remain-silent" || generation.Result is not null)))
                { break; }
            }
            await Frames(8);
            CharacterTurnTranscriptSnapshot final = transcript.Transcript!;
            int replies = final.Entries.Count(e => e.SpokenStatement is { } statement && statement.Text != PlayerText);
            Require(final.Entries.Count(e => e.SpokenStatement?.Text == PlayerText) == 1, "One explicit player submission");
            Require(main.GetNode<DialogueEventController>("DialogueEventController").LastOccurredEvent is not null, "Dialogue event occurred");
            Require(handler.ParticipationCalls > 0, "Ordinary participation HTTP reached: " + JsonSerializer.Serialize(new
            {
                listenerPresentAtSubmit,
                listenerPresentNow = healer.Visible,
                distance = seeker.GlobalPosition.DistanceTo(healer.GlobalPosition),
                audienceCount = main.GetNode<DialogueEventController>("DialogueEventController").LastOccurredEvent?.Audience.ListenerCharacterIds.Count,
                roundStatus = round.Status.ToString(),
                turnState = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController").Activity?.State.Value,
                generation.IsGenerating,
                hasResult = generation.Result is not null,
                handler.ParticipationCalls,
                handler.ReplyCalls,
                transcriptCount = final.Entries.Count,
            }));
            if (mode == "respond")
            {
                Require(handler.ParticipationCalls > 0 && handler.ReplyCalls > 0, "Ordinary participation reached reply HTTP");
                Require(final.Entries.Any(e => e.SpokenStatement?.Text == ReplyText), "Reply committed to ordinary transcript");
            }
            else if (mode is "silence" or "failure")
            {
                Require(handler.ParticipationCalls > 0 && handler.ReplyCalls == 0 && replies == 0, "No fabricated reply on silence/failure");
            }
            CharacterModelStreamTermination? replyTermination = generation.Result?.Updates.OfType<CharacterModelStreamTermination>().LastOrDefault();
            bool liveFailed = mode == "live" && (observed.Failure is not null || observed.Kind is null
                || (observed.Kind == "respond" && replyTermination?.Status != CharacterModelStreamTerminationStatus.Completed));
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                mode,
                checks,
                status = liveFailed ? "failed" : "passed",
                elapsedMilliseconds = Time.GetTicksMsec() - started,
                roundStates,
                sawRound,
                replies,
                participationCalls = handler.ParticipationCalls,
                replyCalls = handler.ReplyCalls,
                liveOutcome = mode == "live" ? replies > 0 ? "reply committed" : "no reply committed" : "synthetic HTTP",
                replyTermination = replyTermination?.Status.ToString(),
                replyFailure = replyTermination?.Failure?.Code.ToString(),
                participationOutcome = observed.Kind,
                participationFailure = observed.Failure,
                httpStatus = handler.LastHttpStatus,
                providerError = handler.ProviderError,
                nativeMessageFields = handler.NativeMessageFields,
                personalAudioCaptured = false
            }, JsonOptions));
            GD.Print((liveFailed ? "DIAGNOSTIC FAILURE: " : "PASS: ") + "Ollama participation Main: " + mode + " · replies=" + replies);
            main.QueueFree(); await Frames(3); GetTree().Quit(liveFailed ? 1 : 0);
        }
        catch (Exception exception)
        {
            GD.PushError("Ollama participation Main: " + mode + ": " + exception);
            GetTree().Quit(1);
        }
    }

    private async Task Frames(int count)
    { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }

    private void Require(bool condition, string check)
    { if (!condition) { throw new InvalidOperationException(check); } checks.Add(check); }

    private sealed class ObservedParticipation(ICharacterTurnParticipationModel inner) : ICharacterTurnParticipationModel
    {
        public string? Kind { get; private set; }
        public string? Failure { get; private set; }
        public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(
            CharacterTurnParticipationConsiderationRequest request, CancellationToken token)
        {
            try
            {
                CharacterTurnParticipationDecision decision = await inner.DecideAsync(request, token).ConfigureAwait(false);
                Kind = decision.Kind.Value; return decision;
            }
            catch (Exception exception)
            { Failure = exception.GetType().Name + ": " + exception.Message; throw; }
        }
    }

}
