using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in actual Seeker motion, seven-mode complete parity, lifecycle and native UI fixture.</summary>
public partial class FoliageAudioRuntimeSmokeCheck : Node
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Viewports = ["1280x720", "1920x1080"];
    private readonly List<object> motion = [];
    private readonly List<string> checks = [];
    private OpeningRegionFoliageAudioController audio = null!;
    private Seeker seeker = null!;
    private MaterialSoundCaptionPanel captions = null!;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_FOLIAGE_MAIN_CHECK") != "1") { return; }
        try
        {
            ProcessMode = ProcessModeEnum.Always;
            await Frames(10);
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            audio = GetNode<OpeningRegionFoliageAudioController>("../OpeningRegionFoliageAudioController");
            seeker = GetNode<Seeker>("../Seeker");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            var settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            GetNode<CanvasLayer>("../SettingsOverlay").Visible = false;
            GetNode<Healer>("../Healer").CommandWait();
            seeker.GlobalPosition = new(2250, 1395);
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); }
            await Frames(8);
            var source = GetNode<OpeningRegionFoliageAudioSource>("../OpeningRegionFoliageAudioSource");
            Require(source.Valid(), "Exact planted source unavailable.");
            Vector2 supported = NavigationServer2D.MapGetClosestPoint(GetNode<GreyboxNavigation>("../GreyboxNavigation").GetWorld2D().NavigationMap, new(2232, 1328));
            GD.Print("FOLIAGE supported listener=" + supported);
            Require(source.ListenerSupported(new(supported.X, supported.Y)) && supported.DistanceTo(new(2232, 1328)) < 160, "Garden listener unsupported.");
            seeker.GlobalPosition = supported; await Frames(8);
            settings.RestoreDefaults();
            string mode = OS.GetEnvironment("ALLRPCS_FOLIAGE_MODE");
            AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "ambience-muted") { p = p.WithChannel(AudioCategory.Ambience, 1, true); }
            if (mode == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
            settings.SetPreferences(p);
            if (mode == "missing-stream")
            { typeof(OpeningRegionFoliageAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null); }
            if (mode == "foliage-disabled") { audio.Automatic = false; }
            GetNode<HandToolTapRuntimeSmokeCheck>("../HandToolTapRuntimeSmokeCheck").ArrangeSave();
            seeker.GlobalPosition = new(2300, 1300);
            GetNode<OpeningRegionMapVisitVerificationCoordinator>("../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit();
            seeker.GlobalPosition = supported;
            await PhysicsFrames(180); audio.ResetAfterLoad();
            var capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Complete save unavailable.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial), truth = ProtectedTruth();
            long starts = audio.ScheduledStarts, presentations = audio.PresentationCount;
            Action<FootstepMotionSample> recorder = sample => motion.Add(new
            { sample.Tick, start = new[] { sample.Start.X, sample.Start.Y }, end = new[] { sample.End.X, sample.End.Y }, sample.Delta, sample.SpeedLimit });
            seeker.MotionCompleted += recorder;
            Input.ActionPress("move_left"); await PhysicsFrames(12); Input.ActionRelease("move_left");
            Input.ActionPress("move_right"); await PhysicsFrames(12); Input.ActionRelease("move_right");
            await PhysicsFrames(2240);
            seeker.MotionCompleted -= recorder;
            long consumed = audio.ScheduledStarts - starts, admitted = audio.PresentationCount - presentations;
            Require(mode == "foliage-disabled" ? consumed == 0 && admitted == 0 : consumed == 3 && admitted == 3,
                $"Wrong ordinary starts/presentations: {consumed}/{admitted}");
            Require(motion.Any(m => m is not null), "Actual motion missing.");
            if (ProtectedTruth() != truth || SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) != before)
            {
                File.WriteAllText("/private/tmp/foliage-before.json", before); File.WriteAllText("/private/tmp/foliage-after.json", SaveSnapshotJsonSerializer.Serialize(capture.Capture()!));
                File.WriteAllText("/private/tmp/foliage-truth-before.json", truth); File.WriteAllText("/private/tmp/foliage-truth-after.json", ProtectedTruth());
                throw new InvalidOperationException("Audio changed complete save/protected truth.");
            }
            checks.Add("Three actual twelve-second schedule starts; actual input/post-collision movement and complete save/transcript/protected truth parity.");
            IReadOnlyList<string> scheduledHistory = audio.RecentCues;
            Require(mode == "foliage-disabled" ? scheduledHistory.Count == 0 : scheduledHistory.Count == 3, "Mode lost accessible history.");
            if (mode == "default") { await Lifecycle(initial); }
            string views = OS.GetEnvironment("ALLRPCS_FOLIAGE_VIEWS");
            if (views.Length > 0) { await Views(views); }
            Require(ProtectedTruth() == truth && SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before,
                "Controlled checks changed protected state.");
            string output = OS.GetEnvironment("ALLRPCS_FOLIAGE_REPORT");
            File.WriteAllText(output + ".save.json", before);
            File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                mode,
                checks,
                motion,
                starts = consumed,
                presentations = admitted,
                truthBefore = truth,
                truthAfter = ProtectedTruth(),
                transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
            }, Options));
            GD.Print("PASS: foliage Main: actual schedule/motion, complete parity, exact binding and lifecycle."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task Lifecycle(SaveSnapshot initial)
    {
        var source = GetNode<OpeningRegionFoliageAudioSource>("../OpeningRegionFoliageAudioSource");
        var sprite = GetNode<Sprite2D>("../OpeningRegionRemedyPreparationSceneSource/BroadleafFoliageTopLeft");
        Vector2 original = sprite.Position;
        sprite.Position += Vector2.One; Require(!source.Valid(), "Moved planting admitted."); sprite.Position = original;
        sprite.Visible = false; Require(!source.Valid(), "Hidden planting admitted."); sprite.Visible = true;
        Texture2D texture = sprite.Texture; sprite.Texture = new ImageTexture(); Require(!source.Valid(), "Replacement texture admitted."); sprite.Texture = texture;
        int slot = sprite.GetIndex(); Node parent = sprite.GetParent(); parent.RemoveChild(sprite);
        Sprite2D replacement = (Sprite2D)sprite.Duplicate(); replacement.Name = "BroadleafFoliageTopLeft"; parent.AddChild(replacement);
        Require(!source.Valid(), "Value-equal replacement admitted."); parent.RemoveChild(replacement); replacement.Free();
        parent.AddChild(sprite); parent.MoveChild(sprite, slot);
        Require(source.Valid(), "Restored planting identity/order unavailable.");
        var nav = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        Vector2[] vertices = nav.NavigationPolygon.GetVertices(); Vector2[] changed = (Vector2[])vertices.Clone(); changed[0] += Vector2.One;
        nav.NavigationPolygon.SetVertices(changed); Require(!source.Valid(), "Navigation drift admitted."); nav.NavigationPolygon.SetVertices(vertices);
        nav.Enabled = false; Require(!source.Valid(), "Disabled map owner admitted."); nav.Enabled = true;
        Require(!source.ListenerSupported(new(2600, 1400)) && !source.ListenerSupported(new(1500, 200)), "Room listener admitted.");
        var listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener");
        RequireThrows(() => audio.DiagnosticAdvance(0.1), "Live clock injection admitted.");
        audio.ResetAfterLoad(); for (int t = 0; t < 120; t++) { audio._PhysicsProcess(0.1); }
        Require(audio.RecentCues.Count == 1, "Live admitted caption/history missing.");
        int count = audio.RecentCues.Count;
        GetTree().Paused = true; audio._Process(0.01); Require(!Playing() && audio.RecentCues.Count == count, "Pause lost history/retained audio.");
        GetTree().Paused = false; audio._Notification((int)NotificationApplicationFocusOut); Require(!Playing(), "Focus retained audio.");
        audio._Notification((int)NotificationApplicationFocusIn);
        audio._Process(double.NaN); Require(!Playing(), "Stall retained audio.");
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); panel.Open(); audio._Process(0.01);
        Require(!Playing() && audio.RecentCues.Count == count, "Reading lost history/retained audio."); panel.Close(false);
        var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        WorldEventId unknown = new("unknown-foliage-proof-event");
        SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
            initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
        Require(!restore.TryRestore(rejected) && audio.RecentCues.Count == count, "Failed load changed history.");
        Require(restore.TryRestore(initial) && audio.RecentCues.Count == 0 && !Playing(), "Load did not clear new transient owner.");
        listener.ClearCurrent(); long prior = audio.PresentationCount;
        for (int t = 0; t < 120; t++) { audio._PhysicsProcess(0.1); }
        Require(audio.PresentationCount == prior, "Noncurrent listener admitted."); listener.MakeCurrent();
        audio.ResetAfterLoad(); await PhysicsFrames(2);
        checks.Add("Static planting hide/move/replacement/texture and nav drift rejected; source/listener room support; live injection; pause/focus/stall/overlay history preservation; failed/successful load; noncurrent listener.");
    }

    private async Task Views(string directory)
    {
        Directory.CreateDirectory(directory);
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        Camera2D camera = GetNode<Camera2D>("../Seeker/FollowCamera"); camera.PositionSmoothingEnabled = false;
        camera.Zoom = new(2, 2); camera.Offset = new(0, -220); camera.ForceUpdateScroll();
        audio.ResetAfterLoad(); await PhysicsFrames(3);
        for (int t = 0; t < 120; t++) { audio._PhysicsProcess(0.1); }
        await Frame();
        Require(captions.GetNode<Label>("Panel/Lines/Foliage").Text.Contains("below", StringComparison.Ordinal), "Ordinary offscreen bearing missing.");
        await Capture(Path.Combine(directory, "ordinary-offscreen.png"));
        int retainedHistory = audio.RecentCues.Count;
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frames(3);
        Require(!GetWindow().HasFocus() && !Playing() && audio.RecentCues.Count == retainedHistory,
            "Native focus loss did not cancel audio/preserve history.");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus(); await Frames(3);
        audio.Automatic = false; audio.SetProcess(false); audio.SetPhysicsProcess(false);
        seeker.SetPhysicsProcess(false); GetNode<Healer>("../Healer").SetPhysicsProcess(false);
        foreach (string name in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionHandToolTapCoordinator", "OpeningRegionHealerFootstepAudioController" })
        { GetNode<Node>("../" + name).SetProcess(false); GetNode<Node>("../" + name).SetPhysicsProcess(false); }
        var store = (FoliageCueHistory)typeof(OpeningRegionFoliageAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
        store.Clear();
        for (int n = 0; n < 20; n++)
        { for (int t = 0; t < 120; t++) { store.Advance(0.1); } Require(store.Record("Leaves rustling below"), "History record lost."); }
        Require(store.Entries.Count == 16 && store.Entries[0].StartsWith("5.", StringComparison.Ordinal), "Capacity/oldest eviction failed.");
        Require(!store.Record("Leaves rustling right"), "Rate-limited start queued/admitted.");
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); await Frames(3);
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation above/right"); captions.SetInterior("Outdoor sounds softened");
            captions.SetWaterContact("Water contact"); captions.SetCaskCover("Container and cloth handling"); captions.SetHandToolTap("Wooden mallet contact");
            captions.SetHealerFootstep("Footsteps in leaves above/right"); captions.SetFoliage("Leaves rustling below/right"); await Frame();
            Require(captions.GetNode<Control>("Panel/Lines").GetChildCount() == 10, "Missing caption owner.");
            Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Caption panel clipped.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && captions.GetNode<Control>("Panel").GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            await Capture(Path.Combine(directory, $"ten-captions-{size.X}.png"));
            settings.SetPreferences(new AudioPreferences(settings.Preferences.Channels, settings.Preferences.Preset, false, false).WithChannel(AudioCategory.Ambience, 1, true));
            panel.Open(); ((Button)panel.FindChild("RecentSoundCues", true, false)).EmitSignal(Button.SignalName.Pressed); await Frames(3);
            var historyPanel = panel.GetNode<PositionalSoundCueHistoryPanel>("PositionalSoundCueHistoryPanel");
            RichTextLabel entries = (RichTextLabel)panel.FindChild("Entries", true, false);
            Require(entries.Text.Contains("Nearby foliage cues", StringComparison.Ordinal) && entries.Text.Contains("20. Leaves rustling below", StringComparison.Ordinal), "Muted/captions-off history missing.");
            Button back = (Button)panel.FindChild("CloseHistory", true, false);
            Require(back.HasFocus(), "Return button not focused.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            Require(entries.HasFocus(), "Tab did not reach selectable history.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false });
            Require(back.HasFocus(), "Shift-Tab did not return.");
            Require(entries.SelectionEnabled && entries.GetVScrollBar().MaxValue > entries.GetVScrollBar().Page, "History lacks selection/scrolling.");
            entries.GetVScrollBar().Value = entries.GetVScrollBar().MaxValue; await Frame();
            await Capture(Path.Combine(directory, $"history-muted-{size.X}.png"));
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Escape, Pressed = true }); await Frames(2);
            if (historyPanel.Visible) { panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true }); await Frame(); }
            Require(!historyPanel.Visible && ((Button)panel.FindChild("RecentSoundCues", true, false)).HasFocus(), "Escape/return focus failed.");
            panel.Open(); ((Button)panel.FindChild("RecentSoundCues", true, false)).EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            back.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(((Button)panel.FindChild("RecentSoundCues", true, false)).HasFocus(), "Back button return focus failed.");
            panel.Close(false); settings.RestoreDefaults();
        }
        checks.Add("Actual native focus loss cancels with history retained; native ten-caption bounds at both sizes; independent capacity/limiter; muted/captions-off selectable scrolling history; Tab/Shift-Tab/Escape/return focus.");
        File.WriteAllText(Path.Combine(directory, "native.json"), JsonSerializer.Serialize(new { status = "passed", viewports = Viewports, checks }, Options));
    }
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

    private bool Playing() => audio.GetChildren().OfType<AudioStreamPlayer2D>().Any(p => p.Playing);
    private async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private async Task Frames(int count) { for (int i = 0; i < count; i++) { await Frame(); } }
    private async Task PhysicsFrames(int count)
    {
        for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
    }
    private async Task Capture(string path) { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); Require(GetViewport().GetTexture().GetImage().SavePng(path) == Error.Ok, "Capture failed."); }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private static void RequireThrows(Action action, string message)
    { try { action(); } catch (InvalidOperationException) { return; } throw new InvalidOperationException(message); }
}
