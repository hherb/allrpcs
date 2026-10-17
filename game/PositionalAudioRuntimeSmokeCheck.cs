using System.Text.Json;
using Allrpcs.Application.Audio;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Infrastructure.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Ordinary source/recorder/camera/reset probes using deterministic completed responses.</summary>
public partial class PositionalAudioRuntimeSmokeCheck : Node
{
    private OpeningRegionPositionalAudioController audio = null!;
    private CharacterTurnActivityController turns = null!;
    private TemporaryCharacterTurnResponseSpeechRecorder recorder = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private Seeker seeker = null!;
    private AudioSettingsController settings = null!;
    private readonly List<object> checks = [];
    private static readonly JsonSerializerOptions EvidenceOptions = new() { WriteIndented = true };
    private int serial;
    private bool captionsOff;

    /// <inheritdoc />
    public override async void _Ready()
    {
        if (OS.GetEnvironment("ALLRPCS_POSITIONAL_MAIN_CHECK") != "1") { return; }
        try
        {
            await Frame(); await Frame();
            GetParent().SetProcess(false);
            GetNode<Node>("../TemporaryCharacterTurnCompletionController").SetProcess(false);
            recorder = GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder");
            recorder.SetProcess(false);
            seeker = GetNode<Seeker>("../Seeker"); seeker.SetPhysicsProcess(false);
            GetNode<Healer>("../Healer").SetPhysicsProcess(false);
            GetNode<Healer>("../Healer").Visible = true; // Explicit present speaker in this disposable recorder fixture.
            audio = GetNode<OpeningRegionPositionalAudioController>("../OpeningRegionPositionalAudioController");
            turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
            captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
            settings = GetNode<AudioSettingsController>("../AudioSettingsController");
            settings.RestoreDefaults();
            string mode = OS.GetEnvironment("ALLRPCS_POSITIONAL_MODE");
            captionsOff = mode == "captions-off";
            if (mode == "muted") { settings.SetPreferences(settings.Preferences.WithChannel(AudioCategory.Master, 0, true)); }
            if (captionsOff) { settings.SetPreferences(new(settings.Preferences.Channels, settings.Preferences.Preset, settings.Preferences.Mono, false)); }
            if (mode == "missing")
            {
                audio.GetNode<AudioStreamPlayer2D>("B02").Stream = null;
                audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Stream = null;
            }
            Node2D healer = GetNode<Node2D>("../Healer");
            Node2D sage = GetNode<Node2D>("../SageHearingPosition");
            object time = GetNode<WorldTimeController>("../WorldTimeController").WorldTime;
            object? perception = GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController").Perception;
            object? objects = GetNode<WorldObjectController>("../WorldObjectController").WorldObjects;
            CharacterTurnTranscriptController transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
            MaterialAudioRuntimeSmokeCheck legacy = GetNode<MaterialAudioRuntimeSmokeCheck>("../MaterialAudioRuntimeSmokeCheck");
            _ = legacy;
            Require(audio.GetChildren().OfType<AudioStreamPlayer2D>().Count() == 2, "Unbounded positional players.");
            Require(GetNode<OpeningRegionMaterialAudioController>("../OpeningRegionMaterialAudioController").GetChildCount() == 5,
                "B02 still has a non-positional owner.");
            Require(audio.GetNode<AudioListener2D>("SeekerListener").IsCurrent(), "Missing explicit listener.");
            OpeningRegionProjectionSource projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
            foreach (var anchor in projection.Projection.Anchors)
            {
                seeker.GlobalPosition = projection.ToGlobal(new(anchor.Position.X, anchor.Position.Y));
                audio._PhysicsProcess(0); await Frame();
                Vector2 nearest = audio.GetNode<AudioStreamPlayer2D>("B02").GlobalPosition;
                Require(nearest.X >= 1600 && nearest.X <= 2000 && nearest.Y >= 452 && nearest.Y <= 548,
                    "Water player left the retained water extent.");
            }
            Transform2D retainedTransform = projection.Transform;
            projection.Position += new Vector2(50, -30); projection.Rotation = 0.3f; projection.Scale = new(1.2f, 0.85f);
            seeker.GlobalPosition = projection.ToGlobal(new(1500, 400)); audio._PhysicsProcess(0); await Frame();
            Require(audio.GetNode<AudioStreamPlayer2D>("B02").GlobalPosition.DistanceTo(projection.ToGlobal(new(1600, 452))) < 0.01f,
                "Water extent did not follow projection transform.");
            AudioListener2D worldListener = audio.GetNode<AudioListener2D>("SeekerListener");
            Require(worldListener.TopLevel && worldListener.Rotation == 0 && worldListener.Scale == Vector2.One,
                "Listener inherited source/camera transform.");
            projection.Transform = retainedTransform;
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size; await Frame();
                foreach (float zoom in new[] { 0.8f, 1f, 2f })
                {
                    Camera2D camera = GetNode<Camera2D>("../Seeker/FollowCamera");
                    camera.Zoom = new(zoom, zoom);
                    seeker.GlobalPosition = healer.GlobalPosition - new Vector2(0, 190);
                    audio._PhysicsProcess(0); camera.ResetSmoothing(); camera.ForceUpdateScroll(); await Frame();
                    int count = audio.RecentCues.Count;
                    Record(0, false);
                    Require(audio.RecentCues.Count == Math.Min(16, count + 1), "Camera changed heard admission.");
                    string text = captions.GetNode<Label>("Panel/Lines/Conversation").Text;
                    if (!captionsOff && size.X == 1280 && zoom == 2)
                    { Require(text == "Conversation below", "Actual zoom-2 off-screen bearing missing: " + text); }
                    checks.Add(new { viewport = size.ToString(), zoom, caption = text, admitted = true });
                    if (zoom == 2) { await Capture("offscreen-" + size.X); }
                }
            }
            foreach (var sample in new[] { (false, 192f), (false, 192.01f), (true, 48f), (true, 48.01f) })
            {
                seeker.GlobalPosition = healer.GlobalPosition - new Vector2(sample.Item2, 0); audio._PhysicsProcess(0);
                int before = audio.RecentCues.Count; int entries = transcript.Transcript!.Entries.Count;
                Record(0, sample.Item1);
                bool heard = sample.Item2 <= (sample.Item1 ? 48 : 192);
                Require(audio.RecentCues.Count == before + (heard ? 1 : 0), "Inclusive heard/whisper boundary changed.");
                Require(transcript.Transcript.Entries.Count == entries + 1, "Audio changed exact transcript append.");
                recorder._Process(0);
                Require(transcript.Transcript.Entries.Count == entries + 1, "Repeated frame appended twice.");
            }
            seeker.GlobalPosition = sage.GlobalPosition; audio._PhysicsProcess(0); Record(1, false);
            Require(audio.RecentCues[^1].Contains("Conversation", StringComparison.Ordinal), "Sage mapping failed.");
            seeker.GlobalPosition = sage.GlobalPosition + new Vector2(193, 0); audio._PhysicsProcess(0); await Frame();
            Require(!audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing
                && captions.GetNode<Label>("Panel/Lines/Conversation").Text.Length == 0, "Departure retained cue.");
            seeker.GlobalPosition = healer.GlobalPosition; audio._PhysicsProcess(0);
            AudioListener2D admissionListener = audio.GetNode<AudioListener2D>("SeekerListener");
            admissionListener.ClearCurrent(); Record(0, false); admissionListener.MakeCurrent();
            DialogueEvent committed = GetNode<DialogueEventController>("../DialogueEventController").LastOccurredEvent!;
            CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
            RecordedReplyPresentation notice = new(committed, committed.Delivery, committed.Utterance.SpeakerCharacterId,
                seekerId, healer, healer.GlobalPosition, 192);
            string[] beforeRejected = audio.RecentCues.ToArray();
            foreach (RecordedReplyPresentation rejectedNotice in new[]
            {
                notice with { SpeakerId = new("foreign") }, notice with { SeekerId = new("foreign") },
                notice with { Delivery = new(new(notice.SpeakerId, "Incomplete chunk."), DialogueDeliveryMode.OrdinarySpeech) },
                notice with { Speaker = sage, WorldPosition = sage.GlobalPosition }, notice with { Range = 193 },
                notice with { WorldPosition = healer.GlobalPosition + Vector2.One },
                notice with { Event = new(committed.Delivery, new(notice.SpeakerId, [])) }
            }) { audio.ReceiveRecorded(rejectedNotice); }
            Require(audio.RecentCues.SequenceEqual(beforeRejected), "Foreign/stale/chunk/source/range notification admitted.");
            audio.ReceiveRecorded(notice);
            Require(!audio.RecentCues.SequenceEqual(beforeRejected), "Valid exact notification did not admit after negative probes.");
            audio.ResetPresentation();
            int dialogueBus = AudioServer.GetBusIndex("Dialogue");
            AudioServer.SetBusName(dialogueBus, "MissingDialogueProbe");
            Record(0, false);
            Require(audio.RecentCues.Count == 1 && !audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing,
                "Missing bus played on fallback Master or erased generic history.");
            AudioServer.SetBusName(dialogueBus, "Dialogue");
            Action<RecordedReplyPresentation> failingObserver = _ => throw new InvalidOperationException("Expected isolated presentation probe.");
            recorder.ReplyRecorded += failingObserver;
            int transcriptBeforeFailure = transcript.Transcript!.Entries.Count;
            Record(0, false); recorder._Process(0);
            recorder.ReplyRecorded -= failingObserver;
            Require(transcript.Transcript.Entries.Count == transcriptBeforeFailure + 1,
                "Presentation failure changed commit/one-time recorder ownership.");
            for (int i = 0; i < 20; i++) { Record(0, false); }
            Require(audio.RecentCues.Count == 16, "History unbounded.");
            Require(ReferenceEquals(time, GetNode<WorldTimeController>("../WorldTimeController").WorldTime)
                && ReferenceEquals(perception, GetNode<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController").Perception)
                && ReferenceEquals(objects, GetNode<WorldObjectController>("../WorldObjectController").WorldObjects), "Audio changed world roots.");
            AudioSettingsPanel panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
            foreach (Vector2I size in new[] { new Vector2I(1280, 720), new Vector2I(1600, 900) })
            {
                GetWindow().Size = size; panel.Open(); await Frame(); await Frame();
                Require(GetViewport().GetVisibleRect().Encloses(panel.GetNode<Control>("Shade/Panel").GetGlobalRect()), "Audio panel exceeds viewport: " + panel.GetNode<Control>("Shade/Panel").GetGlobalRect());
                Button entry = (Button)panel.FindChild("RecentSoundCues", true, false);
                entry.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                PositionalSoundCueHistoryPanel history = panel.GetNode<PositionalSoundCueHistoryPanel>("PositionalSoundCueHistoryPanel");
                Require(history.Visible && !panel.Visible && ((RichTextLabel)history.FindChild("Entries", true, false)).Text.Contains("Conversation", StringComparison.Ordinal), "Accessible history missing.");
                await Capture("history-" + size.X);
                panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true });
                Require(panel.Visible && entry.HasFocus() && !history.Visible, "History return focus failed.");
                Button credits = (Button)panel.FindChild("SourceCredits", true, false);
                credits.EmitSignal(BaseButton.SignalName.Pressed); await Frame();
                MaterialAudioCreditsPanel creditPanel = panel.GetNode<MaterialAudioCreditsPanel>("MaterialAudioCreditsPanel");
                Require(((RichTextLabel)creditPanel.FindChild("Credits", true, false)).Text.Contains("Kenney", StringComparison.Ordinal), "New credit missing.");
                await Capture("credits-" + size.X);
                panel._Input(new InputEventAction { Action = "ui_cancel", Pressed = true }); panel.Close(false);
                Require(GetViewport().GetVisibleRect().Encloses(captions.GetNode<Control>("Panel").GetGlobalRect()), "Four caption lines exceed viewport.");
                captions.SetAction("Grinding", 8); captions.SetFootstep("Footsteps on earth");
                await Frame(); await Capture("captions-" + size.X);
            }
            int retained = audio.RecentCues.Count;
            Require(turns.TryCancel(), "Cannot arrange cancellation of recorded reply.");
            audio._Process(0);
            Require(!audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing
                && captions.GetNode<Label>("Panel/Lines/Conversation").Text.Length == 0
                && audio.RecentCues.Count == retained, "Cancellation retained active cue or erased history.");
            Record(0, false);
            seeker.GlobalPosition += new Vector2(97, 0); audio._PhysicsProcess(0);
            Require(!audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing, "Teleport retained queued reply.");
            seeker.GlobalPosition = healer.GlobalPosition; audio._PhysicsProcess(0); recorder._Process(0);
            Require(!audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing, "Return replayed recorded reply.");
            AudioListener2D listener = audio.GetNode<AudioListener2D>("SeekerListener");
            listener.ClearCurrent(); retained = audio.RecentCues.Count; Record(0, false);
            Require(audio.RecentCues.Count == retained, "Missing listener admitted fallback cue."); listener.MakeCurrent();
            ArrangeSave();
            SaveSnapshotCaptureController capture = GetNode<SaveSnapshotCaptureController>("../SaveSnapshotCaptureController");
            SaveSnapshot snapshot = capture.Capture() ?? throw new InvalidOperationException("Missing full save root.");
            string fullSave = SaveSnapshotJsonSerializer.Serialize(snapshot);
            int beforeFailed = audio.RecentCues.Count;
            WorldEventId unknown = new("unknown-positional-probe");
            SaveSnapshot rejected = new(snapshot.SchemaVersion, snapshot.Discovery,
                new WorldEventSaveState(new WorldEventLogSnapshot([unknown]), new WorldTimeSnapshot(unknown)),
                snapshot.CharacterMemory, snapshot.FirstPatient, snapshot.Journal, snapshot.Map, snapshot.InvestigationRoutes, snapshot.FirstPatientRecovery);
            Require(!GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController").TryRestore(rejected)
                && audio.RecentCues.Count == beforeFailed, "Failed restore erased history.");
            Require(GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController").TryRestore(snapshot), "Full successful restore failed.");
            Require(audio.RecentCues.Count == 0 && !audio.GetNode<AudioStreamPlayer2D>("RecordedReply").Playing, "Reset retained history/player.");
            string output = OS.GetEnvironment("ALLRPCS_POSITIONAL_CHECK_OUTPUT");
            if (output.Length > 0)
            {
                File.WriteAllText(output + ".save.json", fullSave);
                File.WriteAllText(output, JsonSerializer.Serialize(new
                {
                    mode,
                    checks,
                    transcript = transcript.Transcript!.Entries.Select(e => e.SpokenStatement?.Text).ToArray(),
                    unrelatedRootsIdentical = true,
                    recentCueLimit = 16
                }, EvidenceOptions));
            }
            GD.Print("PASS: positional Main; 11 water anchors; actual camera/viewport and 192/48 audience boundaries; exact recorder; repeat/departure/history/credits/focus/reset; world roots unchanged.");
            GetTree().Quit();
        }
        catch (Exception exception) { GD.PushError(exception.ToString()); GetTree().Quit(1); }
    }

    private void Record(int participant, bool whisper)
    {
        if (turns.Activity is { State: var state } && ReferenceEquals(state, CharacterTurnActivityState.Speaking))
        { Require(turns.TryComplete(), "Cannot complete arranged prior turn."); }
        CharacterId seekerId = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id;
        CharacterId speaker = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource").Participants[participant].Identity.Id;
        DialogueUtterance player = new(seekerId, "Please respond.");
        CharacterTurnRequest request = new(speaker, new(new(player, DialogueDeliveryMode.OrdinarySpeech), new(seekerId, [speaker])), new(player, DialogueActType.Request));
        Require(turns.TryStart(request), "Cannot start deterministic response fixture.");
        DialogueDelivery delivery = new(new(speaker, "Recorded reply " + ++serial), whisper ? DialogueDeliveryMode.Whispering : DialogueDeliveryMode.OrdinarySpeech);
        Require(turns.TryReceiveResponse(new(request, CharacterTurnDisposition.Accept, delivery, new(CharacterIntentionType.Wait, null), [], [])), "Cannot receive deterministic response.");
        recorder._Process(0);
    }
    private void ArrangeSave()
    {
        JournalController journal = GetNode<JournalController>("../JournalController");
        if (journal.Journal is null) { Require(GetNode<RepresentativeJournalCompositionCoordinator>("../RepresentativeJournalCompositionCoordinator").TryComposeAndInitialize(), "Journal fixture failed."); }
        SeekerMapController map = GetNode<SeekerMapController>("../SeekerMapController");
        if (map.Map is null) { MapSaveStateRestorationProjection.Restore(new MapSaveState(GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource").Character.Identity.Id, [], [], [])).InstallInto(map); }
        OpeningRegionInvestigationRouteProgressController routes = GetNode<OpeningRegionInvestigationRouteProgressController>("../OpeningRegionInvestigationRouteProgressController");
        if (routes.Progress is null) { Require(SaveRouteLifecycleSmokeArrangement.TryInitializeEmpty(routes, map, GetNode<OpeningRegionOldShrineSceneSource>("../OpeningRegionOldShrineSceneSource")), "Route fixture failed."); }
    }
    private SignalAwaiter Frame() => ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    private static void Require(bool value, string message) { if (!value) { throw new InvalidOperationException(message); } }
    private async Task Capture(string name)
    {
        string directory = OS.GetEnvironment("ALLRPCS_POSITIONAL_VIEWS");
        if (directory.Length == 0 || DisplayServer.GetName() == "headless") { return; }
        await ToSignal(RenderingServer.Singleton, RenderingServer.SignalName.FramePostDraw);
        Directory.CreateDirectory(directory);
        GetViewport().GetTexture().GetImage().SavePng(Path.Combine(directory, name + ".png"));
    }
}
