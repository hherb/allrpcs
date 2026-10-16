using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Persistence;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>Checks actual ordinary Main requests across conversation, action, privacy and load.</summary>
public partial class ConversationContextMainCheck : Node
{
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private readonly List<string> checks = [];
    private readonly List<object> observations = [];
    private Node main = null!;
    private Healer healer = null!;
    private CharacterBody2D seeker = null!;
    private OllamaParticipationHttpHandler handler = null!;
    private CharacterModelGenerationFlowController generation = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterTurnParticipationRoundTaskController round = null!;
    private DialogueEventController dialogue = null!;
    private string mode = "";
    private string report = "";

    /// <inheritdoc />
    public override async void _Ready()
    {
        mode = OS.GetEnvironment("ALLRPCS_CONVERSATION_MODE");
        report = OS.GetEnvironment("ALLRPCS_CONVERSATION_REPORT");
        try
        {
            Require(mode is "controlled" or "live", "Known mode");
            Engine.MaxFps = 60;
            main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
            handler = new(mode == "live" ? "live" : "examination") { WaitInsteadOfExamination = true };
            var composition = main.GetNode<TemporaryCharacterModelCompositionController>("TemporaryCharacterModelCompositionController");
            Require(composition.TryInitializeTransport(new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
                Allrpcs.Infrastructure.Dialogue.OllamaEndpoint.Default), "Isolated model transport");
            AddChild(main); await Frames(8);
            Require(main.GetNode<CharacterModelSettingsController>("CharacterModelSettingsController").TryInitialize(
                new([new(CharacterModelProvider.Ollama, mode == "live" ? "gemma4:e4b" : "conversation-fixture")], CharacterModelProvider.Ollama))
                && composition.TryActivateCurrentBindings(), "Ordinary composition activated");
            healer = main.GetNode<Healer>("Healer");
            seeker = main.GetNode<CharacterBody2D>("Seeker"); seeker.SetPhysicsProcess(false);
            generation = main.GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
            turns = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
            round = main.GetNode<CharacterTurnParticipationRoundTaskController>("CharacterTurnParticipationRoundTaskController");
            dialogue = main.GetNode<DialogueEventController>("DialogueEventController");
            main.GetNode<Button>("SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed);
            main.GetNode<CheckButton>("DialogueOverlay/Panel/Whispering").ButtonPressed = true;
            await Until(() => healer.Visible, 40000, "Healer available"); healer.CommandWait();
            healer.GlobalPosition = new(2000, 950); // Outside the sage's ordinary hearing range.
            var first = await Submit("Healer, I will call the herb patch blue fern during this conversation. Which name will I use?");
            var second = await Submit("Healer, what name am I using for the herb patch in this conversation?");
            var history = Recent();
            Require(history.Length == 2, "Second request contains the preceding player turn and committed character reply");
            Require(history[0].GetProperty("text").GetString()!.Contains("blue fern", StringComparison.Ordinal), "Earlier player words retained exactly");
            Require(history[1].GetProperty("text").GetString() == first, "Exact earlier model reply included");
            Require(history[0].GetProperty("speakerCharacterId").GetString() != history[1].GetProperty("speakerCharacterId").GetString(),
                "Player and character speaker identities differ");
            Require(history[0].GetProperty("deliveryMode").GetString() == "whispering", "Whisper metadata retained");
            if (mode == "live")
            {
                Require(second.Contains("blue fern", StringComparison.OrdinalIgnoreCase), "Selected local model resolves a context-dependent second turn");
                string third = await Submit("Healer, remind me of that name I chose for the patch. Please say the name.");
                Require(third.Contains("blue fern", StringComparison.OrdinalIgnoreCase), "Selected local model resolves a context-dependent third turn");
                await Submit("Healer, please examine the herb patch now. Leave the herbs in place.", expectSpeech: false);
                var action = main.GetNode<HerbExaminationActionController>("HerbExaminationActionController");
                await Until(() => !action.Running, 25000, "Local model examination finished");
                Require(action.LastResult?.Safety == CharacterExaminationSafety.SafeToCollect, "Local model examination committed");
                string finding = await Submit("Healer, based on the examination you just completed, is the herb patch safe to collect? Please explain your result.");
                Require(Context().GetRawText().Contains("safety=safe-to-collect", StringComparison.Ordinal),
                    "Local model follow-up received the committed result");
                Require(finding.Contains("safe", StringComparison.OrdinalIgnoreCase)
                    && !finding.Contains("unsafe", StringComparison.OrdinalIgnoreCase)
                    && !finding.Contains("need to examine", StringComparison.OrdinalIgnoreCase),
                    "Selected local model reports the recorded examination safety");
            }
            else { await Controlled(); }
            await Shutdown();
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                mode,
                status = "passed",
                checks,
                observations,
                fictionalInputsOnly = true,
                physicalMicrophoneOpened = false
            }, ReportOptions));
            GD.Print("PASS: conversation context Main: " + mode); GetTree().Quit();
        }
        catch (Exception error)
        {
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                mode,
                status = "failed",
                checks,
                observations,
                participationCalls = handler?.ParticipationCalls,
                replyCalls = handler?.ReplyCalls,
                httpStatus = handler?.LastHttpStatus,
                providerError = handler?.ProviderError,
                roundStatus = round?.Status.ToString(),
                decisions = round?.Result?.Results.Select(result => result.Decision.Kind.Value).ToArray(),
                failure = error.ToString()
            }));
            GD.PushError(error.ToString());
            if (IsInstanceValid(main)) { await Shutdown(); }
            GetTree().Quit(1);
        }
    }

    private async Task Controlled()
    {
        var participants = main.GetNode<TemporaryOrdinaryCharacterParticipantSource>("TemporaryOrdinaryCharacterParticipantSource").Participants;
        var sage = participants[1].Identity.Id;
        var healerId = participants[0].Identity.Id;
        var player = main.GetNode<DialogueInputController>("DialogueInputController").SpeakerCharacterId!;
        // Actual hearing selection, outside the healer's whisper range.
        Require(dialogue.TryOccur(new(new(sage, "PRIVATE_SECRET_DO_NOT_LEAK"), DialogueDeliveryMode.Whispering),
            new(0, 0), 300, 40, [new(player, new(12, 0)), new(healerId, new(1000, 0))]), "Private event occurred with captured hearing audience");
        Require(!dialogue.LastOccurredEvent!.Audience.ListenerCharacterIds.Contains(healerId), "Healer did not hear the private event");
        await Submit("Healer, what name have we been using?");
        Require(!handler.ReplyRequests[^1].GetRawText().Contains("PRIVATE_SECRET", StringComparison.Ordinal), "Unheard private speech absent from actual healer request");
        Require(Recent().Length == 4, "Third request retains two complete exchanges");
        var sageSpeech = new DialogueUtterance(player, "Sage, hello.");
        var sageRequest = new CharacterTurnRequest(sage,
            new(new(sageSpeech, DialogueDeliveryMode.Whispering), new(player, [sage])),
            new(sageSpeech, DialogueActType.Request));
        var source = main.GetNode<TemporaryCharacterModelGenerationInputSource>("TemporaryCharacterModelGenerationInputSource");
        var sageInput = source.CreateInput(new(sageRequest, null, CharacterTurnActivityState.AwaitingResponse))!;
        Require(sageInput.GenerationRequest.Context.Entries.Where(entry => entry.Kind == CharacterModelContextEntryKind.RecentSpeech)
            .All(entry => !entry.Text.Contains("blue fern", StringComparison.Ordinal)), "Healer whispers absent from sage context");

        handler.WaitInsteadOfExamination = false;
        await Submit("Healer, please examine the herb patch. Leave the herbs in place.");
        var action = main.GetNode<HerbExaminationActionController>("HerbExaminationActionController");
        await Until(() => !action.Running, 25000, "Examination finished");
        Require(action.LastResult?.Safety == CharacterExaminationSafety.SafeToCollect, "Engine committed the examination result");
        handler.WaitInsteadOfExamination = true;
        await Submit("Healer, what did you find? Is it safe to collect?");
        Require(Context().GetProperty("context").EnumerateArray().Any(entry => entry.GetProperty("kind").GetString() == "action-outcome"
            && entry.GetProperty("text").GetString()!.Contains("safety=safe-to-collect", StringComparison.Ordinal)),
            "Post-action follow-up receives authoritative safety result");
        sageInput = source.CreateInput(new(sageRequest, null, CharacterTurnActivityState.AwaitingResponse))!;
        Require(sageInput.GenerationRequest.Context.Entries.All(entry => !entry.Text.Contains("Completed examination:", StringComparison.Ordinal)),
            "Foreign character does not inherit the healer examination result");

        var map = main.GetNode<SeekerMapController>("SeekerMapController");
        Require(map.TryInitialize(player) && SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(
            main.GetNode<OpeningRegionInvestigationRouteProgressController>("OpeningRegionInvestigationRouteProgressController"),
            map, main.GetNode<OpeningRegionOldShrineSceneSource>("OpeningRegionOldShrineSceneSource"))
            && main.GetNode<RepresentativeJournalCompositionCoordinator>("RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(),
            "Complete disposable save context initialized");
        SaveSnapshot saved = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture()!;
        string serialized = Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonSerializer.Serialize(saved);
        var loader = main.GetNode<SaveSnapshotJsonController>("SaveSnapshotJsonController");
        Require(!loader.TryRestoreSerialized("invalid JSON"), "Rejected load");
        await Submit("Healer, continue our conversation.");
        Require(Recent().Length > 0, "Rejected load preserves current conversation");
        handler.ReplyDelayMilliseconds = 600;
        int calls = handler.ReplyCalls;
        Task<string> delayed = Submit("Healer, this is a pending pre-load turn.", expectSpeech: false);
        await Until(() => handler.ReplyCalls > calls, 5000, "Generation pending before load");
        Require(loader.TryRestoreSerialized(serialized), "Successful full save restoration");
        await delayed;
        handler.ReplyDelayMilliseconds = 0;
        Require(turns.Activity?.State == CharacterTurnActivityState.Cancelled, "Pre-load response cannot speak into the restored conversation");
        await Submit("Healer, a new conversation starts now.");
        Require(Recent().Length == 0, "Successful load clears session history and excludes stale response");
        Require(!Context().GetRawText().Contains("Completed examination:", StringComparison.Ordinal), "Successful load clears session action context");
        await Submit("Healer, please answer once more.");
        Require(Recent().Length == 2, "Dialogue resumes with fresh post-load history");
    }

    private JsonElement Context()
    {
        using var doc = JsonDocument.Parse(handler.ReplyRequests[^1].GetProperty("messages")[1].GetProperty("content").GetString()!);
        return doc.RootElement.Clone();
    }
    private JsonElement[] Recent() => Context().GetProperty("context").EnumerateArray()
        .Where(entry => entry.GetProperty("kind").GetString() == "recent-speech")
        .Select(entry => JsonSerializer.SerializeToElement(new
        {
            text = handler.ReplyRequests[^1].GetProperty("messages")[entry.GetProperty("messageIndex").GetInt32()]
                .GetProperty("content").GetString(),
            speakerCharacterId = entry.GetProperty("speakerCharacterId").GetString(),
            deliveryMode = entry.GetProperty("deliveryMode").GetString()
        })).ToArray();

    private async Task<string> Submit(string text, bool expectSpeech = true)
    {
        await Idle(); seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
        var previous = generation.Result;
        var input = main.GetNode<LineEdit>("DialogueOverlay/Panel/Input"); input.Text = text;
        main.GetNode<Button>("DialogueOverlay/Panel/Submit").EmitSignal(BaseButton.SignalName.Pressed);
        await Until(() => generation.Result is not null && !ReferenceEquals(generation.Result, previous),
            mode == "live" ? 90000UL : 10000UL, "Fresh generation");
        await Idle();
        string speech = turns.Activity?.Response?.Speech?.Text ?? "";
        Require(!expectSpeech || speech.Length > 0, "Submitted turn finished with speech");
        observations.Add(new
        {
            input = text,
            reply = speech,
            recentEntries = Recent().Length,
            respondingCharacterId = Context().GetProperty("respondingCharacterId").GetString(),
            messages = handler.ReplyRequests[^1].GetProperty("messages")
        });
        return speech;
    }
    private async Task Idle()
    {
        await Frames(3);
        await Until(() => !generation.IsGenerating && CharacterTurnStartPolicy.CanStart(turns.Activity)
            && round.Status == CharacterTurnParticipationRoundTaskStatus.Idle, 35000, "Dialogue chain idle");
        await Frames(3);
    }
    private async Task Shutdown()
    { main.SetProcess(false); main.QueueFree(); await Frames(10); await Task.Delay(100); await Frames(3); }
    private async Task Frames(int count)
    { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task Until(Func<bool> condition, ulong timeout, string check)
    {
        ulong start = Time.GetTicksMsec();
        while (!condition() && Time.GetTicksMsec() - start < timeout) { await Task.Delay(10); await Frames(1); }
        Require(condition(), check);
    }
    private void Require(bool condition, string check)
    { if (!condition) { throw new InvalidOperationException(check); } checks.Add(check); }
}
