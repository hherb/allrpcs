using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in genuine dialogue/examination/collection parity and accessible sixteenth-line fixture.</summary>
public partial class HerbCollectionAudioRuntimeSmokeCheck : Node
{
    private static readonly string[] Viewports = ["1280x720", "1920x1080"];
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private OpeningRegionHerbCollectionAudioController audio = null!;
    private Seeker seeker = null!;
    private readonly List<string> checks = [];

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_HERB_MAIN_CHECK") != "1") { return; }
        try
        {
            ProcessMode = ProcessModeEnum.Always;
            var flow = GetNode<TemporaryCharacterExaminationFlowController>("../TemporaryCharacterExaminationFlowController");
            flow.SetProcess(false);
            await Frames(12);
            audio = GetNode<OpeningRegionHerbCollectionAudioController>("../OpeningRegionHerbCollectionAudioController");
            seeker = GetNode<Seeker>("../Seeker"); var healer = GetNode<Healer>("../Healer"); healer.CommandWait();
            var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
            GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
            foreach (string name in new[] { "DialogueOverlay", "SettingsOverlay", "SeekerMapPanel", "JournalSectionPanel" })
            { GetNode<CanvasLayer>("../" + name).Visible = false; }
            GetNode<HandToolTapRuntimeSmokeCheck>("../HandToolTapRuntimeSmokeCheck").ArrangeSave();
            string mode = OS.GetEnvironment("ALLRPCS_HERB_MODE");
            string placement = OS.GetEnvironment("ALLRPCS_HERB_PLACEMENT");
            string probe = OS.GetEnvironment("ALLRPCS_HERB_PROBE");
            bool near = placement == "near";
            if (near) { healer.Visible = true; healer.GlobalPosition = new(952, 350); seeker.GlobalPosition = new(920, 350); }
            if (probe == "actor-33") { healer.GlobalPosition = new(953, 350); }
            await Frames(10);
            GetNode<OpeningRegionMapVisitVerificationCoordinator>("../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit();
            var p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            if (mode == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
            settings.SetPreferences(p);
            if (mode == "missing-stream") { typeof(OpeningRegionHerbCollectionAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null); }
            if (mode == "audio-disabled") { audio.Automatic = false; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); }
            var budget = GetNode<AutonomousConversationBudgetController>("../AutonomousConversationBudgetController");
            Require(budget.TryInitialize(new(0, 2, false)), "Budget fixture failed.");
            var capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            var initial = capture.Capture()!; string unchanged = UnchangedState();
            if (probe == "task-arrival") { IndependentArrivalProbe(flow); GetTree().Quit(); return; }
            HerbCollectionCommit? commit = null; int notifications = 0;
            flow.CollectionCommitted += value => { commit = value; notifications++; Require(flow.OwnsCollection(value), "Notification preceded installed state."); GetParent().SetProcess(false); };
            flow.CollectionCommitted += _ => throw new InvalidOperationException("expected isolated herb observer probe");
            var input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input");
            input.Text = "Please wait while we decide what to examine.";
            input.EmitSignal(LineEdit.SignalName.TextSubmitted, input.Text);
            await Frames(1);
            var turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
            Require(turns.Activity is { Response: null } && ReferenceEquals(turns.Activity.State, CharacterTurnActivityState.AwaitingResponse), "Ordinary input did not open awaiting request.");
            var response = GetNode<TemporaryCharacterTurnResponseSource>("../TemporaryCharacterTurnResponseSource").CreateResponse(turns.Activity!.Request,
                GetNode<TemporaryWorldObjectSource>("../TemporaryWorldObjectSource").WorldObjects.Entries[0].Identity.Id);
            Require(turns.TryReceiveResponse(response), "Deterministic reply scaffolding failed.");
            if (probe == "listener-191") { seeker.GlobalPosition = new(729, 350); }
            if (probe == "listener-192") { seeker.GlobalPosition = new(728, 350); }
            // Permit the existing frame-driven examination and collection only after ordinary reply installation.
            input.ReleaseFocus(); GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            if (probe == "blocked") { GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Open(); }
            if (probe == "hidden") { GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch").Visible = false; }
            flow.SetProcess(true);
            if (probe == "direct-call")
            {
                for (int i = 0; i < 30 && flow.LastExaminationResult is null; i++) { await Frames(1); }
                flow.SetProcess(false); GetParent().SetProcess(false); DirectCallProbe(flow); GetTree().Quit(); return;
            }
            for (int i = 0; i < 30 && commit is null; i++) { await Frames(1); }
            Require(commit is not null && notifications == 1 && ReferenceEquals(flow.LastCollectionOccurrence, commit.Occurrence), "Genuine installed collection missing.");
            await Frames(2);
            long expected = near && mode != "audio-disabled" && probe is not "actor-33" and not "listener-192" and not "blocked" and not "hidden" ? 1 : 0;
            Require(audio.PresentationCount == expected && audio.RecentCues.Count == expected, "Spatial admission mismatch: " + probe + " bindings=" + audio.Bindings()
                + " source=" + GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource").Valid()
                + " body=" + typeof(OpeningRegionHerbCollectionAudioSource).GetMethod("BodiesValid", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource"), null)
                + " actor=" + healer.GlobalPosition + " listener=" + seeker.GlobalPosition
                + " exteriorActor=" + GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource").Exterior(healer.GlobalPosition)
                + " exteriorListener=" + GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource").Exterior(seeker.GlobalPosition)
                + " closest=" + NavigationServer2D.MapGetClosestPoint(GetNode<GreyboxNavigation>("../GreyboxNavigation").GetWorld2D().NavigationMap, healer.GlobalPosition)
                + " active=" + typeof(OpeningRegionHerbCollectionAudioController).GetMethod("Active", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(audio, null)
                + " exact=" + HerbCollectionPresentationProjection.IsCommit(commit!.Result, commit.Occurrence,
                    GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource").ActorId,
                    GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource").TargetId,
                    GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource").HerbPatchExaminedWorldEvent,
                    GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource").HerbPatchCollectedWorldEvent));
            Require(ReferenceEquals(turns.Activity!.State, CharacterTurnActivityState.Completed) && budget.Budget!.CompletedTurnCount == 1, "Ordinary turn completion changed.");
            Require(UnchangedState() == unchanged, "Collection feedback changed protected gameplay state.");
            audio.Receive(commit); audio.Receive(commit! with { }); audio.Receive(null);
            var forged = commit! with { Owner = new TemporaryCharacterExaminationFlowController() };
            audio.Receive(forged); forged.Owner.Free();
            Require(audio.PresentationCount == expected, "Copied/null/foreign/duplicate token admitted.");
            if (probe == "blocked") { GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false); }
            if (probe == "hidden") { GetNode<GreyboxInteractionTarget>("../InteractionCandidates/HerbPatch").Visible = true; }
            healer.GlobalPosition = near ? new(952, 350) : healer.GlobalPosition;
            if (probe.Length > 0 && expected == 0) { seeker.GlobalPosition = new(920, 350); }
            await Frames(2); audio.Receive(commit); Require(audio.PresentationCount == expected, "Discarded token replayed after repair.");
            if (mode == "audio-disabled")
            {
                audio.Automatic = true; audio.Receive(commit); Require(audio.PresentationCount == 0, "Disabled commit replayed after re-enable.");
                audio.Automatic = false;
            }
            var label = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel").GetNode<Label>("Panel/Lines/HerbCollection");
            Require(label.Text == (expected == 1 && mode != "captions-off" ? "Soft rustle" : ""), "Anonymous independent caption mismatch.");
            string complete = SaveSnapshotJsonSerializer.Serialize(capture.Capture()!);
            string gameplay = JsonSerializer.Serialize(new
            {
                log = GetNode<WorldEventController>("../WorldEventController").EventLog,
                time = GetNode<WorldEventController>("../WorldEventController").WorldTime,
                transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript,
                turns.Activity,
                budget.Budget,
                unchanged
            }, Options);
            int history = audio.RecentCues.Count;
            await Frames(64);
            if (expected == 1 && mode != "captions-off" && probe.Length == 0) { Require(label.Text == "Soft rustle", "Caption did not outlast audio."); }
            Require(!audio.GetNode<AudioStreamPlayer2D>("Rustle").Playing, "One-second sound did not expire.");
            await Frames(70); Require(label.Text.Length == 0, "Two-second caption did not expire.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == complete, "Feedback expiry changed full save.");
            string views = OS.GetEnvironment("ALLRPCS_HERB_VIEWS"); if (views.Length > 0) { await Views(views); }
            var unknown = new Allrpcs.Domain.Identifiers.WorldEventId("unknown-herb-load");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            var retained = audio.RecentCues.ToArray(); Require(!restore.TryRestore(rejected) && audio.RecentCues.SequenceEqual(retained), "Failed load reset history.");
            Require(restore.TryRestore(initial) && audio.RecentCues.Count == 0, "Successful load did not clear owned history.");
            audio.Receive(commit); Require(audio.PresentationCount == expected, "Pre-load token replayed after successful reset.");
            audio.Automatic = false; audio.Automatic = true; audio.Receive(commit); Require(audio.PresentationCount == expected, "Re-enabled owner caught up old token.");
            bool refused = false; try { audio.DiagnosticPlay(); } catch (InvalidOperationException) { refused = true; }
            Require(refused, "Live diagnostic injection accepted.");
            checks.Add("Genuine ordinary input/reply, frame-driven exact examination/collection, post-install token, exception-isolated observer, complete save/gameplay parity, copied/foreign/null/duplicate/stale tokens, failed/successful load and re-enable rejection.");
            string output = OS.GetEnvironment("ALLRPCS_HERB_REPORT");
            File.WriteAllText(output + ".save.json", complete); File.WriteAllText(output + ".gameplay.json", gameplay);
            File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                mode,
                placement,
                probe,
                checks,
                commits = notifications,
                presentations = expected,
                history,
                unchangedBefore = unchanged,
                unchangedAfter = UnchangedState(),
                scaffolding = "Deterministic reply on real awaiting request; near fixture makes retained Healer body visible and relocates actual bodies before commit, while task/party snapshots remain unchanged. No ordinary approach or picking animation claim."
            }, Options));
            GD.Print("PASS: herb Main: genuine collection, actual spatial admission, complete gameplay/save parity and independent feedback."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private void DirectCallProbe(TemporaryCharacterExaminationFlowController flow)
    {
        var result = flow.LastExaminationResult ?? throw new InvalidOperationException("Examination fixture missing.");
        var controller = GetNode<CharacterTurnWorldEventOccurrenceController>("../CharacterTurnWorldEventOccurrenceController");
        var world = GetNode<WorldEventController>("../WorldEventController");
        var transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        var collected = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource").HerbPatchCollectedWorldEvent;
        var action = Allrpcs.Application.Dialogue.DeterministicPostExaminationIntentionSelector.Select(result);
        var log = world.EventLog; var time = world.WorldTime; var text = transcript.Transcript;
        Require(!controller.TryOccurPostExaminationCollection(new(result.Occurrence, CharacterExaminationSafety.UnsafeToCollect), action, collected), "Unsafe direct call committed.");
        Require(ReferenceEquals(log, world.EventLog) && ReferenceEquals(time, world.WorldTime) && ReferenceEquals(text, transcript.Transcript), "Failed collection changed installed state.");
        Require(controller.TryOccurPostExaminationCollection(result, action, collected), "Exact direct collection fixture failed.");
        Require(flow.LastCollectionOccurrence is null && flow.LastCollectionCommit is null && audio.PresentationCount == 0, "Direct controller call manufactured a producer notification.");
        var rebuilt = new HerbCollectionCommit(flow, result, controller.LastOccurrence!, world.EventLog!, world.WorldTime!, transcript.Transcript!);
        audio.Receive(rebuilt); Require(audio.RecentCues.Count == 0 && audio.PresentationCount == 0, "Rebuilt installed token gained authority.");
        FinishBoundary("direct-call", "Existing direct commit and rejected unsafe/failure are preserved; neither a direct call nor rebuilt installed snapshots notify the flow-owned presentation.");
    }
    private void IndependentArrivalProbe(TemporaryCharacterExaminationFlowController flow)
    {
        GetParent().SetProcess(false);
        var task = GetNode<CharacterIndependentTaskDepartureController>("../CharacterIndependentTaskDepartureController");
        var travel = GetNode<CharacterIndependentTaskTravelController>("../CharacterIndependentTaskTravelController");
        var occurrence = GetNode<CharacterIndependentTaskOccurrenceController>("../CharacterIndependentTaskOccurrenceController");
        var source = GetNode<TemporaryCharacterIndependentTaskOccurrenceSource>("../TemporaryCharacterIndependentTaskOccurrenceSource");
        var world = GetNode<WorldEventController>("../WorldEventController");
        var transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        var candidate = source.CreateOccurrence(task.ActiveActivity!);
        Require(occurrence.TryArriveAndOccur(candidate, travel.Plan!, travel.Progress!, travel.Plan!.Waypoints.Single(),
            task.Activities!, world.EventLog!, world.WorldTime!, transcript.Transcript!), "Existing exact task arrival fixture failed.");
        Require(occurrence.LastOccurrence is not null
            && GetNode<CharacterTurnWorldEventOccurrenceController>("../CharacterTurnWorldEventOccurrenceController").LastOccurrence is null
            && flow.LastExaminationResult is null && flow.LastCollectionCommit is null && audio.PresentationCount == 0 && audio.RecentCues.Count == 0,
            "Independent task arrival acquired collection presentation.");
        FinishBoundary("task-arrival", "Existing task arrival records its separate examination occurrence; no collection-flow result/token/caption/history is fabricated.");
    }
    private void FinishBoundary(string name, string detail)
    {
        string output = OS.GetEnvironment("ALLRPCS_HERB_REPORT");
        File.WriteAllText(output, JsonSerializer.Serialize(new { status = "passed", probe = name, detail, presentations = audio.PresentationCount, history = audio.RecentCues.Count }, Options));
        GD.Print("PASS: herb Main: " + detail);
    }

    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private string UnchangedState() => JsonSerializer.Serialize(new
    {
        objects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
        inventory = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Inventory,
        healer = GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource").Character,
        perception = GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController").Perception,
        tasks = GetNode<CharacterIndependentTaskDepartureController>("../CharacterIndependentTaskDepartureController").Activities,
        party = GetNode<CharacterIndependentTaskDepartureController>("../CharacterIndependentTaskDepartureController").Party,
        recipe = GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController").Recipe,
        ingredients = GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController").Ingredients,
        disease = GetNode<FirstPatientStateController>("../FirstPatientStateController").DiseaseTruth,
        placements = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController").Placements,
        preparation = GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController").Activity
    }, Options);

    private async Task Views(string directory)
    {
        Directory.CreateDirectory(directory); audio.Automatic = false; audio.SetProcess(false); seeker.SetPhysicsProcess(false);
        GetNode<Healer>("../Healer").SetPhysicsProcess(false);
        foreach (string name in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionFootstepAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionHandToolTapCoordinator", "OpeningRegionHealerFootstepAudioController", "OpeningRegionFoliageAudioController", "OpeningRegionWindAudioController", "OpeningRegionRainAudioController", "OpeningRegionRemedyHeatingAudioController", "OpeningRegionRemedySteepingAudioController", "OpeningRegionRemedyCoolingAudioController" })
        { GetNode<Node>("../" + name).SetProcess(false); GetNode<Node>("../" + name).SetPhysicsProcess(false); }
        var captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
        foreach (var size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); await Frames(5);
            captions.SetRegion("Wind, rain and timber creak nearby; quiet sounds continue along the woodland edge"); captions.SetAction("Grinding", 8); captions.SetFootstep("Footsteps on earth");
            captions.SetConversation("Conversation nearby"); captions.SetInterior("Interior sounds"); captions.SetWaterContact("Water contact");
            captions.SetCaskCover("Container and cloth handling"); captions.SetHandToolTap("Wooden mallet tap"); captions.SetHealerFootstep("Footsteps on earth");
            captions.SetFoliage("Leaves rustling"); captions.SetWind("Wind blowing"); captions.SetRain("Rain falling"); captions.SetRemedyHeating("Heating preparation"); captions.SetRemedySteeping("Steeping preparation"); captions.SetRemedyCooling("Cooling step committed"); captions.SetHerbCollection("Soft rustle");
            await Frames(2);
            var lines = captions.GetNode<VBoxContainer>("Panel/Lines");
            Require(lines.GetChildCount() == 16, "Native sixteenth owner missing.");
            Require(lines.GetNode<Label>("Region").GetLineCount() > 1, "Native wrapping was not exercised.");
            foreach (Label line in lines.GetChildren().OfType<Label>()) { Require(line.Text.Length > 0 && captions.GetNode<Control>("Panel").GetGlobalRect().Encloses(line.GetGlobalRect()), "Native caption missing/clipped: " + line.Name); }
            Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Caption panel outside viewport.");
            await Capture(Path.Combine(directory, $"captions-{size.X}.png"));
            var store = (HerbCollectionCueHistory)typeof(OpeningRegionHerbCollectionAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
            store.Clear(); for (int i = 0; i < 20; i++) { store.Record(); }
            Require(store.Entries.Count == 16 && store.Entries[0] == "5. Soft rustle", "Native history capacity failed.");
            var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); panel.Open(); await Frames(2);
            var origin = (Button)panel.FindChild("RecentSoundCues", true, false); origin.EmitSignal(BaseButton.SignalName.Pressed); await Frames(3);
            var view = (PositionalSoundCueHistoryPanel)panel.FindChild("PositionalSoundCueHistoryPanel", true, false);
            Require(view.Visible, "History inaccessible.");
            var entries = (RichTextLabel)view.FindChild("Entries", true, false); var back = (Button)view.FindChild("CloseHistory", true, false);
            Require(back.HasFocus() && entries.Text.Contains("20. Soft rustle", StringComparison.Ordinal), "History selection/admission missing.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false }); Require(entries.HasFocus(), "Tab lost history focus.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false }); Require(back.HasFocus(), "Shift-Tab lost return focus.");
            Require(entries.SelectionEnabled && entries.GetVScrollBar().MaxValue > entries.GetVScrollBar().Page, "History not scrollable/selectable.");
            entries.GetVScrollBar().Value = entries.GetVScrollBar().MaxValue;
            typeof(OpeningRegionHerbCollectionAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null);
            settings.SetPreferences(new(settings.Preferences.WithChannel(AudioCategory.Master, 1, true).Channels, settings.Preferences.Preset, false, false));
            await Frames(2); Require(view.Visible && entries.Text.Contains("20. Soft rustle", StringComparison.Ordinal), "Mute/captions-off hid history.");
            await Capture(Path.Combine(directory, $"history-{size.X}.png"));
            var priorHistory = (RemedyHeatingCueHistory)typeof(OpeningRegionRemedyHeatingAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(GetNode<OpeningRegionRemedyHeatingAudioController>("../OpeningRegionRemedyHeatingAudioController"))!;
            priorHistory.Record(); int priorCount = priorHistory.Entries.Count;
            var priorSteeping = GetNode<OpeningRegionRemedySteepingAudioController>("../OpeningRegionRemedySteepingAudioController"); int steepingCount = priorSteeping.RecentCues.Count; audio.ResetAfterLoad();
            Require(priorSteeping.RecentCues.Count == steepingCount, "Cooling reset changed steeping history.");
            Require(audio.RecentCues.Count == 0 && priorHistory.Entries.Count == priorCount, "Cooling reset changed heating history.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            if (view.Visible) { panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true }); await Frames(2); }
            Require(!view.Visible, "Escape failed to close history."); await Frames(2); Require(GetViewport().GuiGetFocusOwner() == origin, "History return focus lost."); panel.Close(false); settings.RestoreDefaults();
        }
        File.WriteAllText(Path.Combine(directory, "native.json"), JsonSerializer.Serialize(new { status = "passed", viewports = Viewports, captionOwners = 16, capacity = 16, returnFocus = true, keyboard = true, scrolling = true, mutedCaptionOffHistory = true }, Options));
    }
    private async Task Capture(string path) { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); GetViewport().GetTexture().GetImage().SavePng(path); }
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task PhysicsFrames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
}
