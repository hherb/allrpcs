using System.Reflection;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Opt-in actual motion, parity, ownership, lifecycle and native UI fixture for the new emitter.</summary>
public partial class HealerFootstepRuntimeSmokeCheck : Node
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };
    private readonly List<object> motion = [];
    private readonly List<string> checks = [];
    private OpeningRegionHealerFootstepAudioController audio = null!;
    private Healer healer = null!;
    private Seeker seeker = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private string mode = string.Empty;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_CHECK") != "1") { return; }
        try
        {
            ProcessMode = ProcessModeEnum.Always;
            for (int i = 0; i < 10; i++) { await Frame(); }
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            healer = GetNode<Healer>("../Healer"); seeker = GetNode<Seeker>("../Seeker");
            audio = GetNode<OpeningRegionHealerFootstepAudioController>("../OpeningRegionHealerFootstepAudioController");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            mode = OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_MODE");
            GetNode<AudioSettingsPanel>("../AudioSettingsPanel").Close(false);
            GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            GetNode<CanvasLayer>("../SettingsOverlay").Visible = false;
            healer.Visible = true; // Disposable fixture freezes the prior independent-task party presentation.
            healer.CommandWait(); healer.GlobalPosition = new(1660, 900); seeker.GlobalPosition = new(1720, 900);
            await Frames(8); audio.ResetAfterLoad();
            var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
            AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            if (mode == "captions-off") { p = new(p.Channels, p.Preset, false, false); }
            settings.SetPreferences(p);
            if (mode == "missing-resources") { audio.RemoveDiagnosticResources(); }
            if (mode == "observer-disabled") { audio.Automatic = false; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frames(3); }
            GetNode<HandToolTapRuntimeSmokeCheck>("../HandToolTapRuntimeSmokeCheck").ArrangeSave();
            await Frames(8); // Allow ordinary map visitation to settle before the unchanged-state snapshot.
            var capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("No complete save.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial), truth = ProtectedTruth();
            // Saves keep character positions (schema 9); arrival tolerance and fixtures move both
            // characters, so they return to the baseline spots before each full-save comparison.
            Vector2 healerStart = healer.GlobalPosition, seekerStart = seeker.GlobalPosition;
            void ReturnToStart() { healer.GlobalPosition = healerStart; seeker.GlobalPosition = seekerStart; }
            long initialCount = audio.ContactCount;
            Action<FootstepMotionSample> recorder = sample => motion.Add(new
            {
                sample.Tick,
                start = new[] { sample.Start.X, sample.Start.Y },
                end = new[] { sample.End.X, sample.End.Y },
                sample.Delta,
                sample.SpeedLimit,
                command = healer.CommandMode.ToString()
            });
            healer.MotionCompleted += recorder;
            Require(healer.TryCommandMoveToDestination(new(1768, 900)), "Move destination rejected."); await PhysicsFrames(50);
            Require(healer.GlobalPosition.DistanceTo(new(1768, 900)) <= 12, "Actual move did not arrive.");
            long atArrival = audio.ContactCount; await PhysicsFrames(15);
            Require(atArrival == audio.ContactCount && healer.CommandMode == Allrpcs.Application.Movement.HealerCommandMode.Wait, "Arrival/wait emitted contacts.");
            Require(healer.TryCommandMoveToDestination(new(1652, 900)), "Return destination rejected."); await PhysicsFrames(50);
            seeker.GlobalPosition = new(1840, 918); audio.CancelMotion(); await PhysicsFrames(2);
            healer.CommandFollow(); await PhysicsFrames(55); healer.CommandWait(); await PhysicsFrames(2);
            seeker.GlobalPosition = new(1720, 900);
            Require(healer.TryCommandMoveToDestination(new(1652, 900)), "Final return rejected."); await PhysicsFrames(55);
            healer.MotionCompleted -= recorder;
            Require(mode == "observer-disabled" ? audio.ContactCount == initialCount : audio.ContactCount >= initialCount + 4,
                "Missing actual independent contacts: " + audio.ContactCount);
            ReturnToStart();
            if (SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) != before || ProtectedTruth() != truth)
            {
                File.WriteAllText("/private/tmp/healer-truth-before.json", truth); File.WriteAllText("/private/tmp/healer-truth-after.json", ProtectedTruth());
                File.WriteAllText("/private/tmp/healer-save-before.json", before); File.WriteAllText("/private/tmp/healer-save-after.json", SaveSnapshotJsonSerializer.Serialize(capture.Capture()!));
                throw new InvalidOperationException("Motion/audio changed protected truth.");
            }
            checks.Add("Actual Move/arrival/wait/return/Follow motion; independent contacts and complete save/transcript/protected truth parity.");
            long count = audio.ContactCount, presented = audio.PresentationCount;
            MethodInfo receive = typeof(OpeningRegionHealerFootstepAudioController).GetMethod("OnMotion", BindingFlags.NonPublic | BindingFlags.Instance)!;
            var last = healer.LastMotion!; receive.Invoke(audio, [last]); receive.Invoke(audio, [last with { Tick = last.Tick + 1 }]);
            Require(count == audio.ContactCount && presented == audio.PresentationCount, "Forged/repeated motion admitted.");
            bool injectionRejected = false;
            if (audio.Automatic)
            {
                try { audio.DiagnosticContact(new(FootstepMaterial.Earth, FootstepLeg.Left, 1, new(1700, 900)), GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener")); }
                catch (InvalidOperationException) { injectionRejected = true; }
                Require(injectionRejected, "Live diagnostic injection admitted.");
            }
            checks.Add("Exact producer reference, duplicate/forged motion and live diagnostic injection rejected.");
            if (mode == "default") { await Lifecycle(initial, truth); }
            string views = OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_VIEWS");
            if (views.Length > 0) { await Views(views); }
            ReturnToStart();
            Require(ProtectedTruth() == truth && SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Checks changed protected truth/save.");
            string output = OS.GetEnvironment("ALLRPCS_HEALER_FOOTSTEP_REPORT");
            if (output.Length > 0)
            {
                File.WriteAllText(output + ".save.json", before);
                File.WriteAllText(output, JsonSerializer.Serialize(new
                {
                    mode,
                    checks,
                    motion,
                    contacts = count - initialCount,
                    presentations = presented,
                    truthBefore = truth,
                    truthAfter = ProtectedTruth(),
                    transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
                }, Options));
            }
            GD.Print("PASS: Healer footstep Main: actual motion, exact owner, complete parity and lifecycle."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task Lifecycle(SaveSnapshot initial, string truth)
    {
        var material = GetNode<OpeningRegionHealerFootstepMaterialSource>("../OpeningRegionHealerFootstepMaterialSource");
        Require(material.Select(new(1700, 900)) == FootstepMaterial.Earth && material.Select(new(2600, 1400)) is null
            && material.Select(new(1500, 200)) is null, "Exterior source admitted interior/default.");
        var nav = GetNode<GreyboxNavigation>("../GreyboxNavigation");
        Vector2[] vertices = nav.NavigationPolygon.GetVertices(); Vector2[] changed = (Vector2[])vertices.Clone(); changed[0] += Vector2.One;
        nav.NavigationPolygon.SetVertices(changed); Require(!material.Valid(), "Changed navigation accepted."); nav.NavigationPolygon.SetVertices(vertices);
        nav.Enabled = false; Require(material.Select(new(1700, 900)) is null, "Disabled surface accepted."); nav.Enabled = true;
        Rid map = NavigationServer2D.RegionGetMap(nav.GetRid()); NavigationServer2D.MapSetActive(map, false); await Frames(3);
        Require(material.Select(new(1700, 900)) is null, "Inactive map accepted."); NavigationServer2D.MapSetActive(map, true); await Frames(3);
        Require(material.Valid(), "Original surface did not reactivate.");
        healer.GlobalPosition = new(1660, 900); seeker.GlobalPosition = new(1720, 900); audio.ResetAfterLoad(); await PhysicsFrames(2);
        Require(healer.TryCommandMoveToDestination(new(1768, 900)), "Lifecycle movement rejected."); await PhysicsFrames(31);
        Require(captions.GetNode<Label>("Panel/Lines/HealerFootstep").Text.Length > 0, "Live contact missing caption.");
        int entries = audio.RecentCues.Count;
        GetTree().Paused = true; audio._Process(0.01); Require(!Playing() && audio.RecentCues.Count == entries, "Pause retained tails/lost history.");
        GetTree().Paused = false; audio._Notification((int)NotificationApplicationFocusOut); Require(!Playing(), "Focus loss retained tails.");
        audio._Notification((int)NotificationApplicationFocusIn);
        audio._Process(double.NaN); Require(!Playing(), "Invalid delta retained tails.");
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); panel.Open(); audio._Process(0.01); Require(!Playing(), "Reading retained tails."); panel.Close(false);
        healer.CommandWait(); await PhysicsFrames(2);
        var restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        WorldEventId unknown = new("unknown-healer-footstep-probe");
        SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
            initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
        Require(!restore.TryRestore(rejected), "Invalid restore accepted."); Require(audio.RecentCues.Count == entries, "Failed load changed history.");
        Require(restore.TryRestore(initial) && audio.RecentCues.Count == 0 && !Playing(), "Successful load replayed history/tails.");
        // Probe the retained sickhouse wall with actual collision resolution, without changing its geometry.
        healer.CommandWait(); healer.SetPhysicsProcess(false); healer.GlobalPosition = new(2464, 1400);
        var complete = typeof(Healer).GetMethod("CompleteMovement", BindingFlags.NonPublic | BindingFlags.Instance)!;
        FootstepContactCadence blockedCadence = new(); bool observerFailed = false;
        Action<FootstepMotionSample>? failedObserver = null;
        failedObserver = _ => { healer.MotionCompleted -= failedObserver; observerFailed = true; throw new InvalidOperationException("Intentional observer isolation probe"); };
        healer.MotionCompleted += failedObserver;
        for (int t = 0; t < 40; t++)
        {
            healer.Velocity = new(180, 0); complete.Invoke(healer, [1.0 / 60]);
            Require(blockedCadence.Advance(healer.LastMotion!, _ => FootstepMaterial.Earth).Contact is null, "Wall pressure emitted a logical contact.");
            await PhysicsFrames(1);
        }
        Require(observerFailed && healer.GlobalPosition.X < 2470 && healer.LastMotion!.Start == healer.LastMotion.End,
            "Actual retained-wall pressure or observer isolation not exercised.");
        healer.GlobalPosition = new(1768, 900); healer.SetPhysicsProcess(true); audio.ResetAfterLoad();
        checks.Add("Actual collision against unchanged sickhouse wall emits no contact; one failing motion observer is isolated.");
        var listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener"); listener.ClearCurrent();
        Require(healer.TryCommandMoveToDestination(new(1652, 900)), "Listener fixture rejected move.");
        long prior = audio.PresentationCount; await PhysicsFrames(50); Require(audio.PresentationCount == prior, "Noncurrent listener admitted."); listener.MakeCurrent();
        healer.Visible = false; await PhysicsFrames(2); Require(!Playing(), "Hidden source retained tails."); healer.Visible = true;
        Require(ProtectedTruth() == truth, "Lifecycle changed protected state.");
        checks.Add("Room exclusions, mutated/disabled/inactive navigation, pause/focus/invalid delta/overlay, failed/successful load, noncurrent listener and hidden actor cancellation.");
    }
    private async Task Views(string directory)
    {
        Directory.CreateDirectory(directory); healer.CommandWait();
        var settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
        var panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        Camera2D camera = GetNode<Camera2D>("../Seeker/FollowCamera"); camera.PositionSmoothingEnabled = false;
        GetWindow().Size = new(1280, 720); GetWindow().GrabFocus(); await Frames(3);
        seeker.GlobalPosition = new(1720, 900); healer.GlobalPosition = new(1660, 900);
        camera.Zoom = new(2, 2); camera.Offset = new(0, -200); audio.ResetAfterLoad(); await PhysicsFrames(3); camera.ForceUpdateScroll();
        bool Guard(string name) => (bool)typeof(OpeningRegionHealerFootstepAudioController).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(audio, null)!;
        var viewMaterial = GetNode<OpeningRegionHealerFootstepMaterialSource>("../OpeningRegionHealerFootstepMaterialSource");
        GD.Print($"VIEW active={Guard("Active")} bindings={Guard("Bindings")} listener={Guard("ListenerValid")} source={viewMaterial.Select(new(1660, 900))} listenerMaterial={viewMaterial.Select(new(1720, 900))}");
        long priorPresentation = audio.PresentationCount;
        Require(healer.TryCommandMoveToDestination(new(1768, 900)), "Offscreen motion rejected.");
        for (int t = 0; t < 50 && audio.PresentationCount == priorPresentation; t++)
        {
            await PhysicsFrames(1);
            if (t % 10 == 0) { GD.Print($"VIEW tick={t} active={Guard("Active")} bindings={Guard("Bindings")} listener={Guard("ListenerValid")} source={viewMaterial.Select(new(healer.GlobalPosition.X, healer.GlobalPosition.Y))} contacts={audio.ContactCount} motion={healer.LastMotion}"); }
        }
        camera.ForceUpdateScroll(); await Frame();
        Require(captions.GetNode<Label>("Panel/Lines/HealerFootstep").Text == "Footsteps on earth below", "Actual offscreen bearing missing: " + captions.GetNode<Label>("Panel/Lines/HealerFootstep").Text + " viewport=" + GetViewport().GetVisibleRect() + " body=" + healer.GlobalPosition + " contact=" + audio.LastContact + " presentations=" + (audio.PresentationCount - priorPresentation));
        await Capture(Path.Combine(directory, "offscreen-1280.png"));
        healer.CommandWait(); healer.SetPhysicsProcess(false); seeker.SetPhysicsProcess(false);
        IReadOnlyList<string> retainedReplies = GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController").RecentCues.ToArray();
        // Disposable view-only fixture freezes prior writers to inspect simultaneous independently owned lines.
        foreach (string name in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionHandToolTapCoordinator" }) { GetNode<Node>("../" + name).SetProcess(false); }
        audio.SetProcess(false); audio.SetPhysicsProcess(false);
        var historyStore = (HealerFootstepCueHistory)typeof(OpeningRegionHealerFootstepAudioController)
            .GetField("history", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(audio)!;
        int first = historyStore.Entries.Count;
        Require(!historyStore.Record("Footsteps on earth below") && historyStore.Entries.Count == first, "History rate limiter failed.");
        for (int n = 0; n < 16; n++)
        {
            for (int t = 0; t < 10; t++) { historyStore.Advance(0.1); }
            Require(historyStore.Record("Footsteps on earth below"), "Eligible history lost.");
        }
        Require(historyStore.Entries.Count == 16 && historyStore.Entries[0].StartsWith("2.", StringComparison.Ordinal), "History capacity/eviction failed.");
        Require(retainedReplies.SequenceEqual(GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController").RecentCues), "New history changed reply entries.");
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); await Frames(3);
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation above/right"); captions.SetInterior("Outdoor sounds softened");
            captions.SetWaterContact("Water contact"); captions.SetCaskCover("Container and cloth handling"); captions.SetHandToolTap("Wooden mallet contact");
            captions.SetHealerFootstep("Footsteps in leaves above/right"); await Frame();
            Control box = captions.GetNode<Control>("Panel"); Require(GetViewport().GetVisibleRect().Encloses(box.GetGlobalRect()), "Nine captions outside viewport.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && box.GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            await Capture(Path.Combine(directory, "captions-" + size.X + ".png"));
            settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, false, false)); Require(!captions.Visible, "Caption toggle failed.");
            panel.Open(); await Frame(); Button entry = (Button)panel.FindChild("RecentSoundCues", true, false); entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            RichTextLabel history = (RichTextLabel)panel.FindChild("Entries", true, false);
            Button back = (Button)panel.FindChild("CloseHistory", true, false);
            Require(history.Text.Contains("Nearby footstep cues", StringComparison.Ordinal) && history.Text.Contains("17. Footsteps on earth below", StringComparison.Ordinal), "Separate populated history section missing with captions off.");
            Require(back.HasFocus(), "History initial focus missing.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false });
            Require(history.HasFocus(), "History Tab focus failed.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = true }); await Frames(2);
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, ShiftPressed = true, Pressed = false });
            Require(back.HasFocus(), "History Shift-Tab focus failed.");
            await Capture(Path.Combine(directory, "history-" + size.X + ".png"));
            panel._Input(new InputEventKey { Keycode = Key.Escape, Pressed = true }); await Frame();
            if (!entry.HasFocus()) { panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true }); await Frame(); }
            Require(entry.HasFocus(), "History return focus failed."); panel.Close(false); settings.RestoreDefaults();
            panel.Open(); entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame(); back.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(entry.HasFocus(), "History Back return focus failed."); panel.Close(false);
        }
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frames(3); Require(!GetWindow().HasFocus(), "Native focus loss not exercised.");
        audio._Process(0.01); Require(!Playing(), "Native focus loss retained tails."); GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus();
        checks.Add("Native actual offscreen contact; 1280/1920 nine-caption coexistence; separate bounded/rate-limited history with captions off; Tab/Shift-Tab/Escape/Back focus and actual native focus loss.");
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
    private async Task PhysicsFrames(int count) { for (int i = 0; i < count; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); } }
    private async Task Capture(string path) { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); Require(GetViewport().GetTexture().GetImage().SavePng(path) == Error.Ok, "Capture failed."); }
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
