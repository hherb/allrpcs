using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Actions;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Finite live input, reach, lifecycle, UI and complete-save checks for the new action.</summary>
public partial class HandToolTapRuntimeSmokeCheck : Node
{
    private OpeningRegionHandToolTapCoordinator source = null!;
    private OpeningRegionHandToolTapAudioController audio = null!;
    private HandToolTapPanel panel = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private Seeker seeker = null!;
    private readonly List<string> checks = [];
    private readonly List<object> truthChecks = [];
    private string mode = string.Empty;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_TOOL_MAIN_CHECK") != "1")
        {
            if (HasNode("OpeningRegionHandToolTapAudioController"))
            {
                Label label = new() { Text = "Wooden mallet contact proof · stopped", Position = new(16, 16) }; AddChild(label);
                Button play = new() { Text = "Play accepted mallet contact (diagnostic)", Position = new(16, 56) }; AddChild(play);
                Button stop = new() { Text = "Stop", Position = new(16, 100) }; AddChild(stop);
                var local = GetNode<OpeningRegionHandToolTapAudioController>("OpeningRegionHandToolTapAudioController");
                play.Pressed += local.DiagnosticPlay; stop.Pressed += local.Cancel;
                for (int tick = 0; tick < 10; tick++) { await Frame(); }
                Require(!local.Automatic && local.PresentationCount == 0
                    && !local.GetNode<AudioStreamPlayer>("HandToolTap").Playing, "Standalone diagnostic started live feedback.");
                GD.Print("PASS: hand-tool standalone starts stopped; no ordinary commit owner or automatic playback.");
            }
            return;
        }
        try
        {
            for (int i = 0; i < 8; i++) { await Frame(); }
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            source = GetNode<OpeningRegionHandToolTapCoordinator>("../OpeningRegionHandToolTapCoordinator");
            audio = GetNode<OpeningRegionHandToolTapAudioController>("../OpeningRegionHandToolTapAudioController");
            panel = GetNode<HandToolTapPanel>("../HandToolTapPanel");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel"); seeker = GetNode<Seeker>("../Seeker");
            mode = OS.GetEnvironment("ALLRPCS_TOOL_MODE");
            AudioSettingsController settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            settings.RestoreDefaults(); AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange, false, true); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            settings.SetPreferences(p);
            if (mode == "missing-stream") { audio.GetNode<AudioStreamPlayer>("HandToolTap").Stream = null; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frame(); }
            await Place(new(1768, 900));
            Require(source.CommitCount == 0 && !source.TryTap(), "Startup or out-of-reach crossing acquired a commit.");
            await Move("move_left", 9); await Frame();
            Require(seeker.GlobalPosition.DistanceTo(new(1732, 900)) <= 5 && source.CanOffer,
                "Ordinary motion could not reach approved example: " + seeker.GlobalPosition);
            Require(source.CommitCount == 0, "Walking/eligibility emitted tool tap.");
            Vector2 reached = seeker.GlobalPosition;
            await Move("move_right", 9);
            Require(seeker.GlobalPosition.DistanceTo(new(1768, 900)) <= 5, "Ordinary route return blocked.");
            await Place(reached);
            checks.Add("ordinary collision-resolved movement to supported Village Edge example and route return; no walking/standing/startup contact");
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Missing complete save root.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial);
            if (mode == "no-action") { Finish(before); return; }
            if (mode == "views") { await Views(settings); Finish(before); return; }
            Button button = (Button)panel.FindChild("TapPracticeBlock", true, false);
            string ordinaryTruth = ProtectedTruth();
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Require(source.CommitCount == 1 && audio.PresentationCount == 1, "Explicit button did not commit/present exactly once.");
            Require(captions.GetNode<Label>("Panel/Lines/HandToolTap").Text == "Wooden mallet contact", "Missing independent caption.");
            Require(panel.FindChild("Status", true, false) is Label status && status.Text == HandToolTapPanel.StatusText, "Missing own-action status.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Contact mutated durable state.");
            RecordTruth(ordinaryTruth, "ordinary-tap");
            HandToolTapCommit commit = source.LastCommit!;
            Require(!source.TryTap(), "Cooldown admitted duplicate activation."); audio.Receive(commit);
            audio.Receive(new(commit.Request, commit.Sequence));
            Require(audio.PresentationCount == 1, "Repeated/forged notification reached audio.");
            checks.Add("one explicit native button/one exact commit/one caption; copied/repeated commits rejected; complete durable state unchanged");
            double remaining = source.Cooldown;
            GetTree().Paused = true; source._Process(0.1);
            Require(source.Cooldown == remaining && !source.TryTap() && !audio.GetNode<AudioStreamPlayer>("HandToolTap").Playing, "Pause did not freeze/cancel.");
            GetTree().Paused = false;
            source._Process(double.NaN); Require(source.Cooldown == remaining, "Invalid time replenished cooldown.");
            source._Process(0.251); Require(source.Cooldown == remaining, "Long frame replenished cooldown.");
            source._Notification((int)NotificationApplicationFocusOut); source._Process(0.1);
            Require(!source.TryTap() && source.Cooldown == remaining, "Focus loss admitted/freed contact.");
            source._Notification((int)NotificationApplicationFocusIn);
            seeker.SetPhysicsProcess(false); Require(!source.TryTap(), "Disabled Seeker admitted contact."); seeker.SetPhysicsProcess(true);
            source.ResetPresentation();
            AudioSettingsPanel settingsPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); settingsPanel.Open();
            Require(!source.TryTap(), "Reading overlay admitted contact."); settingsPanel.Close(false);
            LineEdit input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input"); GetNode<CanvasLayer>("../DialogueOverlay").Visible = true;
            input.GrabFocus(); Require(!source.TryTap(), "Text input admitted contact."); input.ReleaseFocus(); GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            Marker2D target = GetNode<Marker2D>("../OpeningRegionProjectionSource/village-edge");
            target.Position += Vector2.One; Require(!source.TryTap(), "Moved fixed site admitted tap."); target.Position -= Vector2.One;
            target.Visible = false; Require(!source.TryTap(), "Hidden fixed site admitted tap."); target.Visible = true;
            target.ProcessMode = ProcessModeEnum.Disabled; Require(!source.TryTap(), "Disabled site admitted tap."); target.ProcessMode = ProcessModeEnum.Inherit;
            Node parent = target.GetParent(); parent.RemoveChild(target);
            Marker2D replacement = new() { Name = "village-edge", Position = target.Position }; parent.AddChild(replacement);
            Require(!source.TryTap(), "Value-equal replaced marker acquired site permission.");
            parent.RemoveChild(replacement); replacement.QueueFree(); parent.AddChild(target);
            GreyboxNavigation navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
            Vector2[] vertices = navigation.NavigationPolygon.GetVertices(); Vector2[] changedVertices = (Vector2[])vertices.Clone();
            changedVertices[0] += Vector2.One; navigation.NavigationPolygon.SetVertices(changedVertices);
            Require(!source.TryTap(), "Mutated navigation contents admitted contact."); navigation.NavigationPolygon.SetVertices(vertices);
            navigation.Enabled = false; Require(!source.TryTap(), "Disabled navigation admitted contact."); navigation.Enabled = true;
            Rid mapRid = NavigationServer2D.RegionGetMap(navigation.GetRid());
            NavigationServer2D.MapSetActive(mapRid, false); await Frame(); await Frame(); await Frame();
            Require(!source.TryTap(), "Inactive navigation map admitted contact.");
            NavigationServer2D.MapSetActive(mapRid, true); await Frame(); await Frame(); await Frame();
            Require(source.TryTap(), "Contact failed after valid owners returned.");
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            WorldEventId unknown = new("unknown-tool-probe");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            HandToolTapCommit last = source.LastCommit!; double cooldown = source.Cooldown;
            Require(!restore.TryRestore(rejected) && ReferenceEquals(last, source.LastCommit) && source.Cooldown == cooldown, "Failed restore changed contact state.");
            Require(restore.TryRestore(initial) && source.LastCommit is null && source.Cooldown == 0, "Successful restore retained action token.");
            Require(!audio.GetNode<AudioStreamPlayer>("HandToolTap").Playing && captions.GetNode<Label>("Panel/Lines/HandToolTap").Text == string.Empty, "Restore replayed/retained feedback.");
            checks.Add("pause/focus/invalid time/disabled input, overlays/text focus, changed target visibility/rotation/substitution/navigation and failed/successful full restore");
            await Place(reached); Require(source.TryTap(), "Missing-bus fixture failed to commit.");
            source.ResetPresentation(); int effects = AudioServer.GetBusIndex("Effects"); AudioServer.SetBusName(effects, "MissingEffects");
            Require(source.TryTap() && !audio.GetNode<AudioStreamPlayer>("HandToolTap").Playing, "Missing Effects fell through to Master.");
            Require(captions.GetNode<Label>("Panel/Lines/HandToolTap").Text == "Wooden mallet contact", "Missing bus removed caption.");
            AudioServer.SetBusName(effects, "Effects"); source.ResetPresentation();
            Require(source.TryTap(), "Lifecycle final contact failed."); seeker.GlobalPosition = new(1768, 900); source._Process(0);
            Require(!audio.GetNode<AudioStreamPlayer>("HandToolTap").Playing, "Departure did not stop contact.");
            // Saves keep character positions (schema 9), so return before comparing the full save.
            seeker.GlobalPosition = reached;
            checks.Add("missing Effects stream/bus retain commits/captions without fallback; departure cancels owned audio only");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Lifecycle/action checks changed full save.");
            source.ResetPresentation();
            await Place(reached);
            var contaminationSource = GetNode<TemporaryCharacterObjectSabotageContaminationSource>("../TemporaryCharacterObjectSabotageContaminationSource");
            var contaminationState = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController");
            Require(contaminationState.TryApply(contaminationSource.Character, contaminationSource.PhysicalAccess,
                contaminationSource.WorldObjects, contaminationState.Contamination!, contaminationState.Evidence!,
                contaminationSource.Candidate), "Hidden contamination fixture failed.");
            string contaminatedTruth = ProtectedTruth(); Require(source.TryTap(), "Hidden unrelated truth changed tap availability.");
            RecordTruth(contaminatedTruth, "hidden-contamination"); source.ResetPresentation();
            checks.Add("Exact fixed-site source replacement/hiding/movement rejected; serialized inventory/world/discovery/sabotage remains unchanged around ordinary and hidden-contamination taps.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Hand-tool checks changed durable bytes.");
            Finish(before);
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }
    private void RecordTruth(string before, string name)
    {
        string after = ProtectedTruth(); Require(after == before, "Handling mutated protected truth: " + name);
        truthChecks.Add(new { name, before, after });
    }
    private string ProtectedTruth() => JsonSerializer.Serialize(new
    {
        objects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects,
        inventory = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Inventory,
        perception = GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController").Perception,
        placements = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController").Placements,
        moveEvidence = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController").Evidence,
        concealment = GetNode<CharacterObjectSabotageConcealmentController>("../CharacterObjectSabotageConcealmentController").Concealment,
        substitutions = GetNode<CharacterObjectSabotageSubstitutionController>("../CharacterObjectSabotageSubstitutionController").Substitutions,
        contamination = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController").Contamination,
        contaminationEvidence = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController").Evidence
    }, Options);
    internal void ArrangeSave()
    {
        JournalController journal = GetNode<JournalController>("../JournalController");
        if (journal.Journal is null) { Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(), "Journal fixture failed."); }
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        if (map.Map is null) { MapSaveStateRestorationProjection.Restore(new(GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id, [], [], [])).InstallInto(map); }
        var routes = GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
        if (routes.Progress is null) { Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")), "Route fixture failed."); }
    }
    private void Finish(string save)
    {
        string output = OS.GetEnvironment("ALLRPCS_TOOL_CHECK_OUTPUT");
        if (output.Length > 0)
        {
            File.WriteAllText(output + ".save.json", save); File.WriteAllText(output, JsonSerializer.Serialize(new
            {
                mode,
                checks,
                truthChecks,
                commits = source.CommitCount,
                presentations = audio.PresentationCount,
                transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
            }, Options));
        }
        GD.Print("PASS: hand-tool Main: actual reach/input, exact commits, full-save parity and lifecycle."); GetTree().Quit();
    }
    private async Task Place(Vector2 position)
    { seeker.GlobalPosition = position; for (int i = 0; i < 8; i++) { await Frame(); } }
    private async Task Move(string action, int count)
    { Input.ActionPress(action); for (int i = 0; i <= count; i++) { await Physics(); } Input.ActionRelease(action); await Frame(); }
    private async Task Frame() => await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private async Task Physics() => await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private async Task Capture(string name)
    { await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw); Require(GetViewport().GetTexture().GetImage().SavePng(name) == Error.Ok, "Capture failed."); }

    private async Task Views(AudioSettingsController settings)
    {
        string directory = OS.GetEnvironment("ALLRPCS_TOOL_VIEWS"); Directory.CreateDirectory(directory);
        // Isolate prior caption writers in this disposable layout fixture; the live hand-tool owner stays active.
        Node priorFootsteps = GetNode<Node>("../OpeningRegionFootstepAudioController");
        priorFootsteps.GetParent().RemoveChild(priorFootsteps); priorFootsteps.QueueFree();
        foreach (string owner in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionWaterContactAudioController", "OpeningRegionCaskCoverHandlingCoordinator", "OpeningRegionCaskCoverAudioController" })
        { GetNode<Node>("../" + owner).SetProcess(false); }
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); source.ResetPresentation(); await Frame(); await Frame();
            Button button = (Button)panel.FindChild("TapPracticeBlock", true, false);
            GetViewport().GuiGetFocusOwner()?.ReleaseFocus();
            for (int tab = 0; tab < 64 && !button.HasFocus(); tab++)
            {
                Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = true }); await Frame();
                Input.ParseInputEvent(new InputEventKey { Keycode = Key.Tab, Pressed = false }); await Frame();
            }
            Require(button.HasFocus(), "Native Tab could not reach contextual button.");
            long prior = source.CommitCount;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true }); await Frame();
            long count = source.CommitCount; Require(count == prior + 1, "Native keyboard activation failed.");
            for (int tick = 0; tick < 16; tick++) { source._Process(0.25); }
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = true, Echo = true }); await Frame();
            Require(source.CommitCount == count, "Held-key echo produced a contact after cooldown.");
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Enter, Pressed = false }); await Frame();
            source.ResetPresentation(); await Frame();
            prior = source.CommitCount;
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, Pressed = true }); await Frame();
            Input.ParseInputEvent(new InputEventKey { Keycode = Key.Space, Pressed = false }); await Frame();
            Require(source.CommitCount == prior + 1, "Native Space activation failed.");
            source.ResetPresentation(); await Frame(); await Frame(); prior = source.CommitCount;
            Require(!button.Disabled && source.CanOffer, "Native mouse button not ready.");
            Vector2 buttonCenter = GetViewport().GetFinalTransform() * button.GetGlobalRect().GetCenter();
            Input.ParseInputEvent(new InputEventMouseMotion { Position = buttonCenter, GlobalPosition = buttonCenter }); await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonCenter, GlobalPosition = buttonCenter, Pressed = true }); await Frame();
            Input.ParseInputEvent(new InputEventMouseButton { ButtonIndex = MouseButton.Left, Position = buttonCenter, GlobalPosition = buttonCenter, Pressed = false }); await Frame();
            Require(source.CommitCount == prior + 1, "Native mouse activation failed: " + size + ", " + buttonCenter + ", count=" + source.CommitCount + ", disabled=" + button.Disabled + ", offer=" + source.CanOffer);
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation · off-screen right"); captions.SetInterior("Outdoor sounds softened"); captions.SetWaterContact("Water contact"); captions.SetCaskCover("Container and cloth handling");
            await Frame(); await Frame(); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Control box = captions.GetNode<Control>("Panel");
            Require(GetViewport().GetVisibleRect().Encloses(box.GetGlobalRect()), "Eight-line panel outside viewport.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && box.GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            Require(GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Panel").GetGlobalRect()), "Button/status outside viewport.");
            await Capture(Path.Combine(directory, "contact-" + size.X + ".png"));
            settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, false, false));
            Require(!captions.Visible && ((Label)panel.FindChild("Status", true, false)).Text == HandToolTapPanel.StatusText, "Captions-off removed status.");
            await Capture(Path.Combine(directory, "captions-off-" + size.X + ".png")); settings.RestoreDefaults();
            AudioSettingsPanel preferences = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); preferences.Open(button); await Frame();
            Require(!source.TryTap(), "Audio reading admitted contact."); Button credit = (Button)preferences.FindChild("SourceCredits", true, false);
            credit.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(((RichTextLabel)preferences.FindChild("Credits", true, false)).Text.Contains("Hand tool tap A03-T01", StringComparison.Ordinal), "New notice missing.");
            RichTextLabel creditText = (RichTextLabel)preferences.FindChild("Credits", true, false);
            creditText.GetVScrollBar().Value = creditText.GetVScrollBar().MaxValue; await Frame();
            await Capture(Path.Combine(directory, "credits-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(credit.HasFocus(), "Credits keyboard return failed.");
            Button history = (Button)preferences.FindChild("RecentSoundCues", true, false); history.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            await Capture(Path.Combine(directory, "history-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(history.HasFocus(), "History keyboard return failed."); preferences.Close(false);
        }
        source.ResetPresentation(); await Frame(); Require(source.TryTap(), "Native focus fixture failed.");
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frame(); await Frame();
        Require(!GetWindow().HasFocus(), "Native window did not lose focus."); source._Process(0.1);
        Require(!audio.GetNode<AudioStreamPlayer>("HandToolTap").Playing, "Native focus loss did not stop contact.");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus();
        checks.Add("1280x720 and 1920x1080: eight captions/status, native mouse/Tab/Enter/Space activation/echo guard, captions-off, credits/history return; actual native focus loss");
    }
}
