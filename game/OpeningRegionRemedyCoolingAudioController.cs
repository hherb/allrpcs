using Allrpcs.Application.Audio;
using Allrpcs.Application.Navigation;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns once-only local cooling feedback, independent caption and transient history.</summary>
public partial class OpeningRegionRemedyCoolingAudioController : Node
{
    private const string Resource = "res://assets/audio/remedy-cooling/C01.wav";
    private static readonly string[] Overlays = ["SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay"];
    private readonly RemedyCoolingCueHistory history = new();
    private OpeningRegionRemedyPreparationInputCoordinator coordinator = null!;
    private OpeningRegionRemedyPreparationSceneSource scene = null!;
    private FirstPatientPreparationController preparation = null!;
    private AudioSettingsController settings = null!;
    private AudioSettingsPanel panel = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private SaveSnapshotRestorationController restoration = null!;
    private Seeker seeker = null!;
    private AudioStreamPlayer player = null!;
    private AudioStreamWav? stream;
    private OpeningRegionRemedyPreparationSceneBinding? binding;
    private CoolingPreparationCommit? consumed;
    private Node[] owners = [];
    private string[] names = [];
    private Node[] children = [];
    private Transform2D[] transforms = [];
    private Texture2D?[] textures = [];
    private bool[] visibility = [];
    private Vector2? previous;
    private bool focused = true;
    private double remaining;

    /// <summary>Live Main consumes only committed notifications; stopped diagnostics cannot inject live cues.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Number of unique eligible presentations independent of output preferences.</summary>
    internal long PresentationCount { get; private set; }
    /// <summary>Detached oldest-first anonymous cooling records.</summary>
    public IReadOnlyList<string> RecentCues => history.Entries;

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        coordinator = GetNode<OpeningRegionRemedyPreparationInputCoordinator>("../OpeningRegionRemedyPreparationInputCoordinator");
        scene = GetNode<OpeningRegionRemedyPreparationSceneSource>("../OpeningRegionRemedyPreparationSceneSource");
        preparation = GetNode<FirstPatientPreparationController>("../FirstPatientPreparationController");
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        seeker = GetNode<Seeker>("../Seeker");
        owners = [coordinator, scene, preparation, settings, panel, captions, restoration, seeker];
        names = owners.Select(n => n.Name.ToString()).ToArray();
        player = new() { Name = "Cooling", Bus = "Effects", PitchScale = 1, MaxPolyphony = 1, VolumeLinear = 1 };
        AddChild(player); LoadResource();
        coordinator.CoolingCompleted += Receive;
        settings.Changed += OnSettings; restoration.PresentationReset += ResetAfterLoad;
    }

    private void LoadResource()
    {
        stream = null;
        if (ResourceLoader.Exists(Resource) && GD.Load<AudioStreamWav>(Resource) is { } candidate
            && candidate.MixRate == 48000 && candidate.Stereo && candidate.Data.Length == 144000
            && candidate.Format == AudioStreamWav.FormatEnum.Format16Bits
            && candidate.LoopMode == AudioStreamWav.LoopModeEnum.Disabled)
        { stream = candidate; }
    }

    private void CaptureBinding()
    {
        if (binding is not null || !GodotObject.IsInstanceValid(scene) || scene.Binding is null) { return; }
        binding = scene.Binding;
        children = scene.GetChildren().ToArray();
        transforms = children.Select(n => n is Node2D body ? body.Transform : Transform2D.Identity).ToArray();
        textures = children.Select(n => (n as Sprite2D)?.Texture).ToArray();
        visibility = children.Select(n => n is not CanvasItem item || item.Visible).ToArray();
    }

    /// <summary>Consumes an owned token once, including blocked or silent starts; never changes gameplay.</summary>
    internal void Receive(CoolingPreparationCommit? commit)
    {
        if (!Automatic || !GodotObject.IsInstanceValid(coordinator) || !coordinator.OwnsCooling(commit) || ReferenceEquals(consumed, commit)) { return; }
        consumed = commit; CaptureBinding();
        if (commit is null || !ReferenceEquals(binding, commit.Binding) || !Active() || !Bindings()
            || !RemedyCoolingPresentationProjection.IsCommit(binding.Recipe, commit.Step, commit.Before, commit.After)
            || !RemedyCoolingPresentationProjection.Local(ToNumerics(seeker.GlobalPosition), ToNumerics(commit.ActualPosition), binding.StationPosition))
        { return; }
        Present();
    }

    private void Present()
    {
        Cancel(); PresentationCount++; remaining = RemedyCoolingPresentationProjection.CaptionSeconds;
        previous = seeker.GlobalPosition;
        captions.SetRemedyCooling(RemedyCoolingPresentationProjection.Phrase); history.Record();
        if (Audible() && stream is not null) { player.Stream = stream; player.Play(); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        CaptureBinding();
        if (!Active() || !Bindings() || !RemedyCoolingPresentationProjection.ValidDelta(delta)
            || !RemedyCoolingPresentationProjection.Finite(ToNumerics(seeker.GlobalPosition)))
        { Cancel(); previous = null; return; }
        Vector2 position = seeker.GlobalPosition;
        if (previous is { } old && old.DistanceTo(position) > RemedyCoolingPresentationProjection.TeleportPixels)
        { Cancel(); }
        previous = position;
        if (!Audible()) { player.Stop(); }
        if (remaining <= 0) { return; }
        remaining = Math.Max(0, remaining - delta);
        if (remaining <= 0.000000001) { Cancel(); }
    }

    internal bool Bindings() => binding is not null && owners.All(n => GodotObject.IsInstanceValid(n) && n.IsInsideTree() && n.CanProcess())
        && owners.Select((n, i) => ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).All(x => x)
        && ReferenceEquals(scene.Binding, binding) && ReferenceEquals(preparation.Recipe, binding.Recipe)
        && scene.IsVisibleInTree() && seeker.IsVisibleInTree() && scene.GlobalPosition == new Vector2(2300, 1300)
        && scene.GlobalRotation == 0 && scene.GlobalScale == Vector2.One && scene.GetChildCount() == children.Length
        && children.Select((n, i) => ReferenceEquals(scene.GetChild(i), n)
            && (n is not Node2D body || body.Transform == transforms[i])
            && (n is not Sprite2D sprite || ReferenceEquals(sprite.Texture, textures[i]))
            && (n is not CanvasItem item || item.Visible == visibility[i])).All(x => x)
        && (player.Bus == "Effects" || AudioServer.GetBusIndex("Effects") < 0) && player.PitchScale == 1 && player.MaxPolyphony == 1 && player.VolumeLinear == 1;
    private bool Active() => GodotObject.IsInstanceValid(panel) && panel.IsInsideTree() && focused && !GetTree().Paused
        && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus()) && !panel.IsReading
        && GetViewport().GuiGetFocusOwner() is not LineEdit and not TextEdit
        && GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel") is { Visible: false }
        && Overlays.All(name => GetNodeOrNull<CanvasLayer>("../" + name) is { Visible: false });
    private bool Audible() => AudioServer.GetBusIndex("Effects") >= 0 && settings.Preferences.Channels
        .Where(c => c.Category is AudioCategory.Master or AudioCategory.Effects).All(c => !c.Muted && c.Gain > 0);
    private void OnSettings() { if (!Audible()) { player.Stop(); } }

    /// <summary>Cancels only current cooling feedback; consumed identity and accessible history remain.</summary>
    internal void Cancel()
    {
        remaining = 0;
        if (GodotObject.IsInstanceValid(player)) { player.Stop(); }
        if (GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearRemedyCooling(); }
    }
    /// <summary>Successful reset clears only cooling's transient presentation and history.</summary>
    internal void ResetAfterLoad() { Cancel(); consumed = null; previous = null; history.Clear(); }
    /// <summary>Stopped fixture injection is never an ordinary preparation commit.</summary>
    internal void DiagnosticPlay()
    {
        if (Automatic) { throw new InvalidOperationException("Live cooling requires a committed input."); }
        CaptureBinding(); if (Active() && Bindings()) { Present(); }
    }
    /// <summary>Only stopped output diagnostics may simulate stream loss or repair.</summary>
    internal void DiagnosticResources(bool available)
    {
        if (Automatic) { throw new InvalidOperationException("Live cooling retains its source."); }
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
        if (GodotObject.IsInstanceValid(coordinator)) { coordinator.CoolingCompleted -= Receive; }
        if (GodotObject.IsInstanceValid(settings)) { settings.Changed -= OnSettings; }
        if (GodotObject.IsInstanceValid(restoration)) { restoration.PresentationReset -= ResetAfterLoad; }
        ResetAfterLoad(); stream = null;
    }
    private static NVector ToNumerics(Vector2 point) => new(point.X, point.Y);
}
