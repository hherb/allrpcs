using System.Runtime.CompilerServices;
using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Dialogue;
using Allrpcs.Application.Map;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Finite ordinary owner, action, reading, restore and state-parity fixtures.</summary>
public partial class MusicAudioRuntimeSmokeCheck : Node
{
    private OpeningRegionMusicAudioController music = null!;
    private OpeningRegionMaterialAudioController material = null!;
    private AudioSettingsController settings = null!;
    private Seeker seeker = null!;
    private CharacterTurnActivityController turns = null!;
    private TemporaryCharacterTurnResponseSpeechRecorder recorder = null!;
    private readonly List<string> checks = [];
    private int serial;
    private string mode = string.Empty;
    private static readonly JsonSerializerOptions EvidenceOptions = new() { WriteIndented = true };

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_MUSIC_MAIN_CHECK") != "1") { return; }
        try
        {
            await Frame(); await Frame();
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            seeker = GetNode<Seeker>("../Seeker"); seeker.SetPhysicsProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            music = GetNode<OpeningRegionMusicAudioController>("../OpeningRegionMusicAudioController"); music.SetProcess(false);
            material = GetNode<OpeningRegionMaterialAudioController>("../OpeningRegionMaterialAudioController"); material.SetProcess(false);
            turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
            recorder = GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder"); recorder.SetProcess(false);
            settings = GetNode<AudioSettingsController>("../AudioSettingsController"); settings.RestoreDefaults();
            mode = OS.GetEnvironment("ALLRPCS_MUSIC_MODE");
            if (mode == "views") { await CaptureViews(); GetTree().Quit(); return; }
            if (mode == "muted") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Music, 0, true)); }
            if (mode == "captions-off") { settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, settings.Preferences.Mono, false)); }
            if (mode == "missing")
            {
                // Missing native resources are arranged before a fresh controller binds its own immutable streams.
                foreach (MusicPassage passage in Enum.GetValues<MusicPassage>())
                {
                    AudioStreamWav resource = GD.Load<AudioStreamWav>($"res://assets/audio/music/{(passage == MusicPassage.Exploration ? "M01" : "M02")}.wav");
                    resource.Data = [];
                }
                GetParent().RemoveChild(music); music.Free();
                music = new() { Name = "OpeningRegionMusicAudioController" }; GetParent().AddChild(music); music.SetProcess(false);
            }
            Require(music.GetChildren().OfType<AudioStreamPlayer>().Count() == 1 && music.Starts == 0, "Music autoplay/duplicate player.");
            Require(!music.GetNode<AudioStreamPlayer>("Music").Playing, "Startup is not silent.");
            if (DisplayServer.GetName() != "headless") { GetWindow().GrabFocus(); await Frame(); }
            await ReadyExploration();
            Tick(11.75); Require(music.Starts == 0, "Music started before quiet threshold.");
            Tick(0.25); Require((music.Starts == 1) == (mode is not ("muted" or "missing")), "Ordinary exploration admission mismatch: starts=" + music.Starts + "; pending=" + music.Schedule.Pending + "; active=" + music.Schedule.Active + "; cooldown=" + music.Schedule.CooldownRemaining + "; focus=" + GetViewport().GuiGetFocusOwner()?.Name + "; material=" + material.Current?.ClipId);
            checks.Add("actual ordinary projected region transition and post-collision travel; 12-second admission");
            AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
            foreach (string path in new[] { "DialogueOverlay", "SettingsOverlay", "JournalSectionPanel", "SeekerMapPanel" })
            {
                if (path == "DialogueOverlay") { GetNode<CanvasLayer>("../SettingsOverlay").Hide(); }
                CanvasLayer view = GetNode<CanvasLayer>("../" + path);
                view.Show(); music._Process(0.25); Tick(0.5);
                Require(music.Schedule.Active is null, "Reading retained music: " + path); view.Hide();
                await ReadyExploration(); Tick(12);
            }
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size; panel.Open(); await Frame(); await Frame();
                Require(panel.IsReading && GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Shade/Panel").GetGlobalRect()), "Audio reading/fit failed.");
                Require(((Label)panel.FindChild("MusicExplanation", true, false)).Text.Contains("Mute Music", StringComparison.Ordinal), "Music explanation missing.");
                music._Process(0.25); Tick(0.5); Require(music.Schedule.Active is null, "Audio panel retained music.");
                await Capture("audio-" + size.X);
                Button entry = (Button)panel.FindChild("SourceCredits", true, false); entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                Require(panel.IsReading && !panel.Visible, "Hidden Audio credits reading ignored.");
                MaterialAudioCreditsPanel credits = panel.GetNode<MaterialAudioCreditsPanel>("MaterialAudioCreditsPanel");
                string text = ((RichTextLabel)credits.FindChild("Credits", true, false)).Text;
                Require(text.Contains("Yoiyami", StringComparison.Ordinal) && text.Contains("gonzooka", StringComparison.Ordinal), "Current music contributors missing.");
                await Capture("credits-" + size.X);
                panel._Input(Action("ui_cancel")); Require(panel.Visible && entry.HasFocus(), "Credits return focus failed.");
                Button history = (Button)panel.FindChild("RecentSoundCues", true, false); history.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                Require(panel.IsReading && !panel.Visible, "Hidden Audio cue-history reading ignored.");
                panel._Input(Action("ui_cancel")); panel.Close(false);
            }
            checks.Add("six reading surfaces plus hidden-parent credits/history; both viewports and keyboard return");
            await CheckTaskBlockers();
            await ReadyExploration(); Tick(12);
            seeker.GlobalPosition = GetNode<Node2D>("../Healer").GlobalPosition;
            Record(false); Require(turns.TryComplete(), "Cannot finish reply fixture."); Tick(0.75);
            Require(music.Schedule.Active is null, "Admitted recorded reply retained music.");
            int before = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Count;
            recorder._Process(0);
            Require(GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Count == before, "Recorder duplicated a statement.");
            seeker.GlobalPosition = GetNode<Node2D>("../Healer").GlobalPosition + new Vector2(193, 0); Record(false); Require(turns.TryComplete(), "Cannot finish unheard fixture.");
            seeker.GlobalPosition = GetNode<Node2D>("../Healer").GlobalPosition + new Vector2(49, 0); Record(true); Require(turns.TryComplete(), "Cannot finish whispered fixture.");
            checks.Add("ordinary exact recorder; heard and unheard 192/48 gates; repeated frame unchanged");
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot initial = capture.Capture() ?? throw new InvalidOperationException("Missing full save root.");
            music.ResetPresentation(); Tick(180);
            OpeningRegionRemedyPreparationInputCoordinator preparation = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
            OpeningRegionRemedyPreparationSceneBinding station = GetNode<OpeningRegionRemedyPreparationSceneSource>("../OpeningRegionRemedyPreparationSceneSource").Binding!;
            OpeningRegionFirstPatientAdministrationInputCoordinator treatment = GetNode<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator");
            OpeningRegionFirstPatientSceneBinding patient = GetNode<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource").Binding!;
            seeker.GlobalPosition = new(station.StationPosition.X, station.StationPosition.Y);
            for (int index = 0; index < station.Recipe.Plan.Steps.Count + 4; index++) { preparation._UnhandledInput(Action("remedy_prepare")); }
            seeker.GlobalPosition = new(patient.PatientPosition.X + 65, patient.PatientPosition.Y);
            treatment._UnhandledInput(Action("remedy_administer")); Require(music.Schedule.Pending is null, "Rejected remote treatment admitted music.");
            seeker.GlobalPosition = new(patient.PatientPosition.X, patient.PatientPosition.Y);
            int starts = music.Starts;
            treatment._UnhandledInput(Action("remedy_administer"));
            Require((music.Schedule.Pending == MusicPassage.Treatment) == (mode != "muted"), "Committed treatment did not admit exact optional opportunity.");
            treatment._UnhandledInput(Action("remedy_administer"));
            Tick(19.75); Require(music.Starts == starts, "Treatment music overlapped foreground hold.");
            Tick(0.25); Require((music.Starts == starts + 1) == (mode is not ("muted" or "missing")), "Treatment hold/quiet admission mismatch.");
            checks.Add("ordinary grinding/preparation and exact committed treatment; rejected/repeated action; 8+12 second admission");
            string fullSave = SaveSnapshotJsonSerializer.Serialize(capture.Capture()!);
            SaveSnapshotRestorationController restore = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
            WorldEventId unknown = new("unknown-music-probe");
            SaveSnapshot rejected = new(initial.SchemaVersion, initial.Discovery, new(new([unknown]), new(unknown)), initial.CharacterMemory,
                initial.FirstPatient, initial.Journal, initial.Map, initial.InvestigationRoutes, initial.FirstPatientRecovery);
            MusicPassage? active = music.Schedule.Active;
            Require(!restore.TryRestore(rejected) && music.Schedule.Active == active, "Failed load reset music.");
            Require(restore.TryRestore(initial) && music.Schedule.Active is null && music.Schedule.Pending is null, "Successful load retained music.");
            Tick(180); Require(music.Schedule.Pending is null, "Restored treatment replayed.");
            settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Music, 0, true));
            settings.RestoreDefaults(); Require(music.Schedule.Pending is null, "Unmute/defaults replayed music.");
            music.ResetPresentation(); music._Process(0.251); Require(music.Schedule.CooldownRemaining == 180, "Long frame caught up.");
            int bus = AudioServer.GetBusIndex("Music"); AudioServer.SetBusName(bus, "MissingMusic"); music._Process(0);
            Require(!music.GetNode<AudioStreamPlayer>("Music").Playing, "Missing Music bus fell back to Master."); AudioServer.SetBusName(bus, "Music");
            checks.Add("failed/successful full save restore; no history replay; mute/defaults; long frame; missing bus; owned teardown");
            string output = OS.GetEnvironment("ALLRPCS_MUSIC_CHECK_OUTPUT");
            if (output.Length > 0)
            {
                File.WriteAllText(output + ".save.json", fullSave);
                File.WriteAllText(output, JsonSerializer.Serialize(new
                {
                    mode,
                    checks,
                    music.Starts,
                    transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController").Transcript!.Entries.Select(e => e.SpokenStatement?.Text)
                }, EvidenceOptions));
            }
            GetParent().RemoveChild(music); music.QueueFree(); await Frame();
            GD.Print("PASS: music Main: ordinary region/motion and treatment/recorder owners, reading/credits/history/focus, 12/20-second gates, no replay, load/bus failure and full state capture."); GetTree().Quit();
        }
        catch (Exception error) { GD.PushError(error.ToString()); GetTree().Quit(1); }
    }

    private async Task CheckTaskBlockers()
    {
        await ReadyExploration(); Tick(12);
        CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
        CharacterId healerId = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
        const string text = "Please wait.";
        DialogueUtterance utterance = new(seekerId, text);
        CharacterTurnRequest request = new(healerId, new(new(utterance, DialogueDeliveryMode.OrdinarySpeech), new(seekerId, [healerId])), new(utterance, DialogueActType.Request));
        Require(turns.TryStart(request), "Awaiting turn fixture failed.");
        Tick(0.75); Require(music.Schedule.Active is null, "Hidden awaiting turn did not interrupt music.");
        Require(turns.TryCancel(), "Awaiting turn cancellation failed.");
        await ReadyExploration(); Tick(12);
        CharacterModelGenerationFlowController flow = GetNode<CharacterModelGenerationFlowController>("../CharacterModelGenerationFlowController");
        CharacterModelGenerationRequest generationRequest = new(request, new([new(CharacterModelContextEntryKind.UntrustedPlayerDialogue, text, DialogueDeliveryMode.OrdinarySpeech)]));
        Require(flow.TryStart(new WaitingModel(), generationRequest, GetNode<WorldObjectController>("../WorldObjectController").WorldObjects!, new([]), TimeSpan.FromSeconds(30)), "Credential-free model fixture failed.");
        Require(flow.IsGenerating, "Generation fixture not active.");
        Tick(0.75); Require(music.Schedule.Active is null, "Hidden generation did not interrupt music.");
        Require(flow.TryCancel(), "Generation cancellation failed.");
        for (int i = 0; i < 120 && flow.IsGenerating; i++) { await Frame(); }
        Require(!flow.IsGenerating && flow.TryReset(), "Generation cleanup failed.");
        await ReadyExploration(); Tick(12);
        FocusedSearchActivityController search = GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        TemporaryFocusedSearchContextSource context = GetNode<TemporaryFocusedSearchContextSource>("../TemporaryFocusedSearchContextSource");
        FocusedSearchRequest focused = new(search.SearcherCharacterId, context.Conditions.CapabilityRequirements.Entries[0].ObjectId);
        Require(search.TryStartSearch(focused, context.Conditions, context.Capabilities, context.Inventory), "Search fixture failed.");
        Tick(0.75); Require(music.Schedule.Active is null && search.TryCancelSearch(focused), "Active search music/cleanup failed.");
        await ReadyExploration(); Tick(12);
        CharacterTurnParticipationRoundTaskController participation = GetNode<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController");
        Require(participation.TryStart(new WaitingParticipation(), [GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[0]],
            request.DialogueEvent, [], TimeSpan.FromSeconds(30)), "Participation fixture failed.");
        Tick(0.75); Require(music.Schedule.Active is null && participation.TryCancel(), "Running participation music/cleanup failed.");
        for (int i = 0; i < 120 && participation.Status == CharacterTurnParticipationRoundTaskStatus.Running; i++) { await Frame(); }
        Require(participation.TryReset(), "Participation reset failed.");
        checks.Add("hidden awaiting turn/generation/running participation with credential-free cancellation; exact active Seeker search");
    }

    private async Task CaptureViews()
    {
        AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
        {
            GetWindow().Size = size; panel.Open(); await Frame(); await Frame();
            Require(panel.IsReading && GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Shade/Panel").GetGlobalRect()), "Audio fit/reading failed.");
            await Capture("audio-" + size.X);
            Button entry = (Button)panel.FindChild("SourceCredits", true, false); entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
            Require(panel.IsReading && !panel.Visible, "Credits reading failed.");
            MaterialAudioCreditsPanel credits = panel.GetNode<MaterialAudioCreditsPanel>("MaterialAudioCreditsPanel");
            RichTextLabel label = (RichTextLabel)credits.FindChild("Credits", true, false);
            Require(label.Text.Contains("Yoiyami", StringComparison.Ordinal) && label.Text.Contains("gonzooka", StringComparison.Ordinal), "Current credits missing.");
            label.ScrollToLine(Math.Max(0, label.GetLineCount() - 10)); await Frame();
            await Capture("credits-" + size.X);
            panel._Input(Action("ui_cancel")); Require(panel.Visible && entry.HasFocus(), "Credits return focus failed."); panel.Close(false);
        }
        GD.Print("PASS: ordinary Main music explanation/controls/current credits/keyboard return at both viewports; layout fixture only.");
    }

    private sealed class WaitingModel : ICharacterModel
    {
        public async IAsyncEnumerable<CharacterModelStreamUpdate> StreamResponseAsync(CharacterModelGenerationRequest request,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _ = request;
            yield return new CharacterModelTextDelta("Waiting fixture.");
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
        }
    }

    private sealed class WaitingParticipation : ICharacterTurnParticipationModel
    {
        public async ValueTask<CharacterTurnParticipationDecision> DecideAsync(CharacterTurnParticipationConsiderationRequest request,
            CancellationToken cancellationToken)
        {
            _ = request;
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            throw new InvalidOperationException("Waiting participation must be cancelled.");
        }
    }

    private async Task ReadyExploration()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        music.ResetPresentation(); Tick(180);
        // Anchor positions are deterministic scene fixtures; the following 24px is actual collision-resolved input.
        OpeningRegionProjectionSource projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        seeker.GlobalPosition = projection.ToGlobal(new(500, 900)); material._Process(0); music._Process(0);
        seeker.GlobalPosition = projection.ToGlobal(new(2700, 900)); material._Process(0); music._Process(0);
        Input.ActionPress("move_right");
        for (int i = 0; i < 10; i++) { seeker._PhysicsProcess(0.1); }
        Input.ActionRelease("move_right");
        material._Process(0); music._Process(0);
    }
    private void Tick(double seconds) { for (int i = 0; i < (int)Math.Round(seconds * 4); i++) { music._Process(0.25); } }
    private void Record(bool whisper)
    {
        CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
        CharacterId speaker = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[0].Identity.Id;
        DialogueUtterance utterance = new(seekerId, "Please respond.");
        CharacterTurnRequest request = new(speaker, new(new(utterance, DialogueDeliveryMode.OrdinarySpeech), new(seekerId, [speaker])), new(utterance, DialogueActType.Request));
        Require(turns.TryStart(request), "Cannot start deterministic turn.");
        DialogueDelivery delivery = new(new(speaker, "Music fixture reply " + ++serial), whisper ? DialogueDeliveryMode.Whispering : DialogueDeliveryMode.OrdinarySpeech);
        Require(turns.TryReceiveResponse(new(request, CharacterTurnDisposition.Accept, delivery, new(CharacterIntentionType.Wait, null), [], [])), "Cannot receive fixture reply."); recorder._Process(0);
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
    private SignalAwaiter Frame() => ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static InputEventAction Action(string action) => new() { Action = action, Pressed = true };
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private async Task Capture(string name)
    {
        string directory = OS.GetEnvironment("ALLRPCS_MUSIC_VIEWS");
        if (directory.Length == 0 || DisplayServer.GetName() == "headless") { return; }
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Directory.CreateDirectory(directory); GetViewport().GetTexture().GetImage().SavePng(Path.Combine(directory, name + ".png"));
    }
}
