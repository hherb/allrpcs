using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Speech;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Persistence;
using Godot;
using HttpClient = System.Net.Http.HttpClient;

namespace Allrpcs.Game;

/// <summary>Exercises the ordinary submitted dialogue path and real collision-resolved navigation.</summary>
public partial class HerbExaminationActionMainCheck : Node
{
    private static readonly JsonSerializerOptions ReportOptions = new() { WriteIndented = true };
    private const string FictionalRequest = "Healer, please examine the herb patch. Leave the herbs in place.";
    private readonly List<string> checks = [];
    private readonly List<object> observations = [];
    private Node main = null!;
    private Healer healer = null!;
    private CharacterBody2D seeker = null!;
    private GreyboxInteractionTarget target = null!;
    private HerbExaminationActionController action = null!;
    private WorldEventController world = null!;
    private CharacterTurnActivityController turns = null!;
    private CharacterModelGenerationFlowController generation = null!;
    private CharacterTurnParticipationRoundTaskController round = null!;
    private OllamaParticipationHttpHandler handler = null!;
    private LineEdit input = null!;
    private string lastCollision = "none";
    private string mode = "";
    private string report = "";

    /// <inheritdoc />
    public override async void _Ready()
    {
        mode = OS.GetEnvironment("ALLRPCS_EXAMINATION_MODE");
        report = OS.GetEnvironment("ALLRPCS_EXAMINATION_REPORT");
        try
        {
            Require(mode is "controlled" or "live", "Known mode");
            Engine.MaxFps = 60;
            main = GD.Load<PackedScene>("res://Main.tscn").Instantiate();
            handler = new(mode == "live" ? "live" : "examination");
            var composition = main.GetNode<TemporaryCharacterModelCompositionController>("TemporaryCharacterModelCompositionController");
            Require(composition.TryInitializeTransport(new HttpClient(handler) { Timeout = Timeout.InfiniteTimeSpan },
                Allrpcs.Infrastructure.Dialogue.OllamaEndpoint.Default), "Isolated Ollama HTTP transport");
            AddChild(main); await Frames(8);
            Require(main.GetNode<CharacterModelSettingsController>("CharacterModelSettingsController").TryInitialize(
                new([new(CharacterModelProvider.Ollama, mode == "live" ? "gemma4:e4b" : "examination-fixture")], CharacterModelProvider.Ollama))
                && composition.TryActivateCurrentBindings(), "Ordinary model composition activated");
            healer = main.GetNode<Healer>("Healer"); seeker = main.GetNode<CharacterBody2D>("Seeker");
            seeker.SetPhysicsProcess(false);
            healer.MotionCompleted += _ =>
            {
                if (healer.GetSlideCollisionCount() > 0)
                {
                    var collision = healer.GetSlideCollision(0);
                    lastCollision = ((Node)collision.GetCollider()).GetPath() + " at " + collision.GetPosition()
                        + " normal " + collision.GetNormal();
                }
            };
            target = main.GetNode<GreyboxInteractionTarget>("InteractionCandidates/HerbPatch");
            action = main.GetNode<HerbExaminationActionController>("HerbExaminationActionController");
            world = main.GetNode<WorldEventController>("WorldEventController");
            turns = main.GetNode<CharacterTurnActivityController>("CharacterTurnActivityController");
            generation = main.GetNode<CharacterModelGenerationFlowController>("CharacterModelGenerationFlowController");
            round = main.GetNode<CharacterTurnParticipationRoundTaskController>("CharacterTurnParticipationRoundTaskController");
            main.GetNode<Button>("SettingsOverlay/DialogueEntry").EmitSignal(BaseButton.SignalName.Pressed);
            input = main.GetNode<LineEdit>("DialogueOverlay/Panel/Input");
            main.GetNode<CheckButton>("DialogueOverlay/Panel/Whispering").ButtonPressed = true;
            await Until(() => healer.Visible, 40000, "Healer returned from the authored independent task");
            healer.CommandWait();
            Rid map = NavigationServer2D.RegionGetMap(main.GetNode<NavigationRegion2D>("GreyboxNavigation").GetRid());
            Vector2[] path = NavigationServer2D.MapGetPath(map, healer.GlobalPosition, target.GlobalPosition, true);
            observations.Add(new
            {
                target = target.GlobalPosition.ToString(),
                target.Visible,
                target.IsInteractionEnabled,
                target.IsInteractionVisible,
                target.ObjectIdText,
                healerVisible = healer.Visible,
                actor = main.GetNode<TemporaryOrdinaryCharacterParticipantSource>("TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id.Value,
                objectId = main.GetNode<TemporaryWorldObjectSource>("TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id.Value,
                closest = NavigationServer2D.MapGetClosestPoint(map, target.GlobalPosition).ToString(),
                pathPointCount = path.Length,
                pathEnd = path.Length == 0 ? "none" : path[^1].ToString()
            });
            await Submit(FictionalRequest);
            CharacterTurnResponse? response = turns.Activity?.Response;
            observations.Add(new
            {
                disposition = response?.Disposition.Value,
                intention = response?.ImmediateIntention.Type.Value,
                speech = response?.Speech is not null,
                actionStatus = action.Status,
                completed = action.CompletedCount,
                position = healer.GlobalPosition.ToString()
            });
            if (mode == "live")
            {
                bool choseAction = response?.Disposition == CharacterTurnDisposition.Accept
                    && response.ImmediateIntention.Type == CharacterIntentionType.ExamineObject;
                if (choseAction) { await Arrived(); }
                Require(!choseAction || action.CompletedCount == 1, "Local accepted examination executed");
                observations.Add(new { choseAction, executed = action.CompletedCount == 1, action.Status });
            }
            else { await Controlled(); }
            await Idle();
            await Shutdown();
            File.WriteAllText(report, JsonSerializer.Serialize(new
            {
                mode,
                status = "passed",
                checks,
                observations,
                fictionalInputsOnly = true,
                physicalMicrophoneOpened = false,
                playbackQualified = false
            }, ReportOptions));
            GD.Print("PASS: herb examination action Main: " + mode);
            GetTree().Quit();
        }
        catch (Exception error)
        {
            File.WriteAllText(report, JsonSerializer.Serialize(new { mode, status = "failed", checks, observations, failure = error.ToString() },
                ReportOptions));
            GD.PushError(error.ToString());
            if (IsInstanceValid(main)) { await Shutdown(); }
            GetTree().Quit(1);
        }
    }

    private async Task Controlled()
    {
        Require(action.Running && action.CompletedCount == 0, "Spoken examination dispatched before arrival: " + action.Status);
        int before = world.EventLog!.Entries.Count;
        Vector2 start = healer.GlobalPosition;
        await Frames(15);
        Require(healer.GlobalPosition.DistanceTo(start) > 5 && world.EventLog.Entries.Count == before,
            "Physics moves the healer without recording a distant examination");
        handler.WaitInsteadOfExamination = true;
        await Submit("Healer, please say hello and keep going.");
        Require(turns.Activity?.Response?.Speech is not null && action.Running,
            "An ordinary answer completes while examination travel continues");
        handler.WaitInsteadOfExamination = false;
        await Arrived();
        Require(action.CompletedCount == 1 && world.EventLog.Entries.Count == before + 1,
            "Exactly one world event committed after arrival");
        var first = action.LastOccurrence;
        Require(action.LastResult?.Safety == CharacterExaminationSafety.SafeToCollect
            && ReferenceEquals(action.LastResult.Occurrence, first), "Engine examination result owns the occurrence");
        Require(action.Status.Contains("herbs left in place", StringComparison.OrdinalIgnoreCase), "Visible examination result");
        await Frames(10);
        Require(ReferenceEquals(first, action.LastOccurrence), "Frames cannot replay a completed action");
        var scaffold = main.GetNode<TemporaryCharacterExaminationFlowController>("TemporaryCharacterExaminationFlowController");
        Require(scaffold.LastCollectionOccurrence is null && scaffold.LastExaminationResult is null,
            "Ordinary examination never enters automatic collection scaffolding");

        Reposition();
        handler.SilentExaminationReply = true;
        var mic = main.GetNode<PlayerSpeechController>("PlayerSpeechController");
        var capture = new SyntheticCapture();
        mic.Configure(capture, new Recognizer());
        var talk = (Button)main.GetNode<Control>("DialogueOverlay/Panel").FindChild("HoldToTalk", true, false);
        talk.EmitSignal(BaseButton.SignalName.ButtonDown);
        await Until(() => input.Text == FictionalRequest, 5000, "ASR draft received exact same fictional request");
        Require(!action.Running, "Recognition draft alone does not execute anything");
        talk.EmitSignal(BaseButton.SignalName.ButtonUp);
        await Until(() => !mic.Listening, 5000, "Synthetic capture stopped");
        await Submit(FictionalRequest);
        Require(capture.Opens == 1 && turns.Activity?.Response?.Speech is null && action.Running,
            "Explicit ASR draft submission dispatches a valid silent examination");
        var silentResponse = turns.Activity!.Response!;
        await Arrived();
        Require(action.CompletedCount == 2 && ReferenceEquals(action.LastOccurrence?.TranscriptEntry.PhysicalAction,
            silentResponse.ImmediateIntention), "Repeated silent examination retains the exact generated action");
        Require(action.LastOccurrence!.WorldEvent.EventId.Value == "herb-patch-examination-2", "Repeated events remain distinct");

        // Restore into a new controller with only the original authored definitions.
        var map = main.GetNode<SeekerMapController>("SeekerMapController");
        Require(map.TryInitialize(main.GetNode<DialogueInputController>("DialogueInputController").SpeakerCharacterId!)
            && SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(
                main.GetNode<OpeningRegionInvestigationRouteProgressController>("OpeningRegionInvestigationRouteProgressController"),
                map, main.GetNode<OpeningRegionOldShrineSceneSource>("OpeningRegionOldShrineSceneSource"))
            && main.GetNode<RepresentativeJournalCompositionCoordinator>("RepresentativeJournalCompositionCoordinator")
                .TryComposeAndInitialize(), "Disposable map, route and journal save context initialized");
        var saved = main.GetNode<SaveSnapshotCaptureController>("SaveSnapshotCaptureController").Capture() ?? throw new InvalidOperationException("Save capture unavailable");
        var fresh = new WorldEventController();
        var authored = main.GetNode<TemporaryWorldEventSource>("TemporaryWorldEventSource").WorldEvents;
        Require(fresh.TryInitialize(authored, new([]), new(null))
            && fresh.TryRestoreWorldEventState(saved.WorldEvents), "Cold event catalog restores repeated examinations");
        fresh.Free();
        int completed = action.CompletedCount;

        Reposition(); await Submit(FictionalRequest);
        Require(action.Running, "Cancelable travel started");
        action.Cancel(); await Frames(8);
        Require(!action.Running && healer.CommandMode == Allrpcs.Application.Movement.HealerCommandMode.Wait
            && action.CompletedCount == completed, "Cancellation stops owned travel without an event");

        Reposition(); await Submit(FictionalRequest); healer.CommandFollow(); await Frames(3);
        Require(!action.Running && healer.CommandMode == Allrpcs.Application.Movement.HealerCommandMode.Follow,
            "Manual command supersedes action ownership and is preserved");
        healer.CommandWait();

        Reposition(); await Submit(FictionalRequest);
        target.IsInteractionEnabled = false; await Frames(3);
        Require(!action.Running && action.Status.Contains("no longer available"), "Target invalidation fails visibly");
        target.IsInteractionEnabled = true;
        Vector2 targetPosition = target.GlobalPosition;
        target.GlobalPosition = new(-100, -100);
        await Submit(FictionalRequest);
        Require(!action.Running && action.Status.Contains("cannot reach"), "Unreachable target fails visibly");
        target.GlobalPosition = targetPosition;

        Reposition(); await Submit(FictionalRequest); healer.SetPhysicsProcess(false);
        await Until(() => !action.Running, 6000, "Stationary obstruction has a finite failure");
        healer.SetPhysicsProcess(true);
        Require(action.CompletedCount == completed, "Blocked travel never records an examination");

        Reposition(); await Submit(FictionalRequest);
        Require(main.GetNode<SaveSnapshotJsonController>("SaveSnapshotJsonController").TryRestoreSerialized(
            Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonSerializer.Serialize(saved)),
            "Successful complete JSON snapshot load during travel");
        await Frames(8);
        Require(!action.Running && action.LastOccurrence is null && action.CompletedCount == completed,
            "Load fences stale examination and clears its presentation token");
        action.Offer(turns.Activity!); await Frames(4);
        Require(!action.Running, "Pre-load response cannot restart an examination");

        Reposition();
        handler.ReplyDelayMilliseconds = 500;
        int beforeCalls = handler.ReplyCalls;
        Task delayed = Submit(FictionalRequest);
        await Until(() => handler.ReplyCalls > beforeCalls, 5000, "Generation in flight before load");
        Require(main.GetNode<SaveSnapshotJsonController>("SaveSnapshotJsonController").TryRestoreSerialized(
            Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonSerializer.Serialize(saved)), "Load during pending generation");
        await delayed;
        handler.ReplyDelayMilliseconds = 0;
        Require(!action.Running && action.CompletedCount == completed,
            "A delayed pre-load response cannot launch stale movement");

        Reposition(); await Submit(FictionalRequest);
        // A newer accepted examination replaces the earlier one; one final event is recorded.
        await Submit(FictionalRequest);
        var latest = turns.Activity!.Response!;
        await Arrived();
        Require(action.CompletedCount == completed + 1
            && ReferenceEquals(action.LastOccurrence?.TranscriptEntry.PhysicalAction, latest.ImmediateIntention),
            "Replacement completes only the latest accepted action");
        action.Offer(turns.Activity!); await Frames(5);
        Require(!action.Running && action.CompletedCount == completed + 1, "Duplicate offer cannot replay execution");
        Require(world.EventLog!.Entries.All(id => id.Value != "healer-collected-herb-patch"),
            "All examine-only requests leave collection absent");
        handler.WaitInsteadOfExamination = true; handler.SilentExaminationReply = false;
        await Submit("Healer, please answer hello.");
        Require(turns.Activity?.Response?.Speech is not null && !action.Running,
            "Ordinary dialogue remains usable after every terminal action outcome");
        await mic.Shutdown;
    }

    private void Reposition()
    {
        healer.CommandWait();
        healer.GlobalPosition = new(2000, 950);
    }

    private async Task Submit(string text)
    {
        await Idle();
        seeker.GlobalPosition = healer.GlobalPosition + new Vector2(12, 0);
        var previous = generation.Result;
        int calls = handler.ReplyCalls;
        input.Text = text;
        main.GetNode<Button>("DialogueOverlay/Panel/Submit").EmitSignal(BaseButton.SignalName.Pressed);
        await Until(() => generation.Result is not null && !ReferenceEquals(generation.Result, previous),
            mode == "live" ? 70000UL : 10000UL, "Fresh generation for explicit submission");
        Require(generation.Result!.Updates.OfType<CharacterModelStreamTermination>().Last().Status
            == CharacterModelStreamTerminationStatus.Completed, "Valid response received");
        await Idle();
        Require(handler.ReplyCalls == calls + 1, "Exactly one generation per submitted request");
        Require(main.GetNode<DialogueInputController>("DialogueInputController").LastSubmittedUtterance!.Text == text,
            "Exact submitted words retained by existing dialogue owner");
    }

    private async Task Arrived()
    {
        await Until(() => !action.Running, 25000, "Action reached a terminal outcome");
        observations.Add(new
        {
            action.Status,
            position = healer.GlobalPosition.ToString(),
            destination = target.GlobalPosition.ToString(),
            action.CompletedCount,
            lastCollision
        });
        Require(healer.GlobalPosition.DistanceTo(target.GlobalPosition) <= healer.DestinationStopDistancePixels,
            "Committed examination is physically at the patch: " + action.Status + " at " + healer.GlobalPosition);
        Require(action.LastOccurrence is not null && action.Status.Contains("examined"), "Arrived action committed with feedback");
        string screenshot = OS.GetEnvironment("ALLRPCS_EXAMINATION_SCREENSHOT");
        if (screenshot.Length > 0 && action.CompletedCount == 1)
        {
            seeker.GlobalPosition = healer.GlobalPosition + new Vector2(40, 0);
            await Frames(8);
            var label = main.GetNode<Label>("DialogueOverlay/Panel/ActionStatus");
            Require(label.Visible && GetViewport().GetVisibleRect().Encloses(label.GetGlobalRect()),
                "Action result is visible inside the native viewport");
            Require(GetViewport().GetTexture().GetImage().SavePng(screenshot) == Error.Ok, "Native action screenshot saved");
        }
    }
    private async Task Idle()
    {
        await Frames(3);
        await Until(() => !generation.IsGenerating && CharacterTurnStartPolicy.CanStart(turns.Activity)
            && round.Status == CharacterTurnParticipationRoundTaskStatus.Idle, 35000, "Dialogue chain idle");
        await Frames(3);
    }
    private async Task Shutdown()
    {
        main.SetProcess(false); main.QueueFree(); await Frames(10); await Task.Delay(100); await Frames(3);
    }
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

    private sealed class SyntheticCapture : IMicrophoneCapture
    {
        internal int Opens;
        public async IAsyncEnumerable<SpeechAudioChunk> CaptureAsync([EnumeratorCancellation] CancellationToken token)
        {
            Opens++;
            while (true) { token.ThrowIfCancellationRequested(); yield return new(16000, new float[1600]); await Task.Delay(20, token); }
        }
    }
    private sealed class Recognizer : ISpeechRecognizer
    {
        public async IAsyncEnumerable<SpeechRecognitionEvent> RecognizeAsync(SpeechLanguage language,
            IAsyncEnumerable<SpeechAudioChunk> audio, [EnumeratorCancellation] CancellationToken token)
        {
            await foreach (var _ in audio.WithCancellation(token))
            { yield return new(SpeechRecognitionKind.Partial, "1", FictionalRequest); }
        }
    }
}
