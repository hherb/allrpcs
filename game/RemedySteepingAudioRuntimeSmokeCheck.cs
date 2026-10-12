using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in mapped preparation, actual motion, independent UI and complete save parity proof.</summary>
public partial class RemedySteepingAudioRuntimeSmokeCheck : Node
{
    private static readonly string[] Viewports = ["1280x720", "1920x1080"];
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private OpeningRegionRemedySteepingAudioController audio = null!;
    private OpeningRegionRemedyPreparationInputCoordinator coordinator = null!;
    private FirstPatientPreparationController preparation = null!;
    private Seeker seeker = null!;
    private readonly List<object> motion = [];
    private readonly List<string> checks = [];
    private SteepingPreparationCommit? commit;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_STEEPING_MAIN_CHECK") != "1") { return; }
        try
        {
            ProcessMode = ProcessModeEnum.Always; await Frames(12);
            GetParent().SetProcess(false); GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            GetNode<Healer>("../Healer").CommandWait();
            audio = GetNode<OpeningRegionRemedySteepingAudioController>("../OpeningRegionRemedySteepingAudioController");
            coordinator = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
            preparation = GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController"); seeker = GetNode<Seeker>("../Seeker");
            GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = false; GetNode<CanvasLayer>("../SettingsOverlay").Visible = false;
            GetNode<HandToolTapRuntimeSmokeCheck>("../HandToolTapRuntimeSmokeCheck").ArrangeSave();
            seeker.GlobalPosition = new(2300, 1300); await Frames(10);
            GetNode<OpeningRegionMapVisitVerificationCoordinator>("../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit();
            string mode = OS.GetEnvironment("ALLRPCS_STEEPING_MODE");
            var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults(); var p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            if (mode == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
            settings.SetPreferences(p);
            if (mode == "missing-stream") { typeof(OpeningRegionRemedySteepingAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null); }
            if (mode == "steeping-disabled") { audio.Automatic = false; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); }
            await Frames(10); Require(audio.Bindings(), "Retained exact station unavailable.");
            Action<FootstepMotionSample> recorder = sample => motion.Add(new { sample.Tick, start = new[] { sample.Start.X, sample.Start.Y }, end = new[] { sample.End.X, sample.End.Y }, sample.Delta, sample.SpeedLimit });
            seeker.MotionCompleted += recorder;
            Input.ActionPress("move_left"); await PhysicsFrames(6); Input.ActionRelease("move_left");
            Input.ActionPress("move_right"); await PhysicsFrames(6); Input.ActionRelease("move_right");
            seeker.MotionCompleted -= recorder; Require(motion.Count > 0, "Actual post-collision motion missing.");
            var capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController"); var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            var initial = capture.Capture()!; string protectedBefore = ProtectedTruth();
            int treatment = 0; var administration = GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator");
            administration.TreatmentCompleted += _ => treatment++;
            int grinding = 0; coordinator.GrindingCompleted += (_, _, _) => grinding++;
            int heating = 0; coordinator.HeatingCompleted += _ => heating++;
            int steeping = 0;
            coordinator.SteepingCompleted += value => { steeping++; commit = value; };
            if (mode == "default") { coordinator.SteepingCompleted += _ => throw new InvalidOperationException("expected isolated steeping subscriber probe"); }
            Press(); Press(); Press(); Require(preparation.Activity!.Progress.NextStepIndex == 2, "Grinding/heating did not precede steeping.");
            Press(); Require(preparation.Activity!.Progress.NextStepIndex == 3 && commit is not null, "Exact steeping commit missing.");
            Require(audio.PresentationCount == (mode == "steeping-disabled" ? 0 : 1), "Mode changed admitted commit count.");
            if (mode != "steeping-disabled")
            {
                audio.Receive(commit); audio.Receive(commit! with { }); audio.Receive(null);
                Require(audio.PresentationCount == 1 && audio.RecentCues.Count == 1, "Duplicate/forged token changed feedback.");
            }
            Press(); Require(preparation.Activity!.Progress.NextStepIndex == 4, "Subsequent cooling blocked.");
            Require(heating == 1 && GetNode<OpeningRegionRemedyHeatingAudioController>("../OpeningRegionRemedyHeatingAudioController").PresentationCount == 1, "Steeping changed accepted heating.");
            Require(GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel").GetNode<Label>("Panel/Lines/RemedyHeating").Text == (mode == "captions-off" ? "" : "Heating preparation"), "Steeping cancelled accepted heating text.");
            Require(mode == "steeping-disabled" || GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel").GetNode<Label>("Panel/Lines/RemedySteeping").Text == (mode == "captions-off" ? "" : "Steeping preparation"), "Rapid next step cancelled steeping text.");
            Press(); Press(); Require(preparation.Outcome is not null && GetNode<FirstPatientStateController>("../FirstPatientStateController").PreparedBatches!.Batches.Count == 1, "Batch install failed.");
            Require(grinding == 1 && ProtectedTruth() == protectedBefore, "Presentation changed grinding or protected truth.");
            string views = OS.GetEnvironment("ALLRPCS_STEEPING_VIEWS");
            if (views.Length > 0) { await Views(views); }
            SaveSnapshot prepared = capture.Capture()!;
            long admitted = audio.PresentationCount;
            int ordinaryGrinding = grinding;
            int ordinarySteeping = steeping; int ordinaryHeating = heating;
            string before = SaveSnapshotJsonSerializer.Serialize(prepared);
            int history = audio.RecentCues.Count; await Frames(190);
            Require(!audio.GetNode<AudioStreamPlayer>("Steeping").Playing, "Passage did not expire.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Feedback expiry mutated durable state.");
            var patient = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource").Binding!;
            seeker.GlobalPosition = new(patient.PatientPosition.X, patient.PatientPosition.Y);
            administration._UnhandledInput(new InputEventAction { Action = "remedy_administer", Pressed = true });
            Require(treatment == 1 && GetNode<FirstPatientStateController>("../FirstPatientStateController").AdministrationExperiments!.Records.Count == 1, "Existing mapped treatment failed.");
            SaveSnapshot ordinary = capture.Capture()!; before = SaveSnapshotJsonSerializer.Serialize(ordinary); protectedBefore = ProtectedTruth();
            checks.Add("Actual mapped seven-input preparation and movement plus existing mapped treatment; unique token/forged/null rejection; isolated subscriber; next-step independence; complete save/transcript/protected truth parity.");
            if (mode == "default" && views.Length == 0) { await Lifecycle(initial); Require(restore.TryRestore(ordinary), "Ordinary snapshot reconstruction failed."); }
            string output = OS.GetEnvironment("ALLRPCS_STEEPING_REPORT");
            File.WriteAllText(output + ".save.json", before);
            File.WriteAllText(output, JsonSerializer.Serialize(new { mode, motion, checks, commits = ordinarySteeping, presentations = admitted, grinding = ordinaryGrinding, heating = ordinaryHeating, treatment, history, truthBefore = protectedBefore, truthAfter = ProtectedTruth(), transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text) }, Options));
            Require(restore.TryRestore(initial), "Exact save restore failed.");
            Require(audio.RecentCues.Count == 0 && !coordinator.OwnsSteeping(commit), "Successful load retained token/history.");
            GD.Print("PASS: steeping Main: exact mapped commit, actual motion, complete save/truth parity and independent feedback."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private async Task Lifecycle(SaveSnapshot initial)
    {
        var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        var scene = GetNode<OpeningRegionRemedyPreparationSceneSource>("../OpeningRegionRemedyPreparationSceneSource");
        void Start() { Require(restore.TryRestore(initial), "Lifecycle restore failed."); seeker.GlobalPosition = new(2300, 1300); settings.RestoreDefaults(); Press(); Press(); Press(); }
        Start(); long count = audio.PresentationCount;
        seeker.GlobalPosition = new(2349, 1300); Press(); Require(preparation.Activity!.Progress.NextStepIndex == 2 && audio.PresentationCount == count, "49px input changed preparation.");
        seeker.GlobalPosition = new(2348, 1300); Press(); Require(preparation.Activity!.Progress.NextStepIndex == 3 && audio.PresentationCount == count + 1, "48px input not admitted.");
        Start(); count = audio.PresentationCount; seeker.GlobalPosition = new(2349, 1300);
        var binding = scene.Binding!;
        Require(coordinator.TryProgress(binding, binding.TreatmentContext, binding.Recipe, preparation.Ingredients!, GetNode<FirstPatientStateController>("../FirstPatientStateController").Capture()!, new(2300, 1300)), "Existing supplied-position gameplay result changed.");
        Require(audio.PresentationCount == count && audio.RecentCues.Count == 0, "Forged supplied position acquired presentation.");
        Start(); count = audio.PresentationCount; preparation.TryAdvance(binding.Recipe.Plan.Steps[2]);
        Require(audio.PresentationCount == count, "Direct controller advance emitted ordinary notification.");
        Start(); count = audio.PresentationCount; GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Open(); Press();
        Require(preparation.Activity!.Progress.NextStepIndex == 3 && audio.PresentationCount == count && audio.RecentCues.Count == 0, "Blocked event presented.");
        GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false); audio.Receive(commit); Require(audio.PresentationCount == count, "Blocked event replayed.");
        Start(); Press(); int retained = audio.RecentCues.Count;
        GetTree().Paused = true; audio._Process(.01); Require(!audio.GetNode<AudioStreamPlayer>("Steeping").Playing && audio.RecentCues.Count == retained, "Pause cancellation lost history."); GetTree().Paused = false;
        audio._Notification((int)NotificationApplicationFocusOut); audio._Notification((int)NotificationApplicationFocusIn); audio.Receive(commit);
        Require(audio.RecentCues.Count == retained, "Focus cancellation replayed history.");
        scene.Position += Vector2.One; Require(!audio.Bindings(), "Moved station admitted."); scene.Position -= Vector2.One;
        scene.Visible = false; Require(!audio.Bindings(), "Hidden station admitted."); scene.Visible = true;
        var table = scene.GetNode<Node2D>("PreparationTable"); table.Position += Vector2.One; Require(!audio.Bindings(), "Table drift admitted."); table.Position -= Vector2.One;
        var old = audio.RecentCues.ToArray(); var unknown = new Allrpcs.Domain.Identifiers.WorldEventId("unknown-steeping-lifecycle");
        SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory, initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
        Require(!restore.TryRestore(rejected) && audio.RecentCues.SequenceEqual(old), "Failed load changed history.");
        bool refused = false; try { audio.DiagnosticPlay(); } catch (InvalidOperationException) { refused = true; }
        Require(refused, "Live diagnostic injection admitted.");
        Require(restore.TryRestore(initial) && audio.RecentCues.Count == 0 && !coordinator.OwnsSteeping(commit), "Successful reset retained token/history.");
        await Frames(2);
        checks.Add("Live 48/49px boundaries, forged position, direct-controller advance, blocked/consumed notification, pause/focus, station/table drift, failed/successful load and live-injection rejection.");
    }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private void Press() => coordinator._UnhandledInput(new InputEventAction { Action = "remedy_prepare", Pressed = true });
    private string ProtectedTruth() => JsonSerializer.Serialize(new
    {
        objects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
        inventory = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Inventory,
        healerInventory = GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource").Character.Inventory,
        perception = GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController").Perception,
        knowledge = GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource").Character.Knowledge,
        beliefs = GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource").Character.Beliefs,
        placements = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController").Placements,
        moveEvidence = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController").Evidence,
        concealment = GetNode<CharacterObjectSabotageConcealmentController>("../CharacterObjectSabotageConcealmentController").Concealment,
        substitutions = GetNode<CharacterObjectSabotageSubstitutionController>("../CharacterObjectSabotageSubstitutionController").Substitutions,
        contamination = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController").Contamination,
        contaminationEvidence = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController").Evidence,
        transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript,
        events = GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent,
        time = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController").Capture()?.WorldEvents
    }, Options);

    private async Task Views(string directory)
    {
        Directory.CreateDirectory(directory); audio.Automatic = false; audio.SetProcess(false); seeker.SetPhysicsProcess(false);
        GetNode<Healer>("../Healer").SetPhysicsProcess(false);
        foreach (string name in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionFootstepAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionHandToolTapCoordinator", "OpeningRegionHealerFootstepAudioController", "OpeningRegionFoliageAudioController", "OpeningRegionWindAudioController", "OpeningRegionRainAudioController", "OpeningRegionRemedyHeatingAudioController" })
        { GetNode<Node>("../" + name).SetProcess(false); GetNode<Node>("../" + name).SetPhysicsProcess(false); }
        var captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
        foreach (var size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); await Frames(5);
            captions.SetRegion("Wind, rain and timber creak nearby; quiet sounds continue along the woodland edge"); captions.SetAction("Grinding", 8); captions.SetFootstep("Footsteps on earth");
            captions.SetConversation("Conversation nearby"); captions.SetInterior("Interior sounds"); captions.SetWaterContact("Water contact");
            captions.SetCaskCover("Container and cloth handling"); captions.SetHandToolTap("Wooden mallet tap"); captions.SetHealerFootstep("Footsteps on earth");
            captions.SetFoliage("Leaves rustling"); captions.SetWind("Wind blowing"); captions.SetRain("Rain falling"); captions.SetRemedyHeating("Heating preparation"); captions.SetRemedySteeping("Steeping preparation");
            await Frames(2);
            var lines = captions.GetNode<VBoxContainer>("Panel/Lines");
            Require(lines.GetChildCount() == 14, "Native fourteenth owner missing.");
            Require(lines.GetNode<Label>("Region").GetLineCount() > 1, "Native wrapping was not exercised.");
            foreach (Label line in lines.GetChildren().OfType<Label>()) { Require(line.Text.Length > 0 && captions.GetNode<Control>("Panel").GetGlobalRect().Encloses(line.GetGlobalRect()), "Native caption missing/clipped: " + line.Name); }
            Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Caption panel outside viewport.");
            await Capture(Path.Combine(directory, $"captions-{size.X}.png"));
            var store = (RemedySteepingCueHistory)typeof(OpeningRegionRemedySteepingAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
            store.Clear(); for (int i = 0; i < 20; i++) { store.Record(); }
            Require(store.Entries.Count == 16 && store.Entries[0] == "5. Steeping preparation", "Native history capacity failed.");
            var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); panel.Open(); await Frames(2);
            var origin = (Button)panel.FindChild("RecentSoundCues", true, false); origin.EmitSignal(BaseButton.SignalName.Pressed); await Frames(3);
            var view = (PositionalSoundCueHistoryPanel)panel.FindChild("PositionalSoundCueHistoryPanel", true, false);
            Require(view.Visible, "History inaccessible.");
            var entries = (RichTextLabel)view.FindChild("Entries", true, false); var back = (Button)view.FindChild("CloseHistory", true, false);
            Require(back.HasFocus() && entries.Text.Contains("20. Steeping preparation", StringComparison.Ordinal), "History selection/admission missing.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false }); Require(entries.HasFocus(), "Tab lost history focus.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false }); Require(back.HasFocus(), "Shift-Tab lost return focus.");
            Require(entries.SelectionEnabled && entries.GetVScrollBar().MaxValue > entries.GetVScrollBar().Page, "History not scrollable/selectable.");
            entries.GetVScrollBar().Value = entries.GetVScrollBar().MaxValue;
            typeof(OpeningRegionRemedySteepingAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null);
            settings.SetPreferences(new(settings.Preferences.WithChannel(AudioCategory.Master, 1, true).Channels, settings.Preferences.Preset, false, false));
            await Frames(2); Require(view.Visible && entries.Text.Contains("20. Steeping preparation", StringComparison.Ordinal), "Mute/captions-off hid history.");
            await Capture(Path.Combine(directory, $"history-{size.X}.png"));
            var priorHistory = (RemedyHeatingCueHistory)typeof(OpeningRegionRemedyHeatingAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(GetNode<OpeningRegionRemedyHeatingAudioController>("../OpeningRegionRemedyHeatingAudioController"))!;
            priorHistory.Record(); int priorCount = priorHistory.Entries.Count; audio.ResetAfterLoad();
            Require(audio.RecentCues.Count == 0 && priorHistory.Entries.Count == priorCount, "Steeping reset changed heating history.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = false });
            if (view.Visible) { panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true }); await Frames(2); }
            Require(!view.Visible, "Escape failed to close history."); await Frames(2); Require(GetViewport().GuiGetFocusOwner() == origin, "History return focus lost."); panel.Close(false); settings.RestoreDefaults();
        }
        File.WriteAllText(Path.Combine(directory, "native.json"), JsonSerializer.Serialize(new { status = "passed", viewports = Viewports, captionOwners = 14, capacity = 16, returnFocus = true, keyboard = true, scrolling = true, mutedCaptionOffHistory = true }, Options));
    }
    private async Task Capture(string path) { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); GetViewport().GetTexture().GetImage().SavePng(path); }
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); } }
    private async Task PhysicsFrames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
}
