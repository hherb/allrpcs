using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Finite ordinary Main fixtures for interior ownership, contacts, reset and full-state parity.</summary>
public partial class InteriorAudioRuntimeSmokeCheck : Node
{
    private static readonly JsonSerializerOptions EvidenceOptions = new() { WriteIndented = true };
    private OpeningRegionInteriorAudioSource interior = null!;
    private OpeningRegionFootstepMaterialSource floors = null!;
    private OpeningRegionFootstepAudioController footsteps = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private OpeningRegionMusicAudioController music = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private AudioSettingsController settings = null!;
    private Seeker seeker = null!;
    private readonly List<string> checks = [];
    private string mode = string.Empty;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_INTERIOR_MAIN_CHECK") != "1") { return; }
        try
        {
            for (int i = 0; i < 8; i++) { await Frame(); }
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            interior = GetNode<OpeningRegionInteriorAudioSource>("../OpeningRegionInteriorAudioSource");
            floors = GetNode<OpeningRegionFootstepMaterialSource>("../OpeningRegionFootstepMaterialSource");
            footsteps = GetNode<OpeningRegionFootstepAudioController>("../OpeningRegionFootstepAudioController");
            material = GetNode<OpeningRegionMaterialAudioController>("../OpeningRegionMaterialAudioController");
            music = GetNode<OpeningRegionMusicAudioController>("../OpeningRegionMusicAudioController");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            seeker = GetNode<Seeker>("../Seeker");
            settings.RestoreDefaults();
            mode = OS.GetEnvironment("ALLRPCS_INTERIOR_MODE");
            AudioPreferences p = settings.Preferences;
            if (mode is "muted" or "zero")
            { foreach (AudioCategory category in Enum.GetValues<AudioCategory>()) { p = p.WithChannel(category, mode == "zero" ? 0 : 1, mode == "muted"); } }
            settings.SetPreferences(new(p.Channels, mode == "mono-reduced" ? AudioMixPreset.ReducedRange : AudioMixPreset.Default, mode == "mono-reduced", true));
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frame(); }
            await Place(new(2640, 1450));
            Require(interior.Current.Room == InteriorAudioRoom.Sickhouse && Math.Abs(interior.Gain(2) - Math.Pow(10, -9.0 / 20)) < 0.001, "Sickhouse profile not live: " + interior.Current);
            Require(floors.Select(new(2640, 1450)) == FootstepMaterial.Timber, "Missing interior timber; " + floors.Status);
            Require(interior.Gain(0) == 1, "Shelter bed changed.");
            await Place(new(1500, 220));
            Require(interior.Current.Room == InteriorAudioRoom.Laundry && Math.Abs(interior.Gain(1) - Math.Pow(10, -3.0 / 20)) < 0.001, "Laundry profile not live: " + interior.Current);
            Require(floors.Select(new(1500, 220)) == FootstepMaterial.Earth, "Missing laundry earth; " + floors.Status);
            Require(floors.Select(new(2480, 1400)) is null, "Wall acquired floor support.");
            Require(floors.Select(new(1800, 500)) == FootstepMaterial.Timber, "Retained crossing changed.");
            Require(GetNode<Node>("../OpeningRegionFirstPatientSceneSource/Walls").GetChildCount() == 5, "Sickhouse collision count changed.");
            Require(GetNode<Node>("../OpeningRegionStreamLaundrySceneSource/LaundryBoundaries").GetChildCount() == 6, "Laundry boundaries changed.");
            OpeningRegionProjection projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").Projection;
            Require(projection.Anchors.Count == 11 && projection.Connections.Count == 12, "Topology changed.");
            checks.Add("live sickhouse/laundry layouts, exact derived doors, supported floors, unchanged shelter/exterior/topology/collision owners");
            if (mode == "views") { await CaptureViews(); GetTree().Quit(); return; }
            Require(footsteps.LastContact is null, "Standing/entry produced a contact.");
            await Place(new(2528, 1270));
            long contacts = footsteps.ContactCount;
            await Move("move_down", 40);
            Require(footsteps.ContactCount > contacts, "Actual doorway motion produced no contact.");
            await Place(new(2528, 1360)); contacts = footsteps.ContactCount;
            await Move("move_right", 35);
            Require(footsteps.ContactCount > contacts, "Interior post-collision motion produced no contact.");
            await Place(new(1500, 220)); contacts = footsteps.ContactCount;
            await Move("move_right", 10); await Move("move_left", 20);
            Require(footsteps.ContactCount > contacts, "Laundry motion produced no contact.");
            await Place(new(1500, 220));
            OpeningRegionStreamLaundryLayout laundry = GetNode<OpeningRegionStreamLaundrySceneSource>("../OpeningRegionStreamLaundrySceneSource").Layout;
            await Place(new(1400, laundry.LaundryWoodlandOpening.Y)); await Move("move_right", 20);
            Require(seeker.GlobalPosition.X > 1428, "West portal traversal blocked.");
            await Move("move_left", 20);
            Require(seeker.GlobalPosition.X < 1420, "West return traversal blocked.");
            await Place(new(1600, laundry.LaundryStreamOpening.Y)); await Move("move_left", 20);
            Require(seeker.GlobalPosition.X < 1572, "East portal traversal blocked.");
            await Move("move_right", 20);
            Require(seeker.GlobalPosition.X > 1580, "East return traversal blocked.");
            checks.Add("actual collision-resolved sickhouse entry/interior motion, laundry earth contacts and both opening/return paths");
            await Place(new(2640, 1450));
            music.SetProcess(false); material.SetProcess(false);
            music.ResetPresentation();
            material.Present(new(0, "B01", "Sheltered rain", 1)); music._Process(0);
            await Place(new(1500, 220)); music._Process(0);
            Require(music.Schedule.Pending is null, "Enclosure manufactured a regional music opportunity.");
            material.SetProcess(true); music.SetProcess(true);
            RecordReply();
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Missing complete save root.");
            OpeningRegionRemedyPreparationSceneBinding station = GetNode<OpeningRegionRemedyPreparationSceneSource>("../OpeningRegionRemedyPreparationSceneSource").Binding!;
            await Place(new(station.StationPosition.X, station.StationPosition.Y));
            OpeningRegionRemedyPreparationInputCoordinator preparation = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
            for (int i = 0; i < station.Recipe.Plan.Steps.Count + 4; i++) { preparation._UnhandledInput(Action("remedy_prepare")); }
            OpeningRegionFirstPatientSceneBinding patient = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource").Binding!;
            await Place(new(patient.PatientPosition.X, patient.PatientPosition.Y));
            GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator")._UnhandledInput(Action("remedy_administer"));
            string fullSave = SaveSnapshotJsonSerializer.Serialize(capture.Capture()!);
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            WorldEventId unknown = new("unknown-interior-probe");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            InteriorAudioSnapshot before = interior.Current;
            Require(!restore.TryRestore(rejected) && interior.Current == before, "Failed restore reset enclosure.");
            Require(restore.TryRestore(initial) && interior.Current.Room == InteriorAudioRoom.None && interior.Gain(1) == 1, "Successful restore retained enclosure.");
            checks.Add("ordinary preparation/treatment/recorded reply and failed/successful complete save restore without indoor world/perception state");
            await Place(new(2640, 1450));
            GetTree().Paused = true; interior._Process(0.1); RequireNeutral("pause"); GetTree().Paused = false;
            await Place(new(2640, 1450));
            seeker.SetPhysicsProcess(false); interior._Process(0.1); RequireNeutral("disabled Seeker"); seeker.SetPhysicsProcess(true);
            await Place(new(2640, 1450));
            interior._Process(0.251); RequireNeutral("long frame");
            await Place(new(2640, 1450));
            seeker.GlobalPosition = new(500, 900); interior._Process(0); RequireNeutral("teleport");
            await Place(new(2640, 1450));
            Node2D owner = GetNode<Node2D>("../OpeningRegionFirstPatientSceneSource");
            owner.Position += Vector2.One; interior._Process(0.1); RequireNeutral("foreign transform"); owner.Position -= Vector2.One;
            await Place(new(2640, 1450));
            int bus = AudioServer.GetBusIndex("Ambience"); AudioServer.SetBusName(bus, "MissingAmbience");
            GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController")._Process(0);
            Require(GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController").GetNode<AudioStreamPlayer2D>("B02").VolumeLinear == 0, "Missing B02 bus fell back to Master.");
            AudioServer.SetBusName(bus, "Ambience");
            footsteps.RemoveDiagnosticResources();
            footsteps.PresentContact(new(FootstepMaterial.Timber, FootstepLeg.Left, 1, default));
            Require(footsteps.GetChildren().OfType<AudioStreamPlayer>().All(player => !player.Playing), "Missing contact resources produced audio.");
            Require(captions.GetNode<Label>("Panel/Lines/Footstep").Text == "Footsteps on timber", "Missing resources removed safe contact caption.");
            footsteps.ResetPresentation();
            GetParent().RemoveChild(interior); RequireNeutral("removal");
            Require(floors.Select(new(2640, 1450)) is null, "Missing interior source lost original silent fallback.");
            material._Process(0.25); Require(material.GetNode<AudioStreamPlayer>("B01").VolumeLinear == 1, "Missing interior source did not restore neutral material gain.");
            GetParent().AddChild(interior);
            checks.Add("pause, disabled, long frame, teleport, foreign transform, missing bus/source, neutral fallback and owned cancellation");
            string output = OS.GetEnvironment("ALLRPCS_INTERIOR_CHECK_OUTPUT");
            if (output.Length > 0)
            {
                File.WriteAllText(output + ".save.json", fullSave);
                File.WriteAllText(output, JsonSerializer.Serialize(new
                {
                    mode,
                    checks,
                    transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
                }, EvidenceOptions));
            }
            GetParent().RemoveChild(interior); interior.QueueFree();
            GD.Print("PASS: interior Main: live owners/portals, actual motion, save/transcript, cancellation and existing audio isolation."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task Place(Vector2 local)
    {
        Input.ActionRelease("move_down"); Input.ActionRelease("move_right"); Input.ActionRelease("move_left");
        seeker.GlobalPosition = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource").ToGlobal(local);
        footsteps.ResetPresentation();
        for (int i = 0; i < 50; i++) { await Frame(); }
        // Graphical idle frames can run faster than presentation time; settle the layout fixture explicitly.
        if (mode == "views") { for (int i = 0; i < 3; i++) { interior._Process(0.25); } }
    }
    private async Task Move(string action, int frames)
    {
        Input.ActionPress(action);
        for (int i = 0; i < frames; i++) { await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame); }
        Input.ActionRelease(action);
    }
    private void RequireNeutral(string context) => Require(interior.Current.Room == InteriorAudioRoom.None && interior.Gain(1) == 1,
        "Interior cancellation failed: " + context);
    private void RecordReply()
    {
        seeker.GlobalPosition = GetNode<Node2D>("../Healer").GlobalPosition;
        CharacterId player = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
        CharacterId healer = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
        DialogueUtterance utterance = new(player, "Please respond.");
        CharacterTurnRequest request = new(healer, new(new(utterance, DialogueDeliveryMode.OrdinarySpeech), new(player, [healer])), new(utterance, DialogueActType.Request));
        CharacterTurnActivityController turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        Require(turns.TryStart(request), "Reply fixture start failed.");
        DialogueDelivery reply = new(new(healer, "Interior fixture reply."), DialogueDeliveryMode.OrdinarySpeech);
        Require(turns.TryReceiveResponse(new(request, CharacterTurnDisposition.Accept, reply, new(CharacterIntentionType.Wait, null), [], [])), "Reply fixture response failed.");
        GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder")._Process(0);
        Require(turns.TryComplete(), "Reply fixture completion failed.");
    }
    private void ArrangeSave()
    {
        JournalController journal = GetNode<JournalController>("../JournalController");
        if (journal.Journal is null) { Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(), "Journal fixture failed."); }
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        if (map.Map is null) { MapSaveStateRestorationProjection.Restore(new(GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id, [], [], [])).InstallInto(map); }
        OpeningRegionInvestigationRouteProgressController routes = GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
        if (routes.Progress is null) { Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")), "Route fixture failed."); }
    }
    private async Task CaptureViews()
    {
        string directory = OS.GetEnvironment("ALLRPCS_INTERIOR_VIEWS"); Directory.CreateDirectory(directory);
        AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1920, 1080) })
        {
            GetWindow().Size = size;
            await Place(new(2640, 1450));
            Input.ActionPress("move_right");
            captions.SetRegion("Wind, rain and timber creak"); captions.SetAction("Treatment handling", 60);
            captions.SetFootstep("Footsteps on timber"); captions.SetConversation("Conversation · off-screen right");
            captions.SetInterior("Outdoor sounds softened");
            await Frame(); await Frame();
            Control frame = captions.GetNode<Control>("Panel");
            Require(GetViewport().GetVisibleRect().Encloses(frame.GetGlobalRect()), "Five-line panel outside viewport.");
            foreach (Label line in captions.GetNode<VBoxContainer>("Panel/Lines").GetChildren().OfType<Label>())
            { Require(line.Text.Length > 0 && frame.GetGlobalRect().Encloses(line.GetGlobalRect()), "Caption missing/overflow: " + line.Name); }
            await Capture(directory, "captions-" + size.X);
            settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Master, 1, true));
            Require(captions.Visible, "Muted output removed captions."); await Capture(directory, "muted-" + size.X);
            Input.ActionRelease("move_right");
            settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, settings.Preferences.Mono, false));
            Require(!captions.Visible, "Captions-off setting ignored."); settings.RestoreDefaults();
            panel.Open(); await Frame();
            Button credits = (Button)panel.FindChild("SourceCredits", true, false); credits.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(panel.IsReading, "Credits reading owner lost."); await Capture(directory, "credits-" + size.X);
            panel._Input(Action("ui_cancel")); Require(panel.Visible && credits.HasFocus(), "Credits keyboard return failed.");
            Button history = (Button)panel.FindChild("RecentSoundCues", true, false); history.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            await Capture(directory, "history-" + size.X);
            panel._Input(Action("ui_cancel")); Require(panel.Visible && history.HasFocus(), "History keyboard return failed."); panel.Close(false);
        }
        GetWindow().Mode = Window.ModeEnum.Minimized; await Frame(); await Frame();
        Require(!GetWindow().HasFocus(), "Focus-loss fixture did not lose native window focus.");
        interior._Process(0.1); RequireNeutral("native window focus loss");
        GetWindow().Mode = Window.ModeEnum.Windowed; GetWindow().GrabFocus(); await Frame();
        checks.Add("five noninteractive caption lines, mute/captions-off, credits/history and keyboard return at 1280x720 and 1920x1080; native focus-loss reset");
        File.WriteAllText(Path.Combine(directory, "checks.json"), JsonSerializer.Serialize(checks, EvidenceOptions));
        GD.Print("PASS: interior five-line captions/credits/history/mute/keyboard at both viewports.");
    }
    private async Task Capture(string directory, string name)
    {
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        GetViewport().GetTexture().GetImage().SavePng(Path.Combine(directory, name + ".png"));
    }
    private SignalAwaiter Frame() => ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static InputEventAction Action(string name) => new() { Action = name, Pressed = true };
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
}
