using Allrpcs.Application.Audio;
using Allrpcs.Application.Discovery;
using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Ingredients;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Optional accepted music from existing presentation owners; never mutates gameplay.</summary>
public partial class OpeningRegionMusicAudioController : Node
{
    private readonly RestrainedMusicSchedule schedule = new();
    private readonly Dictionary<MusicPassage, AudioStreamWav?> streams = [];
    private readonly HashSet<string> reported = [];
    private AudioStreamPlayer player = null!;
    private AudioSettingsController? settings;
    private OpeningRegionMaterialAudioController? material;
    private OpeningRegionFirstPatientSceneSource? patient;
    private OpeningRegionFirstPatientAdministrationInputCoordinator? administration;
    private OpeningRegionRemedyPreparationInputCoordinator? preparation;
    private TemporaryCharacterTurnResponseSpeechRecorder? recorder;
    private CharacterTurnActivityController? turns;
    private CharacterTurnTranscriptController? transcript;
    private TemporaryOrdinaryCharacterParticipantSource? roster;
    private TemporarySeekerCharacterSource? seekerSource;
    private CharacterModelGenerationFlowController? generation;
    private CharacterTurnParticipationRoundTaskController? participation;
    private FocusedSearchActivityController? search;
    private Seeker? seeker;
    private SaveSnapshotRestorationController? restoration;
    private AudioSettingsPanel? audioPanel;
    private readonly List<CanvasLayer> reading = [];
    private Window? window;
    private DialogueDelivery? lastReply;
    private readonly object diagnosticTreatment = new();

    /// <summary>Ordinary Main observes live owners; the isolated proof starts stopped.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Read-only transient schedule for finite ordinary/proof checks.</summary>
    internal RestrainedMusicSchedule Schedule => schedule;
    /// <summary>Monotonic presentation starts, without a saved world event.</summary>
    internal int Starts { get; private set; }
    /// <summary>The last optional passage whose resource was actually started.</summary>
    internal MusicPassage? LastStarted { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        settings = GetNodeOrNull<AudioSettingsController>("../AudioSettingsController");
        player = new() { Name = "Music", Bus = "Music", Autoplay = false, MaxPolyphony = 1, PitchScale = 1, VolumeLinear = 0 };
        AddChild(player);
        foreach (MusicPassage passage in Enum.GetValues<MusicPassage>())
        {
            string id = passage == MusicPassage.Exploration ? "M01" : "M02";
            string path = $"res://assets/audio/music/{id}.wav";
            AudioStreamWav? source = ResourceLoader.Exists(path) ? GD.Load<AudioStreamWav>(path) : null;
            AudioStreamWav? stream = source is null || source.GetLength() <= 0 ? null : (AudioStreamWav)source.Duplicate();
            if (stream is not null) { stream.LoopMode = AudioStreamWav.LoopModeEnum.Disabled; }
            else { Report(id, $"Missing optional music {id}; gameplay remains available."); }
            streams.Add(passage, stream);
        }
        player.Finished += OnFinished;
        if (settings is not null) { settings.Changed += OnPreferencesChanged; }
        if (!Automatic) { return; }
        material = GetNodeOrNull<OpeningRegionMaterialAudioController>("../OpeningRegionMaterialAudioController");
        patient = GetNodeOrNull<OpeningRegionFirstPatientSceneSource>("../OpeningRegionFirstPatientSceneSource");
        administration = GetNodeOrNull<OpeningRegionFirstPatientAdministrationInputCoordinator>("../OpeningRegionFirstPatientAdministrationInputCoordinator");
        preparation = GetNodeOrNull<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
        recorder = GetNodeOrNull<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder");
        turns = GetNodeOrNull<CharacterTurnActivityController>("../CharacterTurnActivityController");
        transcript = GetNodeOrNull<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        roster = GetNodeOrNull<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource");
        seekerSource = GetNodeOrNull<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        generation = GetNodeOrNull<CharacterModelGenerationFlowController>("../CharacterModelGenerationFlowController");
        participation = GetNodeOrNull<CharacterTurnParticipationRoundTaskController>("../CharacterTurnParticipationRoundTaskController");
        seeker = GetNodeOrNull<Seeker>("../Seeker");
        search = GetNodeOrNull<FocusedSearchActivityController>("../Seeker/FocusedSearchActivityController");
        restoration = GetNodeOrNull<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        audioPanel = GetNodeOrNull<AudioSettingsPanel>("../AudioSettingsPanel");
        foreach (string path in new[] { "DialogueOverlay", "SettingsOverlay", "JournalSectionPanel", "SeekerMapPanel" })
        {
            CanvasLayer? panel = GetNodeOrNull<CanvasLayer>("../" + path);
            if (panel is not null) { reading.Add(panel); }
        }
        if (administration is not null) { administration.TreatmentCompleted += OnTreatment; }
        if (preparation is not null) { preparation.GrindingCompleted += OnGrinding; }
        if (recorder is not null) { recorder.ReplyRecorded += OnReply; }
        if (seeker is not null) { seeker.MotionCompleted += OnMotion; }
        if (restoration is not null) { restoration.PresentationReset += ResetPresentation; }
        window = GetWindow();
        window.FocusExited += ResetPresentation;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Automatic) { return; }
        if (!OwnersValid()) { Report("owners", "Optional music owner unavailable; presentation stays silent."); ResetPresentation(); return; }
        bool audible = Audible();
        schedule.ObserveRegion(material!.Current?.ClipId, audible);
        bool focus = DisplayServer.GetName() == "headless" || window!.HasFocus();
        Advance(delta, Blocked(), audible, focus);
    }

    /// <summary>Clears transient score after successful restore or discontinuity, without replay.</summary>
    internal void ResetPresentation()
    {
        schedule.Reset(); lastReply = null;
        if (player is not null) { player.Stop(); player.VolumeLinear = 0; }
    }

    /// <summary>Explicit stopped-proof observation; never overrides a live regional owner.</summary>
    internal void DiagnosticRegion(string? clip)
    {
        RequireDiagnostic(); schedule.ObserveRegion(clip, Audible());
    }

    /// <summary>Explicit stopped-proof motion, without moving an ordinary character.</summary>
    internal void DiagnosticMotion(FootstepMotionSample sample)
    {
        RequireDiagnostic(); OnMotion(sample);
    }

    /// <summary>Explicit stopped-proof treatment, without an administration or world event.</summary>
    internal void DiagnosticTreatment()
    {
        RequireDiagnostic(); schedule.ObserveTreatment(diagnosticTreatment, diagnosticTreatment, Audible()); Sync();
    }

    /// <summary>Finite output-proof step; production observations remain separately bound.</summary>
    internal void DiagnosticAdvance(double delta, bool blocked = false, bool focused = true)
    {
        RequireDiagnostic(); Advance(delta, blocked, Audible(), focused);
    }

    /// <summary>Protects foreground in the isolated proof without borrowing mixer leases.</summary>
    internal void DiagnosticForeground(double seconds)
    {
        RequireDiagnostic(); schedule.Foreground(seconds); Sync();
    }

    /// <summary>Removes only proof-owned stream bindings for missing-resource output checks.</summary>
    internal void DiagnosticMissing()
    {
        RequireDiagnostic();
        foreach (MusicPassage passage in Enum.GetValues<MusicPassage>()) { streams[passage] = null; }
    }

    /// <summary>Restores proof-owned bindings from unchanged installed resources between cases.</summary>
    internal void DiagnosticRestoreResources()
    {
        RequireDiagnostic();
        foreach (MusicPassage passage in Enum.GetValues<MusicPassage>())
        {
            string id = passage == MusicPassage.Exploration ? "M01" : "M02";
            streams[passage] = GD.Load<AudioStreamWav>($"res://assets/audio/music/{id}.wav");
        }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (settings is not null) { settings.Changed -= OnPreferencesChanged; }
        if (administration is not null) { administration.TreatmentCompleted -= OnTreatment; }
        if (preparation is not null) { preparation.GrindingCompleted -= OnGrinding; }
        if (recorder is not null) { recorder.ReplyRecorded -= OnReply; }
        if (seeker is not null) { seeker.MotionCompleted -= OnMotion; }
        if (restoration is not null) { restoration.PresentationReset -= ResetPresentation; }
        if (window is not null) { window.FocusExited -= ResetPresentation; }
        if (player is not null) { player.Finished -= OnFinished; ResetPresentation(); }
    }

    private void Advance(double delta, bool blocked, bool audible, bool focused)
    {
        MusicPassage? start = schedule.Advance(delta, blocked, audible, focused);
        if (start is { } passage)
        {
            if (!BusExists() || streams[passage] is not { } stream)
            {
                Report("start", "Optional music resource/bus unavailable; start discarded."); schedule.Finished();
            }
            else
            {
                player.Stream = stream; player.VolumeLinear = 1; player.Play(); Starts++; LastStarted = passage;
            }
        }
        Sync();
    }

    private void Sync()
    {
        if (!BusExists()) { if (schedule.Active is not null) { schedule.Finished(); } player.Stop(); player.VolumeLinear = 0; return; }
        player.VolumeLinear = (float)schedule.Gain;
        if (schedule.Active is null) { player.Stop(); }
    }

    private bool Audible() => settings is not null && GodotObject.IsInstanceValid(settings)
        && settings.Preferences.Channels.Where(channel => channel.Category is AudioCategory.Master or AudioCategory.Music)
            .All(channel => channel.Gain > 0 && !channel.Muted);

    private bool OwnersValid() => new Node?[] { settings, material, patient, administration, preparation, recorder,
        turns, transcript, roster, seekerSource, generation, participation, seeker, search, restoration, audioPanel, window }
        .All(owner => owner is not null && GodotObject.IsInstanceValid(owner) && owner.IsInsideTree())
        && reading.Count == 4 && reading.All(panel => GodotObject.IsInstanceValid(panel) && panel.IsInsideTree())
        && BusExists();

    private bool Blocked() => reading.Any(panel => panel.Visible) || audioPanel!.IsReading
        || GetViewport().GuiGetFocusOwner() is { } focus && focus.IsVisibleInTree()
        || turns!.Activity?.State is { } state && (state == CharacterTurnActivityState.AwaitingResponse || state == CharacterTurnActivityState.Speaking)
        || generation!.IsGenerating || participation!.Status == CharacterTurnParticipationRoundTaskStatus.Running
        || FocusedSearchActivityIndicatorPolicy.ShouldShow(search!.SearcherCharacterId, search.Activities);

    private void OnTreatment(OpeningRegionFirstPatientSceneBinding binding)
    {
        schedule.ObserveTreatment(binding, patient?.Binding, Audible()); Sync();
    }

    private void OnGrinding(IngredientPreparationRecipe recipe, IngredientPreparationStep step, Vector2 position)
    {
        _ = position;
        if (OpeningRegionMaterialAudioProjection.IsGrinding(recipe, step)) { schedule.Foreground(RestrainedMusicSchedule.ActionHoldSeconds); Sync(); }
    }

    private void OnReply(RecordedReplyPresentation notice)
    {
        if (seeker is null || seekerSource is null || roster is null || turns is null || transcript is null
            || ReferenceEquals(lastReply, notice.Delivery) || !GodotObject.IsInstanceValid(notice.Speaker)
            || !ReferenceEquals(notice.Event.Delivery, notice.Delivery)
            || !ReferenceEquals(turns.Activity?.Response?.Delivery, notice.Delivery)
            || turns.Activity?.State != CharacterTurnActivityState.Speaking
            || notice.SeekerId != seekerSource.Character.Identity.Id || notice.WorldPosition != notice.Speaker.GlobalPosition) { return; }
        int index = roster.Participants.ToList().FindIndex(character => character.Identity.Id == notice.SpeakerId);
        Node2D? expected = index switch { 0 => GetNodeOrNull<Node2D>("../Healer"), 1 => GetNodeOrNull<Node2D>("../SageHearingPosition"), _ => null };
        if (!ReferenceEquals(expected, notice.Speaker) || !OpeningRegionPositionalAudioProjection.Admitted(
            notice.Event, transcript.Transcript, notice.SeekerId, notice.SpeakerId,
            ToNumerics(seeker.GlobalPosition), ToNumerics(notice.Speaker.GlobalPosition), notice.Range)) { return; }
        lastReply = notice.Delivery;
        schedule.Foreground(RestrainedMusicSchedule.ReplyHoldSeconds); Sync();
    }

    private void OnMotion(FootstepMotionSample sample) { schedule.ObserveMotion(sample); Sync(); }
    private void OnFinished() { schedule.Finished(); Sync(); }
    private void OnPreferencesChanged()
    {
        if (!Audible()) { schedule.Advance(0, false, false); Sync(); }
    }
    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
    private static bool BusExists() => AudioServer.GetBusIndex("Music") >= 0;
    private void RequireDiagnostic() { if (Automatic) { throw new InvalidOperationException("Live music cannot accept proof observations."); } }
    private void Report(string key, string message) { if (reported.Add(key)) { GD.PrintErr(message); } }
}
