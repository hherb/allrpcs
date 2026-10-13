using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns once-only local rustle feedback, independent caption and transient history.</summary>
public partial class OpeningRegionHerbCollectionAudioController : Node
{
    private const string Resource = "res://assets/audio/herb-collection/HC01.wav";
    private static readonly string[] Overlays = ["SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay"];
    private readonly HerbCollectionCueHistory history = new();
    private TemporaryCharacterExaminationFlowController coordinator = null!;
    private OpeningRegionHerbCollectionAudioSource scene = null!;
    private AudioSettingsController settings = null!;
    private AudioSettingsPanel panel = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private Seeker seeker = null!;
    private AudioStreamPlayer2D player = null!;
    private Healer healer = null!;
    private AudioListener2D listener = null!;
    private AudioStreamWav? stream;
    private bool bound;
    private HerbCollectionCommit? consumed;
    private Node[] owners = [];
    private string[] names = [];
    private Vector2? previous;
    private bool focused = true;
    private double remaining;
    private double soundRemaining;

    /// <summary>Live Main consumes only committed notifications; stopped diagnostics cannot inject live cues.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Number of unique eligible presentations independent of output preferences.</summary>
    internal long PresentationCount { get; private set; }
    /// <summary>Detached oldest-first anonymous rustle records.</summary>
    public IReadOnlyList<string> RecentCues => history.Entries;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        coordinator = GetNode<TemporaryCharacterExaminationFlowController>("../TemporaryCharacterExaminationFlowController");
        scene = GetNode<OpeningRegionHerbCollectionAudioSource>("../OpeningRegionHerbCollectionAudioSource");
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        seeker = GetNode<Seeker>("../Seeker");
        healer = GetNode<Healer>("../Healer");
        listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener");
        owners = [coordinator, scene, settings, panel, captions, restoration, seeker, healer];
        names = owners.Select(n => n.Name.ToString()).ToArray();
        player = new()
        {
            Name = "Rustle",
            Bus = "Effects",
            PitchScale = 1,
            MaxPolyphony = 1,
            VolumeLinear = 1,
            MaxDistance = 192,
            Attenuation = 1,
            PanningStrength = 0.6f,
            AreaMask = 0
        };
        AddChild(player); LoadResource();
        consumed = coordinator.LastCollectionCommit;
        coordinator.CollectionCommitted += Receive;
        settings.Changed += OnSettings; restoration.PresentationReset += ResetAfterLoad;
    }

    private void LoadResource()
    {
        stream = null;
        if (ResourceLoader.Exists(Resource) && GD.Load<AudioStreamWav>(Resource) is { } candidate
            && candidate.MixRate == 48000 && candidate.Stereo && candidate.Data.Length == 192000
            && candidate.Format == AudioStreamWav.FormatEnum.Format16Bits
            && candidate.LoopMode == AudioStreamWav.LoopModeEnum.Disabled)
        { stream = candidate; }
    }

    private void CaptureBinding() { bound = GodotObject.IsInstanceValid(scene) && scene.IsInsideTree() && scene.CanProcess() && scene.Valid(); }

    /// <summary>Consumes an owned token once, including blocked or silent starts; never changes gameplay.</summary>
    internal void Receive(HerbCollectionCommit? commit)
    {
        if (!GodotObject.IsInstanceValid(coordinator) || !coordinator.OwnsCollection(commit) || ReferenceEquals(consumed, commit)) { return; }
        consumed = commit;
        if (!Automatic) { return; }
        CaptureBinding();
        var events = GetNode<TemporaryWorldEventSource>("../TemporaryWorldEventSource");
        if (commit is null || !Active() || !Bindings()
            || !HerbCollectionPresentationProjection.IsCommit(commit.Result, commit.Occurrence, scene.ActorId,
                scene.TargetId, events.HerbPatchExaminedWorldEvent, events.HerbPatchCollectedWorldEvent)
            || !Local()) { return; }
        Present();
    }

    private void Present()
    {
        Cancel(); PresentationCount++; remaining = HerbCollectionPresentationProjection.CaptionSeconds;
        soundRemaining = HerbCollectionPresentationProjection.PassageSeconds;
        previous = seeker.GlobalPosition; player.GlobalPosition = scene.PatchPosition;
        captions.SetHerbCollection(HerbCollectionPresentationProjection.Phrase); history.Record();
        if (Audible() && stream is not null) { player.Stream = stream; player.Play(); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        CaptureBinding();
        if (!Active() || !Bindings() || !Local() || !HerbCollectionPresentationProjection.ValidDelta(delta)
            || !HerbCollectionPresentationProjection.Finite(ToNumerics(seeker.GlobalPosition)))
        { Cancel(); previous = null; return; }
        if (remaining > 0 && Automatic && !coordinator.OwnsCollection(consumed)) { Cancel(); return; }
        Vector2 position = seeker.GlobalPosition;
        if (previous is { } old && old.DistanceTo(position) > HerbCollectionPresentationProjection.TeleportPixels)
        { Cancel(); }
        previous = position;
        soundRemaining = Math.Max(0, soundRemaining - delta);
        if (!Audible() || soundRemaining <= 0.000000001) { player.Stop(); }
        if (remaining <= 0) { return; }
        remaining = Math.Max(0, remaining - delta);
        if (remaining <= 0.000000001) { Cancel(); }
    }

    internal bool Bindings() => bound && owners.All(n => GodotObject.IsInstanceValid(n) && n.IsInsideTree() && n.CanProcess())
        && owners.Select((n, i) => ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).All(x => x)
        && scene.Valid() && ListenerValid()
        && (player.Bus == "Effects" || AudioServer.GetBusIndex("Effects") < 0)
        && player.PitchScale == 1 && player.MaxPolyphony == 1 && player.VolumeLinear == 1
        && (remaining <= 0 || player.GlobalPosition == scene.PatchPosition)
        && player.MaxDistance == 192 && player.Attenuation == 1 && player.PanningStrength == 0.6f && player.AreaMask == 0;
    private bool ListenerValid() => GodotObject.IsInstanceValid(listener) && listener.IsInsideTree() && listener.CanProcess()
        && listener.IsCurrent() && listener.TopLevel && listener.GlobalRotation == 0 && listener.GlobalScale == Vector2.One
        && listener.GlobalPosition.DistanceSquaredTo(seeker.GlobalPosition) <= 1
        && ReferenceEquals(GetNodeOrNull<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener"), listener);
    private bool Local() => scene.Exterior(healer.GlobalPosition) && scene.Exterior(seeker.GlobalPosition)
        && HerbCollectionPresentationProjection.Local(ToNumerics(healer.GlobalPosition),
            ToNumerics(seeker.GlobalPosition), ToNumerics(scene.PatchPosition));
    private bool Active() => GodotObject.IsInstanceValid(panel) && panel.IsInsideTree() && focused && !GetTree().Paused
        && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus()) && !panel.IsReading
        && GetViewport().GuiGetFocusOwner() is not LineEdit and not TextEdit
        && GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel") is { Visible: false }
        && Overlays.All(name => GetNodeOrNull<CanvasLayer>("../" + name) is { Visible: false });
    private bool Audible() => AudioServer.GetBusIndex("Effects") >= 0 && settings.Preferences.Channels
        .Where(c => c.Category is AudioCategory.Master or AudioCategory.Effects).All(c => !c.Muted && c.Gain > 0);
    private void OnSettings() { if (!Audible()) { player.Stop(); } }

    /// <summary>Cancels only current rustle feedback; consumed identity and accessible history remain.</summary>
    internal void Cancel()
    {
        remaining = 0; soundRemaining = 0;
        if (GodotObject.IsInstanceValid(player)) { player.Stop(); }
        if (GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearHerbCollection(); }
    }
    /// <summary>Successful reset clears only rustle's transient presentation and history.</summary>
    internal void ResetAfterLoad() { Cancel(); consumed = coordinator.LastCollectionCommit; previous = null; history.Clear(); }
    /// <summary>Stopped fixture injection is never an ordinary preparation commit.</summary>
    internal void DiagnosticPlay()
    {
        if (Automatic) { throw new InvalidOperationException("Live rustle requires a committed input."); }
        CaptureBinding(); if (Active() && Bindings() && Local()) { Present(); }
    }
    /// <summary>Only stopped output diagnostics may simulate stream loss or repair.</summary>
    internal void DiagnosticResources(bool available)
    {
        if (Automatic) { throw new InvalidOperationException("Live rustle retains its source."); }
        stream = null; if (available) { LoadResource(); }
    }
    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { focused = false; Cancel(); }
        if (what == NotificationApplicationFocusIn) { focused = true; }
    }
    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(coordinator)) { coordinator.CollectionCommitted -= Receive; }
        if (GodotObject.IsInstanceValid(settings)) { settings.Changed -= OnSettings; }
        if (GodotObject.IsInstanceValid(restoration)) { restoration.PresentationReset -= ResetAfterLoad; }
        ResetAfterLoad(); stream = null;
    }
    private static NVector ToNumerics(Vector2 point) => new(point.X, point.Y);
}
