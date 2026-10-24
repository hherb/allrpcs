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
public partial class CaskCoverHandlingRuntimeSmokeCheck : Node
{
    private OpeningRegionCaskCoverHandlingCoordinator source = null!;
    private OpeningRegionCaskCoverAudioController audio = null!;
    private CaskCoverHandlingPanel panel = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private Seeker seeker = null!;
    private readonly List<string> checks = [];
    private readonly List<object> truthChecks = [];
    private string mode = string.Empty;
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_CASK_MAIN_CHECK") != "1")
        {
            if (HasNode("OpeningRegionCaskCoverAudioController"))
            {
                Label label = new() { Text = "Container and cloth handling proof · stopped", Position = new(16, 16) }; AddChild(label);
                Button play = new() { Text = "Play accepted cover passage (diagnostic)", Position = new(16, 56) }; AddChild(play);
                Button stop = new() { Text = "Stop", Position = new(16, 100) }; AddChild(stop);
                var local = GetNode<OpeningRegionCaskCoverAudioController>("OpeningRegionCaskCoverAudioController");
                play.Pressed += local.DiagnosticPlay; stop.Pressed += local.Cancel;
                for (int tick = 0; tick < 10; tick++) { await Frame(); }
                Require(!local.Automatic && local.PresentationCount == 0
                    && !local.GetNode<AudioStreamPlayer>("CaskCover").Playing, "Standalone diagnostic started live feedback.");
                GD.Print("PASS: cask standalone starts stopped; no ordinary commit owner or automatic playback.");
            }
            return;
        }
        try
        {
            // Freeze the Healer before it can follow: asynchronous navigation sync makes how far it
            // walks in these frames vary between runs, and its position is part of the compared save.
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            for (int i = 0; i < 8; i++) { await Frame(); }
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            source = GetNode<OpeningRegionCaskCoverHandlingCoordinator>("../OpeningRegionCaskCoverHandlingCoordinator");
            audio = GetNode<OpeningRegionCaskCoverAudioController>("../OpeningRegionCaskCoverAudioController");
            panel = GetNode<CaskCoverHandlingPanel>("../CaskCoverHandlingPanel");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel"); seeker = GetNode<Seeker>("../Seeker");
            mode = OS.GetEnvironment("ALLRPCS_CASK_MODE");
            AudioSettingsController settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            settings.RestoreDefaults(); AudioPreferences p = settings.Preferences;
            if (mode == "quiet") { p = new(p.Channels, AudioMixPreset.ReducedRange, false, true); }
            if (mode == "master-muted") { p = p.WithChannel(AudioCategory.Master, 1, true); }
            if (mode == "effects-muted") { p = p.WithChannel(AudioCategory.Effects, 1, true); }
            settings.SetPreferences(p);
            if (mode == "missing-stream") { audio.GetNode<AudioStreamPlayer>("CaskCover").Stream = null; }
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frame(); }
            await Place(new(2132, 900));
            Require(source.CommitCount == 0 && !source.TryHandle(), "Startup or out-of-reach crossing acquired a commit.");
            await Move("move_right", 9); await Frame();
            Require(seeker.GlobalPosition.DistanceTo(new(2168, 900)) <= 5 && source.CanOffer,
                "Ordinary motion could not reach approved example: " + seeker.GlobalPosition);
            Require(source.CommitCount == 0, "Walking/eligibility emitted cask handling.");
            Vector2 reached = seeker.GlobalPosition;
            await Move("move_left", 9);
            Require(seeker.GlobalPosition.DistanceTo(new(2132, 900)) <= 5, "Ordinary route return blocked.");
            await Place(reached);
            checks.Add("ordinary collision-resolved movement to supported cask-side example and route return; no walking/standing/startup contact");
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Missing complete save root.");
            string before = SaveSnapshotJsonSerializer.Serialize(initial);
            if (mode == "no-action") { Finish(before); return; }
            if (mode == "views") { await Views(settings); Finish(before); return; }
            Button button = (Button)panel.FindChild("AdjustCaskCover", true, false);
            button.EmitSignal(BaseButton.SignalName.Pressed);
            Require(source.CommitCount == 1 && audio.PresentationCount == 1, "Explicit button did not commit/present exactly once.");
            Require(captions.GetNode<Label>("Panel/Lines/CaskCover").Text == "Container and cloth handling", "Missing independent caption.");
            Require(panel.FindChild("Status", true, false) is Label status && status.Text == CaskCoverHandlingPanel.StatusText, "Missing own-action status.");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Contact mutated durable state.");
            CaskCoverHandlingCommit commit = source.LastCommit!;
            Require(!source.TryHandle(), "Cooldown admitted duplicate activation."); audio.Receive(commit);
            audio.Receive(new(commit.Request, commit.Sequence));
            Require(audio.PresentationCount == 1, "Repeated/forged notification reached audio.");
            checks.Add("one explicit native button/one exact commit/one caption; copied/repeated commits rejected; complete durable state unchanged");
            double remaining = source.Cooldown;
            GetTree().Paused = true; source._Process(0.1);
            Require(source.Cooldown == remaining && !source.TryHandle() && !audio.GetNode<AudioStreamPlayer>("CaskCover").Playing, "Pause did not freeze/cancel.");
            GetTree().Paused = false;
            source._Process(double.NaN); Require(source.Cooldown == remaining, "Invalid time replenished cooldown.");
            source._Process(0.251); Require(source.Cooldown == remaining, "Long frame replenished cooldown.");
            source._Notification((int)NotificationApplicationFocusOut); source._Process(0.1);
            Require(!source.TryHandle() && source.Cooldown == remaining, "Focus loss admitted/freed contact.");
            source._Notification((int)NotificationApplicationFocusIn);
            seeker.SetPhysicsProcess(false); Require(!source.TryHandle(), "Disabled Seeker admitted contact."); seeker.SetPhysicsProcess(true);
            source.ResetPresentation();
            AudioSettingsPanel settingsPanel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); settingsPanel.Open();
            Require(!source.TryHandle(), "Reading overlay admitted contact."); settingsPanel.Close(false);
            LineEdit input = GetNode<LineEdit>("../DialogueOverlay/Panel/Input"); GetNode<CanvasLayer>("../DialogueOverlay").Visible = true;
            input.GrabFocus(); Require(!source.TryHandle(), "Text input admitted contact."); input.ReleaseFocus(); GetNode<CanvasLayer>("../DialogueOverlay").Visible = false;
            GreyboxInteractionTarget target = GetNode<GreyboxInteractionTarget>("../InteractionCandidates/WaterCask");
            target.Rotation += 0.1f; Require(!source.TryHandle(), "Changed target rotation admitted handling."); target.Rotation -= 0.1f;
            target.IsInteractionEnabled = false; Require(!source.TryHandle(), "Disabled marker admitted handling."); target.IsInteractionEnabled = true;
            target.Visible = false; Require(!source.TryHandle(), "Hidden marker admitted handling."); target.Visible = true;
            target.GetNode<Polygon2D>("Body").Visible = false;
            Require(!source.TryHandle(), "Hidden original body admitted handling."); target.GetNode<Polygon2D>("Body").Visible = true;
            target.GetNode<Polygon2D>("SubstituteBody").Visible = true;
            Require(!source.TryHandle(), "Substituted cask inherited material permission."); target.GetNode<Polygon2D>("SubstituteBody").Visible = false;
            GreyboxNavigation navigation = GetNode<GreyboxNavigation>("../GreyboxNavigation");
            Vector2[] vertices = navigation.NavigationPolygon.GetVertices(); Vector2[] changedVertices = (Vector2[])vertices.Clone();
            changedVertices[0] += Vector2.One; navigation.NavigationPolygon.SetVertices(changedVertices);
            Require(!source.TryHandle(), "Mutated navigation contents admitted contact."); navigation.NavigationPolygon.SetVertices(vertices);
            navigation.Enabled = false; Require(!source.TryHandle(), "Disabled navigation admitted contact."); navigation.Enabled = true;
            Rid mapRid = NavigationServer2D.RegionGetMap(navigation.GetRid());
            NavigationServer2D.MapSetActive(mapRid, false); await Frame(); await Frame(); await Frame();
            Require(!source.TryHandle(), "Inactive navigation map admitted contact.");
            NavigationServer2D.MapSetActive(mapRid, true); await Frame(); await Frame(); await Frame();
            Require(source.TryHandle(), "Contact failed after valid owners returned.");
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            WorldEventId unknown = new("unknown-cask-probe");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            CaskCoverHandlingCommit last = source.LastCommit!; double cooldown = source.Cooldown;
            Require(!restore.TryRestore(rejected) && ReferenceEquals(last, source.LastCommit) && source.Cooldown == cooldown, "Failed restore changed contact state.");
            Require(restore.TryRestore(initial) && source.LastCommit is null && source.Cooldown == 0, "Successful restore retained action token.");
            Require(!audio.GetNode<AudioStreamPlayer>("CaskCover").Playing && captions.GetNode<Label>("Panel/Lines/CaskCover").Text == string.Empty, "Restore replayed/retained feedback.");
            checks.Add("pause/focus/invalid time/disabled input, overlays/text focus, changed target visibility/rotation/substitution/navigation and failed/successful full restore");
            await Place(reached); Require(source.TryHandle(), "Missing-bus fixture failed to commit.");
            source.ResetPresentation(); int effects = AudioServer.GetBusIndex("Effects"); AudioServer.SetBusName(effects, "MissingEffects");
            Require(source.TryHandle() && !audio.GetNode<AudioStreamPlayer>("CaskCover").Playing, "Missing Effects fell through to Master.");
            Require(captions.GetNode<Label>("Panel/Lines/CaskCover").Text == "Container and cloth handling", "Missing bus removed caption.");
            AudioServer.SetBusName(effects, "Effects"); source.ResetPresentation();
            Require(source.TryHandle(), "Lifecycle final contact failed."); seeker.GlobalPosition = new(2132, 900); source._Process(0);
            Require(!audio.GetNode<AudioStreamPlayer>("CaskCover").Playing, "Departure did not stop contact.");
            // Saves keep character positions (schema 9), so return before comparing the full save.
            seeker.GlobalPosition = reached;
            checks.Add("missing Effects stream/bus retain commits/captions without fallback; departure cancels owned audio only");
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Lifecycle/action checks changed full save.");
            source.ResetPresentation();
            var movedFlow = GetNode<TemporaryCharacterObjectSabotageMovePresentationFlowController>("../TemporaryCharacterObjectSabotageMovePresentationFlowController");
            Require(movedFlow.TryApply(), "Actual cask move fixture failed.");
            Require(!source.TryHandle(), "Old cask location still authorized handling.");
            await Place(new(1868, 1640));
            string movedTruth = ProtectedTruth(); Require(source.TryHandle(), "Current supported moved cask unreachable.");
            RecordTruth(movedTruth, "moved-cask");
            source.ResetPresentation();
            var contaminationSource = GetNode<TemporaryCharacterObjectSabotageContaminationSource>("../TemporaryCharacterObjectSabotageContaminationSource");
            var contaminationState = GetNode<CharacterObjectSabotageContaminationController>("../CharacterObjectSabotageContaminationController");
            Require(contaminationState.TryApply(contaminationSource.Character, contaminationSource.PhysicalAccess,
                contaminationSource.WorldObjects, contaminationState.Contamination!, contaminationState.Evidence!,
                contaminationSource.Candidate), "Hidden contamination fixture failed.");
            string contaminatedTruth = ProtectedTruth(); Require(source.TryHandle(), "Hidden contamination changed availability.");
            RecordTruth(contaminatedTruth, "hidden-contamination");
            source.ResetPresentation();
            var moveState = GetNode<CharacterObjectSabotageMoveController>("../CharacterObjectSabotageMoveController");
            var moveSource = GetNode<TemporaryCharacterObjectSabotageMoveSource>("../TemporaryCharacterObjectSabotageMoveSource");
            var currentPlacement = moveState.Placements!;
            var temporaryPlacement = moveSource.Placements;
            var presenter = GetNode<CharacterObjectSabotageMovePresentationController>("../CharacterObjectSabotageMovePresentationController");
            Require(presenter.TryPresent(currentPlacement, temporaryPlacement), "Moved cancellation fixture translation failed.");
            source._Process(0); Require(!source.TryHandle(), "Stale state/presentation placement accepted.");
            Require(presenter.TryPresent(temporaryPlacement, currentPlacement), "Moved cancellation fixture restore failed.");
            checks.Add("exact moved placement follows actual cask; old position rejected; handling retains serialized sabotage/evidence/discovery/inventory, including hidden active contamination");
            seeker.GlobalPosition = reached; // Saved positions (schema 9) must match the reached baseline.
            Require(SaveSnapshotJsonSerializer.Serialize(capture.Capture()!) == before, "Extended handling checks changed durable bytes.");
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
        string output = OS.GetEnvironment("ALLRPCS_CASK_CHECK_OUTPUT");
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
        GD.Print("PASS: cask Main: actual reach/input, exact commits, full-save parity and lifecycle."); GetTree().Quit();
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
        string directory = OS.GetEnvironment("ALLRPCS_CASK_VIEWS"); Directory.CreateDirectory(directory);
        // Isolate prior caption writers in this disposable layout fixture; the live cask owner stays active.
        Node priorFootsteps = GetNode<Node>("../OpeningRegionFootstepAudioController");
        priorFootsteps.GetParent().RemoveChild(priorFootsteps); priorFootsteps.QueueFree();
        foreach (string owner in new[] { "OpeningRegionMaterialAudioController", "OpeningRegionPositionalAudioController", "OpeningRegionInteriorAudioSource", "OpeningRegionStreamWaterContactCoordinator", "OpeningRegionWaterContactAudioController" })
        { GetNode<Node>("../" + owner).SetProcess(false); }
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size; GetWindow().GrabFocus(); source.ResetPresentation(); await Frame(); await Frame();
            Button button = (Button)panel.FindChild("AdjustCaskCover", true, false);
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
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation · off-screen right"); captions.SetInterior("Outdoor sounds softened"); captions.SetWaterContact("Water contact");
            await Frame(); await Frame(); await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
            Control box = captions.GetNode<Control>("Panel");
            Require(GetViewport().GetVisibleRect().Encloses(box.GetGlobalRect()), "Seven-line panel outside viewport.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && box.GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/clipped: " + line.Name); }
            Require(GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Panel").GetGlobalRect()), "Button/status outside viewport.");
            await Capture(Path.Combine(directory, "contact-" + size.X + ".png"));
            settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, false, false));
            Require(!captions.Visible && ((Label)panel.FindChild("Status", true, false)).Text == CaskCoverHandlingPanel.StatusText, "Captions-off removed status.");
            await Capture(Path.Combine(directory, "captions-off-" + size.X + ".png")); settings.RestoreDefaults();
            AudioSettingsPanel preferences = GetNode<AudioSettingsPanel>("../AudioSettingsPanel"); preferences.Open(button); await Frame();
            Require(!source.TryHandle(), "Audio reading admitted contact."); Button credit = (Button)preferences.FindChild("SourceCredits", true, false);
            credit.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(((RichTextLabel)preferences.FindChild("Credits", true, false)).Text.Contains("Cask cover handling A02-C01", StringComparison.Ordinal), "New notice missing.");
            RichTextLabel creditText = (RichTextLabel)preferences.FindChild("Credits", true, false);
            creditText.GetVScrollBar().Value = creditText.GetVScrollBar().MaxValue; await Frame();
            await Capture(Path.Combine(directory, "credits-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(credit.HasFocus(), "Credits keyboard return failed.");
            Button history = (Button)preferences.FindChild("RecentSoundCues", true, false); history.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            await Capture(Path.Combine(directory, "history-" + size.X + ".png")); preferences._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
            Require(history.HasFocus(), "History keyboard return failed."); preferences.Close(false);
        }
        source.ResetPresentation(); await Frame(); Require(source.TryHandle(), "Native focus fixture failed.");
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frame(); await Frame();
        Require(!GetWindow().HasFocus(), "Native window did not lose focus."); source._Process(0.1);
        Require(!audio.GetNode<AudioStreamPlayer>("CaskCover").Playing, "Native focus loss did not stop contact.");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus();
        checks.Add("1280x720 and 1920x1080: seven captions/status, native mouse/Tab/Enter/Space activation/echo guard, captions-off, credits/history return; actual native focus loss");
    }
}
