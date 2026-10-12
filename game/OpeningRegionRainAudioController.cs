using Allrpcs.Application.Audio;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns one fixed woodland-clearing passage, anonymous caption and independent transient history.</summary>
public partial class OpeningRegionRainAudioController : Node
{
    private static readonly string[] Overlays = ["SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay"];
    private readonly RainPresentationSchedule schedule = new();
    private readonly RainCueHistory history = new();
    private OpeningRegionRainAudioSource source = null!;
    private AudioSettingsController settings = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private AudioSettingsPanel panel = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private AudioListener2D listener = null!;
    private Seeker seeker = null!;
    private AudioStreamPlayer2D player = null!;
    private AudioStreamWav? stream;
    private Node[] owners = [];
    private string[] names = [];
    private NVector? previousListener;
    private bool focused = true;
    private bool presenting;

    private bool automatic = true;
    /// <summary>Ordinary Main schedules automatically; stopped fixtures can never inject into this mode.</summary>
    [Export]
    public bool Automatic
    {
        get => automatic;
        set { if (automatic != value) { automatic = value; CancelAndRebase(); } }
    }
    /// <summary>Consumed schedule starts regardless of eligibility or output availability.</summary>
    internal long ScheduledStarts { get; private set; }
    /// <summary>Admitted presentations independent of mute, resource failure and captions preference.</summary>
    internal long PresentationCount { get; private set; }
    /// <summary>Detached read-only anonymous rain history.</summary>
    public IReadOnlyList<string> RecentCues => history.Entries;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; ProcessPhysicsPriority = 130;
        source = GetNode<OpeningRegionRainAudioSource>("../OpeningRegionRainAudioSource");
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        seeker = GetNode<Seeker>("../Seeker");
        listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener");
        owners = [source, settings, captions, panel, restoration, seeker, listener.GetParent()];
        names = owners.Select(n => n.Name.ToString()).ToArray();
        player = new()
        {
            Name = "RoadRain",
            TopLevel = true,
            Bus = "Ambience",
            MaxDistance = 256,
            Attenuation = 1,
            PanningStrength = 0.6f,
            PitchScale = 1,
            MaxPolyphony = 1,
            AreaMask = 0,
            VolumeLinear = 1
        };
        AddChild(player); player.GlobalPosition = new(1000, 350);
        LoadResource(); settings.Changed += OnSettings; restoration.PresentationReset += ResetAfterLoad;
    }

    private void LoadResource()
    {
        stream = null;
        if (ResourceLoader.Exists("res://assets/audio/rain/R01.wav")
            && GD.Load<AudioStreamWav>("res://assets/audio/rain/R01.wav") is { } candidate
            && candidate.MixRate == 48000 && candidate.Stereo && Math.Abs(candidate.GetLength() - 4) < 0.00001
            && candidate.LoopMode == AudioStreamWav.LoopModeEnum.Disabled
            && candidate.Format == AudioStreamWav.FormatEnum.Format16Bits && candidate.Data.Length == 768000)
        { stream = candidate; }
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        if (!Automatic) { return; }
        if (!Active() || !Bindings() || !RainPresentationSchedule.ValidDelta(delta)) { CancelAndRebase(); return; }
        NVector position = ToNumerics(seeker.GlobalPosition);
        if (previousListener is { } prior && NVector.Distance(position, prior) > RainSpatialProjection.TeleportPixels)
        { CancelAndRebase(); previousListener = position; return; }
        previousListener = position;
        history.Advance(delta);
        bool start = schedule.Advance(delta, false);
        if (presenting && (!schedule.InPassage || !Admitted())) { CancelCurrent(); }
        if (!Audible()) { player.Stop(); }
        if (start) { ScheduledStarts++; Present(Admitted()); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (!Active() || !Bindings() || !RainPresentationSchedule.ValidDelta(delta)) { CancelAndRebase(); return; }
        if (presenting && (!ListenerValid() || !InRange() || (Automatic && !source.ListenerSupported(ToNumerics(seeker.GlobalPosition)))))
        { CancelCurrent(); }
        if (!Audible()) { player.Stop(); }
    }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { focused = false; CancelAndRebase(); }
        if (what == NotificationApplicationFocusIn) { focused = true; }
    }

    /// <summary>Stops only this owner and starts a fresh wait; prior history and its limiter stay frozen.</summary>
    internal void CancelAndRebase() { CancelCurrent(); schedule.Reset(); previousListener = null; }
    /// <summary>Successful load/teardown additionally clears only new transient history.</summary>
    internal void ResetAfterLoad() { CancelAndRebase(); history.Clear(); }

    /// <summary>Explicit stopped-fixture clock; refuses arbitrary live injection and still consumes twenty-second waits.</summary>
    internal bool DiagnosticAdvance(double delta, bool sourceValid = true, bool listenerSupported = true)
    {
        if (Automatic) { throw new InvalidOperationException("Live rain requires its own physics schedule."); }
        if (!Active() || !Bindings() || !RainPresentationSchedule.ValidDelta(delta)) { CancelAndRebase(); return false; }
        NVector position = ToNumerics(listener.GlobalPosition);
        if (previousListener is { } prior && NVector.Distance(position, prior) > RainSpatialProjection.TeleportPixels)
        { CancelAndRebase(); previousListener = position; return false; }
        previousListener = position;
        history.Advance(delta);
        bool start = schedule.Advance(delta, false);
        if (presenting && !schedule.InPassage) { CancelCurrent(); }
        if (start)
        {
            ScheduledStarts++;
            Present(ListenerValid() && RainSpatialProjection.Admitted(ToNumerics(listener.GlobalPosition), sourceValid, listenerSupported));
        }
        return start;
    }

    /// <summary>Resource controls are available only to a stopped output fixture.</summary>
    internal void DiagnosticResources(bool available)
    {
        if (Automatic) { throw new InvalidOperationException("Live resource ownership is retained."); }
        player.Stop(); stream = null; if (available) { LoadResource(); }
    }

    private void Present(bool admitted)
    {
        if (!admitted) { schedule.CancelPassage(); return; }
        PresentationCount++; presenting = true;
        Transform2D canvas = GetViewport().GlobalCanvasTransform * GetViewport().CanvasTransform;
        Vector2 screen = canvas * new Vector2(1000, 350);
        string text = RainSpatialProjection.Caption(ToNumerics(screen), ToNumerics(GetViewport().GetVisibleRect().Size));
        captions.SetRain(text); history.Record(text);
        if (!Audible() || stream is null) { return; }
        player.Stop(); player.Stream = stream; player.GlobalPosition = new(1000, 350); player.VolumeLinear = 1; player.Play();
    }

    private bool Admitted() => ListenerValid() && source.ListenerSupported(ToNumerics(seeker.GlobalPosition))
        && RainSpatialProjection.Admitted(ToNumerics(seeker.GlobalPosition), true, true);
    private bool InRange() => RainSpatialProjection.Admitted(ToNumerics(listener.GlobalPosition), true, true);
    private bool ListenerValid() => GodotObject.IsInstanceValid(listener) && listener.IsInsideTree() && listener.CanProcess()
        && listener.IsCurrent() && listener.TopLevel && listener.GlobalRotation == 0 && listener.GlobalScale == Vector2.One
        && OpeningRegionPositionalAudioProjection.Finite(ToNumerics(seeker.GlobalPosition))
        && listener.GlobalPosition.DistanceSquaredTo(seeker.GlobalPosition) <= 1
        && ReferenceEquals(GetNodeOrNull<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener"), listener);
    private bool PlayerValid() => GodotObject.IsInstanceValid(player) && player.IsInsideTree()
        && player.GetParent() == this && GetChildCount() == 1 && player.TopLevel
        && player.GlobalPosition == new Vector2(1000, 350) && player.GlobalRotation == 0 && player.GlobalScale == Vector2.One
        && player.MaxDistance == 256 && player.Attenuation == 1 && player.PanningStrength == 0.6f
        && player.PitchScale == 1 && player.MaxPolyphony == 1 && player.AreaMask == 0 && player.VolumeLinear == 1;
    private bool Bindings() => PlayerValid() && owners.Length > 0 && owners.All(n => GodotObject.IsInstanceValid(n) && n.IsInsideTree() && n.CanProcess())
        && !owners.Where((n, i) => !ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).Any()
        && source.Valid() && seeker.IsVisibleInTree() && (!Automatic || seeker.IsPhysicsProcessing());
    private bool Active() => focused && !GetTree().Paused && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus()) && !Blocked();
    private bool Blocked() => !GodotObject.IsInstanceValid(panel) || panel.IsReading
        || GetViewport().GuiGetFocusOwner() is LineEdit or TextEdit
        || GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel") is not { Visible: false }
        || Overlays.Any(name => GetNodeOrNull<CanvasLayer>("../" + name) is not { Visible: false });
    private bool Audible() => AudioServer.GetBusIndex("Ambience") >= 0 && settings.Preferences.Channels
        .Where(c => c.Category is AudioCategory.Master or AudioCategory.Ambience).All(c => !c.Muted && c.Gain > 0);
    private void OnSettings() { if (!Audible()) { player.Stop(); } }
    private void CancelCurrent()
    {
        presenting = false; schedule.CancelPassage();
        if (GodotObject.IsInstanceValid(player)) { player.Stop(); }
        if (GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearRain(); }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(settings)) { settings.Changed -= OnSettings; }
        if (GodotObject.IsInstanceValid(restoration)) { restoration.PresentationReset -= ResetAfterLoad; }
        ResetAfterLoad(); stream = null;
    }
    private static NVector ToNumerics(Vector2 p) => new(p.X, p.Y);
}
