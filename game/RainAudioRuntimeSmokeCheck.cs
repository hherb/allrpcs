using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in actual Seeker motion, seven-mode complete parity, lifecycle and native UI fixture.</summary>
public partial class RainAudioRuntimeSmokeCheck : Node
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private static readonly string[] Viewports = ["1280x720", "1920x1080"];
    private readonly List<object> motion = [];
    private readonly List<string> checks = [];
    private OpeningRegionRainAudioController audio = null!;
    private Seeker seeker = null!;
    private MaterialSoundCaptionPanel captions = null!;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_RAIN_MAIN_CHECK") != "1") { return; }
        try
        {
            ProcessMode = ProcessModeEnum.Always;
            await Frames(10);
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            audio = GetNode<OpeningRegionRainAudioController>("../OpeningRegionRainAudioController");
            seeker = GetNode<Seeker>("../Seeker");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            var settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            GetNode<CanvasLayer>("../SettingsOverlay").Visible = false;
            GetNode<Healer>("../Healer").CommandWait();
            seeker.GlobalPosition = new(1000, 350);
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); }
            await Frames(8);
            var source = GetNode<OpeningRegionRainAudioSource>("../OpeningRegionRainAudioSource");
            Require(source.Valid(), "Exact clearing source unavailable.");
            Vector2 supported = NavigationServer2D.MapGetClosestPoint(GetNode<GreyboxNavigation>("../GreyboxNavigation").GetWorld2D().NavigationMap, new(1000, 350));
            GD.Print("RAIN supported listener=" + supported);
            Require(source.ListenerSupported(new(supported.X, supported.Y)) && supported.DistanceTo(new(1000, 350)) < 256, "Clearing listener unsupported.");
            seeker.GlobalPosition = supported; await Frames(8);
            settings.RestoreDefaults();
            string mode = OS.GetEnvironment("ALLRPCS_RAIN_MODE");
            AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "ambience-muted") { p = p.WithChannel(AudioCategory.Ambience, 1, true); }
            if (mode == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
            settings.SetPreferences(p);
            if (mode == "missing-stream")
            { typeof(OpeningRegionRainAudioController).GetField("stream", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(audio, null); }
            if (mode == "rain-disabled") { audio.Automatic = false; }
            GetNode<HandToolTapRuntimeSmokeCheck>("../HandToolTapRuntimeSmokeCheck").ArrangeSave();
            seeker.GlobalPosition = new(2300, 1300);
            GetNode<OpeningRegionMapVisitVerificationCoordinator>("../OpeningRegionMapVisitVerificationCoordinator").TryVerifyCurrentVisit();
            seeker.GlobalPosition = supported;
            await PhysicsFrames(180); audio.ResetAfterLoad();
            var capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Complete save unavailable.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial), truth = ProtectedTruth();
            string nativeViews = OS.GetEnvironment("ALLRPCS_RAIN_VIEWS");
            if (nativeViews.Length > 0)
            {
                await Views(nativeViews);
                Require(ProtectedTruth() == truth && SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Native UI changed protected state.");
                GD.Print("PASS: rain native UI: twelve owners, bearings, independent history and focus."); GetTree().Quit(); return;
            }
            long starts = audio.ScheduledStarts, presentations = audio.PresentationCount;
            Action<FootstepMotionSample> recorder = sample => motion.Add(new
            { sample.Tick, start = new[] { sample.Start.X, sample.Start.Y }, end = new[] { sample.End.X, sample.End.Y }, sample.Delta, sample.SpeedLimit });
            seeker.MotionCompleted += recorder;
            Input.ActionPress("move_left"); await PhysicsFrames(12); Input.ActionRelease("move_left");
            Input.ActionPress("move_right"); await PhysicsFrames(12); Input.ActionRelease("move_right");
            await PhysicsFrames(3660);
            seeker.MotionCompleted -= recorder;
            long consumed = audio.ScheduledStarts - starts, admitted = audio.PresentationCount - presentations;
            Require(mode == "rain-disabled" ? consumed == 0 && admitted == 0 : consumed == 3 && admitted == 3,
                $"Wrong ordinary starts/presentations: {consumed}/{admitted}");
            Require(motion.Any(m => m is not null), "Actual motion missing.");
            if (ProtectedTruth() != truth || SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) != before)
            {
                File.WriteAllText("/private/tmp/rain-before.json", before); File.WriteAllText("/private/tmp/rain-after.json", SaveSnapshotJsonSerializer.Serialize(capture.Capture()!));
                File.WriteAllText("/private/tmp/rain-truth-before.json", truth); File.WriteAllText("/private/tmp/rain-truth-after.json", ProtectedTruth());
                throw new InvalidOperationException("Audio changed complete save/protected truth.");
            }
            checks.Add("Three actual twenty-second schedule starts; actual input/post-collision movement and complete save/transcript/protected truth parity.");
            IReadOnlyList<string> scheduledHistory = audio.RecentCues;
            Require(mode == "rain-disabled" ? scheduledHistory.Count == 0 : scheduledHistory.Count == 3, "Mode lost accessible history.");
            if (mode == "default") { await Lifecycle(initial); }
            string views = OS.GetEnvironment("ALLRPCS_RAIN_VIEWS");
            if (views.Length > 0) { await Views(views); }
            Require(ProtectedTruth() == truth && SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before,
                "Controlled checks changed protected state.");
            string output = OS.GetEnvironment("ALLRPCS_RAIN_REPORT");
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
            GD.Print("PASS: rain Main: actual schedule/motion, complete parity, exact binding and lifecycle."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task Lifecycle(SaveSnapshot initial)
    {
        var source = GetNode<OpeningRegionRainAudioSource>("../OpeningRegionRainAudioSource");
        var clearing = GetNode<OpeningRegionWoodlandClearingSceneSource>("../OpeningRegionWoodlandClearingSceneSource");
        clearing.Position += Vector2.One; Require(!source.Valid(), "Moved clearing admitted."); clearing.Position = Vector2.Zero;
        clearing.Visible = false; Require(!source.Valid(), "Hidden clearing admitted."); clearing.Visible = true;
        var area = clearing.GetNode<Polygon2D>("ClearingArea");
        Require(area.Visible && source.Valid(), "Retained clearing data disabled rain.");
        area.Visible = false; Require(!source.Valid(), "Altered clearing area admitted."); area.Visible = true;
        Vector2[] points = area.Polygon; area.Polygon = [Vector2.Zero, Vector2.One]; Require(!source.Valid(), "Clearing area drift admitted."); area.Polygon = points;
        var marker = clearing.GetNode<Marker2D>("ConcealedRareSpecimen");
        bool hidden = marker.Visible; marker.Visible = !hidden;
        Require(source.Valid(), "Inert specimen visibility gained audio authority."); marker.Visible = hidden;
        var collision = clearing.GetNode<Node2D>("TreeBoundaries").GetChild<StaticBody2D>(0).GetChild<CollisionShape2D>(0);
        Shape2D shape = collision.Shape; collision.Shape = shape.Duplicate() as Shape2D; Require(!source.Valid(), "Equal replacement shape admitted."); collision.Shape = shape;
        var segment = (SegmentShape2D)shape; Vector2 start = segment.A; segment.A += Vector2.One; Require(!source.Valid(), "Clearing boundary drift admitted."); segment.A = start;
        int slot = clearing.GetIndex(); Node parent = clearing.GetParent(); parent.RemoveChild(clearing);
        var replacement = new Node2D { Name = "OpeningRegionWoodlandClearingSceneSource" }; parent.AddChild(replacement);
        Require(!source.Valid(), "Replaced clearing admitted."); parent.RemoveChild(replacement); replacement.Free();
        parent.AddChild(clearing); parent.MoveChild(clearing, slot); Require(source.Valid(), "Restored clearing identity/order unavailable.");
        var nav = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        Vector2[] vertices = nav.NavigationPolygon.GetVertices(); Vector2[] changed = (Vector2[])vertices.Clone(); changed[0] += Vector2.One;
        nav.NavigationPolygon.SetVertices(changed); Require(!source.Valid(), "Navigation drift admitted."); nav.NavigationPolygon.SetVertices(vertices);
        nav.Enabled = false; Require(!source.Valid(), "Disabled map owner admitted."); nav.Enabled = true;
        Require(!source.ListenerSupported(new(2600, 1400)) && !source.ListenerSupported(new(1500, 200)), "Room listener admitted.");
        var listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener");
        RequireThrows(() => audio.DiagnosticAdvance(0.1), "Live clock injection admitted.");
        RequireThrows(() => audio.DiagnosticResources(false), "Live resource injection admitted.");
        AudioStreamPlayer2D player = audio.GetChildren().OfType<AudioStreamPlayer2D>().Single();
        player.GlobalPosition += Vector2.One;
        Require(!(bool)typeof(OpeningRegionRainAudioController).GetMethod("Bindings", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(audio, null)!, "Moved rain player admitted.");
        player.GlobalPosition = new(1000, 350);
        audio.ResetAfterLoad(); await PhysicsFrames(1201);
        Require(audio.RecentCues.Count == 1, "Live admitted caption/history missing.");
        int count = audio.RecentCues.Count;
        GetTree().Paused = true; audio._Process(0.01); Require(!Playing() && audio.RecentCues.Count == count, "Pause lost history/retained audio.");
        GetTree().Paused = false; audio._Notification((int)NotificationApplicationFocusOut); Require(!Playing(), "Focus retained audio.");
        audio._Notification((int)NotificationApplicationFocusIn);
        audio._Process(double.NaN); Require(!Playing(), "Stall retained audio.");
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); panel.Open(); audio._Process(0.01);
        Require(!Playing() && audio.RecentCues.Count == count, "Reading lost history/retained audio."); panel.Close(false);
        var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        WorldEventId unknown = new("unknown-rain-proof-event");
        SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
            initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
        Require(!restore.TryRestore(rejected) && audio.RecentCues.Count == count, "Failed load changed history.");
        Require(restore.TryRestore(initial) && audio.RecentCues.Count == 0 && !Playing(), "Load did not clear new transient owner.");
        listener.ClearCurrent(); long prior = audio.PresentationCount;
        await PhysicsFrames(1201);
        Require(audio.PresentationCount == prior, "Noncurrent listener admitted."); listener.MakeCurrent();
        audio.ResetAfterLoad(); await PhysicsFrames(2);
        checks.Add("Static clearing hide/move/replacement/area and clearing boundary drift and nav drift rejected; source/listener room support; live injection; pause/focus/stall/overlay history preservation; failed/successful load; noncurrent listener.");
    }

    private async Task Views(string directory)
    {
        Directory.CreateDirectory(directory);
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        Camera2D camera = GetNode<Camera2D>("../Seeker/FollowCamera"); camera.PositionSmoothingEnabled = false;
        camera.Zoom = new(2, 2); camera.Offset = new(0, -220); camera.ForceUpdateScroll();
        audio.ResetAfterLoad(); await PhysicsFrames(3);
        await PhysicsFrames(1201);
        await Frame();
        Require(captions.GetNode<Label>("Panel/Lines/Rain").Text.Contains("below", StringComparison.Ordinal), "Ordinary offscreen bearing missing.");
        await Capture(Path.Combine(directory, "ordinary-offscreen.png"));
        int retainedHistory = audio.RecentCues.Count;
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frames(3);
        Require(!GetWindow().HasFocus() && !Playing() && audio.RecentCues.Count == retainedHistory,
            "Native focus loss did not cancel audio/preserve history.");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus(); await Frames(3);
        audio.Automatic = false; audio.SetProcess(false); audio.SetPhysicsProcess(false);
        seeker.SetPhysicsProcess(false); GetNode<Healer>("../Healer").SetPhysicsProcess(false);
        foreach (string name in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionHandToolTapCoordinator", "OpeningRegionHealerFootstepAudioController", "OpeningRegionFoliageAudioController", "OpeningRegionWindAudioController" })
        { GetNode<Node>("../" + name).SetProcess(false); GetNode<Node>("../" + name).SetPhysicsProcess(false); }
        var leaves = GetNode<OpeningRegionFoliageAudioController>("../OpeningRegionFoliageAudioController");
        var footsteps = GetNode<OpeningRegionHealerFootstepAudioController>("../OpeningRegionHealerFootstepAudioController");
        var wind = GetNode<OpeningRegionWindAudioController>("../OpeningRegionWindAudioController");
        var windStore = (WindCueHistory)typeof(OpeningRegionWindAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(wind)!;
        windStore.Clear(); windStore.Record("Wind blowing below");
        var replies = GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController");
        var leafStore = (FoliageCueHistory)typeof(OpeningRegionFoliageAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(leaves)!;
        var footStore = (HealerFootstepCueHistory)typeof(OpeningRegionHealerFootstepAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(footsteps)!;
        var replyStore = (PositionalCueHistory)typeof(OpeningRegionPositionalAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(replies)!;
        leafStore.Clear(); leafStore.Record("Leaves rustling below"); footStore.Clear(); footStore.Record("Footsteps on earth above");
        replyStore.Clear(); replyStore.Record("Conversation right");
        string[] priorHistory = [string.Join('|', leaves.RecentCues), string.Join('|', footsteps.RecentCues), string.Join('|', replies.RecentCues), string.Join('|', wind.RecentCues)];
        var store = (RainCueHistory)typeof(OpeningRegionRainAudioController).GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
        store.Clear();
        for (int n = 0; n < 20; n++)
        { for (int t = 0; t < 200; t++) { store.Advance(0.1); } Require(store.Record("Rain falling below"), "History record lost."); }
        Require(store.Entries.Count == 16 && store.Entries[0].StartsWith("5.", StringComparison.Ordinal), "Capacity/oldest eviction failed.");
        Require(!store.Record("Rain falling right"), "Rate-limited start queued/admitted.");
        var bearings = new List<object>();
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); await Frames(3);
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation above/right"); captions.SetInterior("Outdoor sounds softened");
            captions.SetWaterContact("Water contact"); captions.SetCaskCover("Container and cloth handling"); captions.SetHandToolTap("Wooden mallet contact");
            captions.SetHealerFootstep("Footsteps in leaves above/right"); captions.SetFoliage("Leaves rustling below/right"); captions.SetWind("Wind blowing below/right"); captions.SetRain("Rain falling below/right"); await Frame();
            Require(captions.GetNode<Control>("Panel/Lines").GetChildCount() == 12, "Missing caption owner.");
            Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Caption panel clipped.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && captions.GetNode<Control>("Panel").GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            await Capture(Path.Combine(directory, $"twelve-captions-{size.X}.png"));
            captions.SetProcess(false);
            var retained = captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>().Where(l => l.Name != "Rain").Select(l => l.Text).ToArray();
            Vector2 viewport = GetViewport().GetVisibleRect().Size;
            foreach (Vector2 target in new[] { new Vector2(-1, -1), new Vector2(viewport.X / 2, -1), new Vector2(viewport.X + 1, -1),
                new Vector2(-1, viewport.Y / 2), new Vector2(viewport.X + 1, viewport.Y / 2), new Vector2(-1, viewport.Y + 1),
                new Vector2(viewport.X / 2, viewport.Y + 1), new Vector2(viewport.X + 1, viewport.Y + 1), Vector2.Zero, viewport })
            {
                audio.ResetAfterLoad(); camera.Zoom = new(2, 2);
                camera.Offset = new Vector2(1000, 350) - seeker.GlobalPosition - (target - viewport / 2) / camera.Zoom;
                camera.ForceUpdateScroll(); await Frames(2);
                for (int t = 0; t < 200; t++) { audio.DiagnosticAdvance(0.1); }
                Transform2D canvas = GetViewport().GlobalCanvasTransform * GetViewport().CanvasTransform;
                Vector2 actual = canvas * new Vector2(1000, 350);
                string expected = RainSpatialProjection.Caption(new(actual.X, actual.Y), new(viewport.X, viewport.Y));
                Require(captions.GetNode<Label>("Panel/Lines/Rain").Text == expected, "Native transformed bearing mismatch: " + captions.GetNode<Label>("Panel/Lines/Rain").Text + " vs " + expected + " viewport=" + viewport + " screen=" + actual);
                bearings.Add(new { size = new[] { size.X, size.Y }, viewport = new[] { viewport.X, viewport.Y }, target = new[] { target.X, target.Y }, screen = new[] { actual.X, actual.Y }, text = expected });
            }
            audio.ResetAfterLoad();
            Require(new[] { string.Join('|', leaves.RecentCues), string.Join('|', footsteps.RecentCues), string.Join('|', replies.RecentCues), string.Join('|', wind.RecentCues) }.SequenceEqual(priorHistory), "Rain clear changed retained histories.");
            Require(captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>().Where(l => l.Name != "Rain").Select(l => l.Text).SequenceEqual(retained), "Rain clear changed prior captions.");
            store.Clear();
            for (int n = 0; n < 20; n++)
            { for (int t = 0; t < 200; t++) { store.Advance(0.1); } Require(store.Record("Rain falling below"), "Independent history lost."); }
            // Missing R01 remains a text/history presentation in the stopped native fixture.
            audio.DiagnosticResources(false); audio.ResetAfterLoad();
            for (int t = 0; t < 200; t++) { audio.DiagnosticAdvance(0.1); }
            Require(!Playing() && audio.RecentCues.Count == 1, "Missing stream lost accessible history.");
            audio.ResetAfterLoad(); audio.DiagnosticResources(true);
            for (int n = 0; n < 20; n++)
            { for (int t = 0; t < 200; t++) { store.Advance(0.1); } Require(store.Record("Rain falling below"), "History lost after native checks."); }
            settings.SetPreferences(new AudioPreferences(settings.Preferences.Channels, settings.Preferences.Preset, false, false).WithChannel(AudioCategory.Ambience, 1, true));
            panel.Open(); ((Button)panel.FindChild("RecentSoundCues", true, false)).EmitSignal(Button.SignalName.Pressed); await Frames(3);
            var historyPanel = panel.GetNode<PositionalSoundCueHistoryPanel>("PositionalSoundCueHistoryPanel");
            RichTextLabel entries = (RichTextLabel)panel.FindChild("Entries", true, false);
            Require(entries.Text.IndexOf("Heard reply cues", StringComparison.Ordinal) < entries.Text.IndexOf("Nearby footstep cues", StringComparison.Ordinal)
                && entries.Text.IndexOf("Nearby footstep cues", StringComparison.Ordinal) < entries.Text.IndexOf("Nearby foliage cues", StringComparison.Ordinal)
                && entries.Text.IndexOf("Nearby foliage cues", StringComparison.Ordinal) < entries.Text.IndexOf("Nearby wind cues", StringComparison.Ordinal)
                && entries.Text.IndexOf("Nearby wind cues", StringComparison.Ordinal) < entries.Text.IndexOf("Nearby rain cues", StringComparison.Ordinal), "Prior history section order changed.");
            Require(entries.Text.Contains("Nearby rain cues", StringComparison.Ordinal) && entries.Text.Contains("20. Rain falling below", StringComparison.Ordinal), "Muted/captions-off history missing.");
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
        checks.Add("Actual native focus loss cancels with history retained; native twelve-caption bounds at both sizes; independent capacity/limiter; muted/captions-off selectable scrolling history; Tab/Shift-Tab/Escape/return focus.");
        File.WriteAllText(Path.Combine(directory, "native.json"), JsonSerializer.Serialize(new { status = "passed", viewports = Viewports, bearings, crossOwnerClear = true, missingAudioHistory = true, checks }, Options));
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
