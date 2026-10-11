using Allrpcs.Application.Audio;
using Allrpcs.Domain.Characters;
using Godot;
using NVector = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns exact Healer-only exterior contacts, fixed spatial tails and anonymous transient cues.</summary>
public partial class OpeningRegionHealerFootstepAudioController : Node
{
    private static readonly string[] Overlays = ["SeekerMapPanel", "JournalSectionPanel", "DialogueOverlay", "SettingsOverlay"];
    private readonly FootstepContactCadence cadence = new();
    private readonly HealerFootstepCueHistory history = new();
    private readonly Dictionary<string, AudioStreamWav> streams = [];
    private readonly List<AudioStreamPlayer2D> players = [];
    private readonly ulong[] ages = new ulong[2];
    private readonly FootstepContact?[] tails = new FootstepContact?[2];
    private AudioSettingsController settings = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private OpeningRegionHealerFootstepMaterialSource? materials;
    private Healer? healer;
    private Seeker? seeker;
    private TemporaryHealerCharacterSource? actor;
    private TemporarySeekerCharacterSource? human;
    private TemporaryOrdinaryCharacterParticipantSource? roster;
    private SaveSnapshotRestorationController? restoration;
    private AudioSettingsPanel? panel;
    private AudioListener2D? listener;
    private Character? retainedActor;
    private Character? retainedHuman;
    private Node? bodyParent;
    private float bodyRotation;
    private Vector2 bodyScale;
    private Node?[] owners = [];
    private string[] names = [];
    private FootstepContact? pending;
    private ulong pendingFrame;
    private long consumedTick = long.MinValue;
    private FootstepContact? captionContact;
    private double remaining;
    private NVector? previousListener;
    private bool focused = true;

    /// <summary>Only ordinary Main observes body motion; proof scenes start without any live producer.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Independent modeled contact count regardless of output/listener range.</summary>
    internal long ContactCount { get; private set; }
    /// <summary>Spatially admitted contact presentations regardless of mute or resource failure.</summary>
    internal long PresentationCount { get; private set; }
    /// <summary>Detached anonymous accessible cue history.</summary>
    public IReadOnlyList<string> RecentCues => history.Entries;
    /// <summary>Last modeled contact for controlled proof assertions, never world state.</summary>
    internal FootstepContact? LastContact { get; private set; }

    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always; ProcessPhysicsPriority = 100;
        settings = GetNode<AudioSettingsController>("../AudioSettingsController");
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        for (int i = 0; i < 2; i++)
        {
            AudioStreamPlayer2D player = new()
            {
                Name = "HealerContact" + i,
                TopLevel = true,
                Bus = "Effects",
                MaxDistance = HealerFootstepSpatialProjection.RangePixels,
                Attenuation = 1,
                PanningStrength = 0.6f,
                PitchScale = 1,
                MaxPolyphony = 1,
                AreaMask = 0,
                VolumeLinear = 1
            };
            AddChild(player); players.Add(player);
        }
        LoadResources();
        settings.Changed += OnSettings;
        if (!Automatic) { return; }
        BindLive();
    }

    private void LoadResources()
    {
        streams.Clear();
        foreach (FootstepMaterial material in Enum.GetValues<FootstepMaterial>())
        {
            for (int variant = 1; variant <= 6; variant++)
            {
                string id = $"{OpeningRegionFootstepMaterialProjection.ClipId(material)}-{variant:00}";
                if (GD.Load<AudioStreamWav>($"res://assets/audio/footsteps/{id}.wav") is { } stream
                    && stream.MixRate == 48000 && stream.Stereo && Math.Abs(stream.GetLength() - 0.5) < 0.001
                    && stream.LoopMode == AudioStreamWav.LoopModeEnum.Disabled) { streams.Add(id, stream); }
            }
        }
    }

    private void BindLive()
    {
        healer = GetNode<Healer>("../Healer"); seeker = GetNode<Seeker>("../Seeker");
        actor = GetNode<TemporaryHealerCharacterSource>("../TemporaryHealerCharacterSource");
        human = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        roster = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource");
        materials = GetNode<OpeningRegionHealerFootstepMaterialSource>("../OpeningRegionHealerFootstepMaterialSource");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        panel = GetNode<AudioSettingsPanel>("../AudioSettingsPanel");
        listener = GetNode<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener");
        retainedActor = actor.Character; retainedHuman = human.Character;
        bodyParent = healer.GetParent(); bodyRotation = healer.Rotation; bodyScale = healer.Scale;
        owners = [healer, seeker, actor, human, roster, materials, restoration, panel, settings, listener.GetParent()];
        names = owners.Select(n => n!.Name.ToString()).ToArray();
        healer.MotionCompleted += OnMotion; restoration.PresentationReset += ResetAfterLoad;
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        if (!Automatic) { return; }
        if (!Active() || !Bindings() || !HealerFootstepSpatialProjection.ValidDelta(delta)) { CancelMotion(); return; }
        NVector position = ToNumerics(seeker!.GlobalPosition);
        if (previousListener is { } previous && NVector.Distance(position, previous) > 96) { CancelMotion(); }
        previousListener = position;
        if (!ListenerValid()) { pending = null; ClearCurrent(); StopTails(); return; }
        history.Advance(delta);
        remaining = Math.Max(0, remaining - delta);
        if (remaining == 0 || (captionContact is { } last && !Admitted(last))) { ClearCurrent(); }
        for (int i = 0; i < players.Count; i++)
        {
            if (tails[i] is { } tail && (!Admitted(tail) || !Audible())) { players[i].Stop(); tails[i] = null; }
        }
        FootstepContact? contact = pending; pending = null;
        if (contact is not null && pendingFrame == Engine.GetPhysicsFrames()) { Present(contact); }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (Automatic && (!Active() || !Bindings() || !HealerFootstepSpatialProjection.ValidDelta(delta))) { CancelMotion(); }
    }

    /// <inheritdoc />
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { focused = false; CancelMotion(); }
        if (what == NotificationApplicationFocusIn) { focused = true; }
    }

    /// <summary>Discards only new cadence/current cues; retained anonymous history does not replay.</summary>
    internal void CancelMotion()
    {
        cadence.Reset(); pending = null; LastContact = null; previousListener = null;
        StopTails(); ClearCurrent();
    }
    /// <summary>Successful load resets only new presentation and history.</summary>
    internal void ResetAfterLoad() { CancelMotion(); history.Clear(); }

    /// <summary>Explicit stopped-fixture playback; cannot inject a contact into the ordinary live owner.</summary>
    internal void DiagnosticContact(FootstepContact contact, AudioListener2D proofListener, bool sourceSupported = true, bool listenerSupported = true)
    {
        if (Automatic) { throw new InvalidOperationException("Live contacts require the exact producer."); }
        listener = proofListener;
        if (!listener.IsCurrent() || !HealerFootstepSpatialProjection.Admitted(contact,
            ToNumerics(listener.GlobalPosition), sourceSupported, listenerSupported)) { return; }
        Present(contact);
    }
    /// <summary>Resource-failure fixture; movement and cue admission continue unchanged.</summary>
    internal void RemoveDiagnosticResources() { streams.Clear(); StopTails(); }

    /// <summary>Reloads only the unchanged installed streams in a stopped output fixture.</summary>
    internal void RestoreDiagnosticResources()
    {
        if (Automatic) { throw new InvalidOperationException("Live resources are not diagnostic-owned."); }
        LoadResources();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (healer is not null && GodotObject.IsInstanceValid(healer)) { healer.MotionCompleted -= OnMotion; }
        if (restoration is not null && GodotObject.IsInstanceValid(restoration)) { restoration.PresentationReset -= ResetAfterLoad; }
        if (GodotObject.IsInstanceValid(settings)) { settings.Changed -= OnSettings; }
        ResetAfterLoad(); streams.Clear();
    }

    private void OnMotion(FootstepMotionSample sample)
    {
        if (healer is null || !ReferenceEquals(healer.LastMotion, sample) || sample.Tick <= consumedTick) { return; }
        consumedTick = sample.Tick;
        try
        {
            if (!Active() || !Bindings() || materials is null) { CancelMotion(); return; }
            FootstepCadenceResult result = cadence.Advance(sample, materials.Select);
            if (result.Cancelled) { pending = null; StopTails(); ClearCurrent(); LastContact = null; }
            if (result.Contact is { } contact)
            {
                ContactCount++; LastContact = contact; pending = contact; pendingFrame = Engine.GetPhysicsFrames();
            }
        }
        catch (Exception error) { CancelMotion(); GD.PrintErr("Healer footstep observer: " + error.GetType().Name); }
    }
    private void Present(FootstepContact contact)
    {
        if (Automatic && (!Active() || !Bindings() || !Admitted(contact))) { return; }
        if (!Automatic && !HealerFootstepSpatialProjection.Admitted(contact, ToNumerics(listener!.GlobalPosition), true, true)) { return; }
        PresentationCount++; captionContact = contact; remaining = HealerFootstepSpatialProjection.CaptionSeconds;
        Transform2D canvas = GetViewport().GlobalCanvasTransform * GetViewport().CanvasTransform;
        Vector2 screen = canvas * new Vector2(contact.Position.X, contact.Position.Y);
        string text = HealerFootstepSpatialProjection.Caption(contact, ToNumerics(screen), ToNumerics(GetViewport().GetVisibleRect().Size));
        captions.SetHealerFootstep(text); history.Record(text);
        string id = $"{OpeningRegionFootstepMaterialProjection.ClipId(contact.Material)}-{contact.Variant:00}";
        if (!Audible() || !streams.TryGetValue(id, out AudioStreamWav? stream)) { return; }
        int slot = players.FindIndex(p => !p.Playing);
        if (slot < 0) { slot = ages[0] <= ages[1] ? 0 : 1; }
        AudioStreamPlayer2D player = players[slot]; player.Stop(); player.Stream = stream;
        player.GlobalPosition = new(contact.Position.X, contact.Position.Y); player.VolumeLinear = 1; player.Play();
        ages[slot] = Engine.GetPhysicsFrames(); tails[slot] = contact;
    }
    private bool Admitted(FootstepContact contact) => ListenerValid() && materials is not null
        && HealerFootstepSpatialProjection.Admitted(contact, ToNumerics(seeker!.GlobalPosition),
            materials.Select(contact.Position) == contact.Material, materials.Select(ToNumerics(seeker.GlobalPosition)) is not null);
    private bool ListenerValid() => listener is not null && GodotObject.IsInstanceValid(listener) && listener.IsInsideTree()
        && listener.CanProcess() && listener.IsCurrent() && listener.TopLevel && listener.GlobalRotation == 0
        && listener.GlobalScale == Vector2.One && seeker is not null && Finite(seeker.GlobalPosition)
        && listener.GlobalPosition.DistanceSquaredTo(seeker.GlobalPosition) <= 1
        && ReferenceEquals(GetNodeOrNull<AudioListener2D>("../OpeningRegionPositionalAudioController/SeekerListener"), listener);
    private bool Bindings() => owners.Length > 0 && owners.All(n => n is not null && GodotObject.IsInstanceValid(n)
        && n.IsInsideTree() && n.CanProcess()) && !owners.Where((n, i) => !ReferenceEquals(GetNodeOrNull<Node>("../" + names[i]), n)).Any()
        && healer!.GetParent() == bodyParent && healer.Rotation == bodyRotation && healer.Scale == bodyScale
        && healer.IsVisibleInTree() && healer.IsPhysicsProcessing() && seeker!.IsVisibleInTree() && seeker.IsPhysicsProcessing()
        && ReferenceEquals(actor!.Character, retainedActor) && ReferenceEquals(human!.Character, retainedHuman)
        && ReferenceEquals(roster!.Participants[0], retainedActor) && materials!.Valid();
    private bool Active() => Automatic && focused && !GetTree().Paused && (DisplayServer.GetName() == "headless" || GetWindow().HasFocus())
        && !Blocked();
    private bool Blocked()
    {
        if (panel is null || !GodotObject.IsInstanceValid(panel) || panel.IsReading || GetViewport().GuiGetFocusOwner() is LineEdit or TextEdit
            || GetNodeOrNull<Control>("../SeekerCustomizationOverlay/Panel") is not { Visible: false }) { return true; }
        return Overlays
            .Any(name => GetNodeOrNull<CanvasLayer>("../" + name) is not { Visible: false });
    }
    private bool Audible() => AudioServer.GetBusIndex("Effects") >= 0 && OpeningRegionMaterialAudioProjection.EffectsAudible(settings.Preferences);
    private void OnSettings() { if (!Audible()) { StopTails(); } }
    private void StopTails() { foreach (AudioStreamPlayer2D player in players) { player.Stop(); } Array.Clear(tails); }
    private void ClearCurrent()
    {
        remaining = 0; captionContact = null;
        if (GodotObject.IsInstanceValid(captions) && captions.IsInsideTree()) { captions.ClearHealerFootstep(); }
    }
    private static NVector ToNumerics(Vector2 p) => new(p.X, p.Y);
    private static bool Finite(Vector2 p) => float.IsFinite(p.X) && float.IsFinite(p.Y);
}
