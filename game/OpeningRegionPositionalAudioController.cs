using Allrpcs.Application.Audio;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns one spatial waterway bed and one admitted recorded-reply indication.</summary>
public partial class OpeningRegionPositionalAudioController : Node
{
    private readonly PositionalCueHistory history = new();
    private AudioListener2D listener = null!;
    private AudioStreamPlayer2D water = null!;
    private AudioStreamPlayer2D reply = null!;
    private MaterialSoundCaptionPanel captions = null!;
    private OpeningRegionProjectionSource? projection;
    private Seeker? seeker;
    private TemporaryCharacterTurnResponseSpeechRecorder? recorder;
    private CharacterTurnActivityController? turns;
    private CharacterTurnTranscriptController? transcript;
    private TemporaryOrdinaryCharacterParticipantSource? roster;
    private TemporarySeekerCharacterSource? seekerSource;
    private SaveSnapshotRestorationController? restoration;
    private RecordedReplyPresentation? active;
    private DialogueDelivery? lastPresented;
    private double captionRemaining;
    private Vector2? lastPosition;
    private float waterWeight;
    private bool waterSelected;

    /// <summary>Ordinary Main subscribes live; standalone diagnostics remain stopped.</summary>
    [Export] public bool Automatic { get; set; } = true;
    /// <summary>Detached generic cue history, without saved world state.</summary>
    public IReadOnlyList<string> RecentCues => history.Entries;

    /// <inheritdoc />
    public override void _Ready()
    {
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        listener = new() { Name = "SeekerListener", TopLevel = true, Rotation = 0, Scale = Vector2.One };
        AddChild(listener);
        listener.MakeCurrent();
        water = Player("B02", "Ambience", "res://assets/audio/material/B02.wav", 768, true);
        reply = Player("RecordedReply", "Dialogue", "res://assets/audio/positional/A06-D01.wav", 192, false);
        if (!Automatic) { listener.GlobalPosition = new(1800, 500); return; }
        projection = GetNode<OpeningRegionProjectionSource>("../OpeningRegionProjectionSource");
        seeker = GetNode<Seeker>("../Seeker");
        recorder = GetNode<TemporaryCharacterTurnResponseSpeechRecorder>("../TemporaryCharacterTurnResponseSpeechRecorder");
        turns = GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        transcript = GetNode<CharacterTurnTranscriptController>("../CharacterTurnTranscriptController");
        roster = GetNode<TemporaryOrdinaryCharacterParticipantSource>("../TemporaryOrdinaryCharacterParticipantSource");
        seekerSource = GetNode<TemporarySeekerCharacterSource>("../TemporarySeekerCharacterSource");
        restoration = GetNode<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        recorder.ReplyRecorded += ReceiveRecorded;
        restoration.PresentationReset += ResetPresentation;
        listener.GlobalPosition = seeker.GlobalPosition;
        lastPosition = seeker.GlobalPosition;
    }

    /// <inheritdoc />
    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        if (seeker is null) { return; }
        Vector2 position = seeker.GlobalPosition;
        if (!OpeningRegionPositionalAudioProjection.Finite(ToNumerics(position))) { CancelReply(); return; }
        if (lastPosition is { } previous && previous.DistanceTo(position) > OpeningRegionPositionalAudioProjection.TeleportDistance)
        { CancelReply(); }
        lastPosition = position;
        listener.GlobalPosition = position;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        history.Advance(delta);
        UpdateWater();
        if (!listener.IsCurrent()) { CancelReply(); return; }
        if (active is null) { return; }
        captionRemaining = Math.Max(0, captionRemaining - delta);
        bool invalid = !GodotObject.IsInstanceValid(active.Speaker)
            || !active.Speaker.IsInsideTree() || captionRemaining == 0
            || (turns?.Activity is { } turn && ReferenceEquals(turn.Response?.Delivery, active.Delivery)
                && ReferenceEquals(turn.State, CharacterTurnActivityState.Cancelled));
        if (invalid || !StillAdmitted(active)) { CancelReply(); return; }
        reply.GlobalPosition = active.Speaker.GlobalPosition;
        captions.SetConversation(SafeCaption("Conversation", reply.GlobalPosition));
    }

    /// <summary>Receives only the retained regional ramp; it owns no new region selection.</summary>
    internal void PresentWaterway(double weight, bool selected)
    {
        waterWeight = double.IsFinite(weight) ? (float)Math.Clamp(weight, 0, 1) : 0;
        waterSelected = selected;
        UpdateWater();
    }

    /// <summary>Handles only committed ordinary recorder notifications; failures are isolated by producer.</summary>
    internal void ReceiveRecorded(RecordedReplyPresentation notice)
    {
        if (!listener.IsCurrent() || seeker is null || seekerSource is null || roster is null || transcript is null || turns is null
            || ReferenceEquals(notice.Delivery, lastPresented)
            || !ReferenceEquals(notice.Event.Delivery, notice.Delivery)
            || !ReferenceEquals(turns.Activity?.Response?.Delivery, notice.Delivery)
            || !ReferenceEquals(turns.Activity.State, CharacterTurnActivityState.Speaking)
            || notice.SeekerId != seekerSource.Character.Identity.Id
            || notice.WorldPosition != notice.Speaker.GlobalPosition) { return; }
        int index = roster.Participants.ToList().FindIndex(character => character.Identity.Id == notice.SpeakerId);
        Node2D? expected = index switch
        {
            0 => GetNode<Node2D>("../Healer"),
            1 => GetNode<Node2D>("../SageHearingPosition"),
            _ => null
        };
        if (!ReferenceEquals(expected, notice.Speaker) || !StillAdmitted(notice)) { return; }
        lastPresented = notice.Delivery;
        active = notice;
        captionRemaining = OpeningRegionPositionalAudioProjection.CaptionSeconds;
        string text = SafeCaption("Conversation", notice.WorldPosition);
        captions.SetConversation(text);
        bool play = history.Record(text);
        if (play && reply.Stream is not null && RequiredBusExists("Dialogue"))
        {
            reply.Stop();
            reply.GlobalPosition = notice.WorldPosition;
            reply.MaxDistance = notice.Range;
            reply.VolumeLinear = 1;
            reply.Play();
        }
    }

    /// <summary>Clears transient cues/history on successful load, without modifying the recorder guard.</summary>
    internal void ResetPresentation()
    {
        CancelReply(); history.Clear(); lastPosition = seeker?.GlobalPosition;
    }

    /// <summary>Stops owned diagnostics and clears transient caption/history without replay.</summary>
    internal void StopPlayback()
    {
        ResetPresentation(); waterWeight = 0; waterSelected = false;
        water.Stop(); UpdateWater();
    }

    /// <summary>Provides controlled positions to stopped-by-default output diagnostics only.</summary>
    internal void DiagnosticWater(Vector2 listenerPosition, Vector2 sourcePosition, double weight)
    {
        if (Automatic) { throw new InvalidOperationException("Live source ownership cannot be overridden."); }
        listener.GlobalPosition = listenerPosition; water.GlobalPosition = sourcePosition;
        waterWeight = (float)weight; waterSelected = true;
        water.VolumeLinear = waterWeight;
        if (water.Stream is not null && RequiredBusExists("Ambience")) { water.Play(); }
    }

    /// <summary>Plays diagnostic output without inventing a recorded world event.</summary>
    internal void DiagnosticReply(Vector2 listenerPosition, Vector2 sourcePosition, float range)
    {
        if (Automatic) { throw new InvalidOperationException("Live reply ownership cannot be overridden."); }
        listener.GlobalPosition = listenerPosition; reply.GlobalPosition = sourcePosition;
        reply.MaxDistance = range; reply.VolumeLinear = 1;
        if (reply.Stream is not null && RequiredBusExists("Dialogue")) { reply.Play(); }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (recorder is not null) { recorder.ReplyRecorded -= ReceiveRecorded; }
        if (restoration is not null) { restoration.PresentationReset -= ResetPresentation; }
        if (reply is not null) { CancelReply(); water.Stop(); }
        if (listener is not null && listener.IsCurrent()) { listener.ClearCurrent(); }
        history.Clear();
    }

    private void UpdateWater()
    {
        if (water is null) { return; }
        if (!listener.IsCurrent() || !RequiredBusExists("Ambience"))
        { water.VolumeLinear = 0; if (waterSelected) { captions.SetRegion(string.Empty); } return; }
        if (Automatic)
        {
            if (projection is null || seeker is null) { water.VolumeLinear = 0; return; }
            Vector2 local = projection.ToLocal(seeker.GlobalPosition);
            NumericsVector2? point = OpeningRegionPositionalAudioProjection.NearestWaterPoint(ToNumerics(local),
                OpeningRegionStreamLaundryGeometry.StreamWater);
            if (point is null) { water.VolumeLinear = 0; if (waterSelected) { captions.SetRegion(string.Empty); } return; }
            water.GlobalPosition = projection.ToGlobal(new(point.Value.X, point.Value.Y));
        }
        water.VolumeLinear = waterWeight;
        if (waterWeight > 0 && !water.Playing && water.Stream is not null && RequiredBusExists("Ambience")) { water.Play(); }
        if (waterSelected)
        {
            bool eligible = waterWeight > 0 && OpeningRegionPositionalAudioProjection.DistanceGain(
                ToNumerics(water.GlobalPosition), ToNumerics(listener.GlobalPosition), 768) > 0;
            captions.SetRegion(eligible ? SafeCaption("Running water and rain", water.GlobalPosition) : string.Empty);
        }
    }

    private bool StillAdmitted(RecordedReplyPresentation notice) => seeker is not null
        && GodotObject.IsInstanceValid(notice.Speaker)
        && OpeningRegionPositionalAudioProjection.Admitted(notice.Event, transcript?.Transcript,
            notice.SeekerId, notice.SpeakerId, ToNumerics(seeker.GlobalPosition),
            ToNumerics(notice.Speaker.GlobalPosition), notice.Range);

    private string SafeCaption(string prefix, Vector2 worldPosition)
    {
        Transform2D canvas = GetViewport().GlobalCanvasTransform * GetViewport().CanvasTransform;
        Vector2 screen = canvas * worldPosition;
        return OpeningRegionPositionalAudioProjection.Caption(prefix, ToNumerics(screen), ToNumerics(GetViewport().GetVisibleRect().Size));
    }

    private void CancelReply() { reply.Stop(); active = null; captionRemaining = 0; captions.SetConversation(string.Empty); }
    private static NumericsVector2 ToNumerics(Vector2 value) => new(value.X, value.Y);
    private static bool RequiredBusExists(string bus) => AudioServer.GetBusIndex(bus) >= 0;

    private AudioStreamPlayer2D Player(string name, string bus, string path, float range, bool loop)
    {
        AudioStreamPlayer2D player = new()
        {
            Name = name,
            Bus = bus,
            MaxDistance = range,
            Attenuation = 1,
            PanningStrength = 0.6f,
            PitchScale = 1,
            MaxPolyphony = 1,
            AreaMask = 0,
            VolumeLinear = 0
        };
        AddChild(player);
        if (ResourceLoader.Exists(path) && GD.Load<AudioStreamWav>(path) is { } original)
        {
            AudioStreamWav stream = (AudioStreamWav)original.Duplicate();
            if (loop)
            {
                stream.LoopMode = AudioStreamWav.LoopModeEnum.Forward;
                stream.LoopBegin = 0; stream.LoopEnd = (int)Math.Round(stream.GetLength() * stream.MixRate);
            }
            player.Stream = stream;
        }
        else { GD.PrintErr($"Missing positional resource {name}; text/gameplay remain available."); }
        return player;
    }
}
