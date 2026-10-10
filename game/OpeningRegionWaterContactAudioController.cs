using Allrpcs.Domain.Actions;
using Godot;

namespace Allrpcs.Game;

/// <summary>Once-only local Effects feedback for the exact new live contact owner.</summary>
public partial class OpeningRegionWaterContactAudioController : Node
{
    private OpeningRegionStreamWaterContactCoordinator? coordinator;
    private MaterialSoundCaptionPanel captions = null!;
    private AudioStreamPlayer player = null!;
    private StreamWaterContactCommit? presented;
    private double remaining;
    /// <summary>Ordinary Main subscribes; standalone output diagnostics stay stopped.</summary>
    [Export] public bool Automatic { get; set; }
    /// <summary>Counts fresh live notifications, including silent/missing-resource presentations.</summary>
    internal long PresentationCount { get; private set; }
    /// <inheritdoc />
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        captions = GetNode<MaterialSoundCaptionPanel>("../MaterialSoundCaptionPanel");
        player = new() { Name = "WaterContact", Bus = "Effects", VolumeLinear = 1 }; AddChild(player);
        string path = "res://assets/audio/water-contact/A01-W01.wav";
        if (ResourceLoader.Exists(path) && GD.Load<AudioStreamWav>(path) is { } source)
        {
            AudioStreamWav copy = (AudioStreamWav)source.Duplicate();
            copy.LoopMode = AudioStreamWav.LoopModeEnum.Disabled; player.Stream = copy;
        }
        if (!Automatic) { return; }
        coordinator = GetNode<OpeningRegionStreamWaterContactCoordinator>("../OpeningRegionStreamWaterContactCoordinator");
        coordinator.Committed += Receive; coordinator.PresentationCancelled += Cancel;
    }
    /// <summary>Rejects copied/forged/repeated commits before touching captions or playback.</summary>
    internal void Receive(StreamWaterContactCommit commit)
    {
        if (!Automatic || coordinator is null || !coordinator.Owns(commit) || ReferenceEquals(presented, commit)) { return; }
        presented = commit; PresentationCount++; Present();
    }
    private void Present()
    {
        remaining = 1.5; captions.SetWaterContact("Water contact");
        player.Stop();
        if (player.Stream is not null && AudioServer.GetBusIndex("Effects") >= 0) { player.Play(); }
    }
    /// <summary>Clears only new feedback; the consumed commit remains consumed after cancellation.</summary>
    internal void Cancel() { player.Stop(); remaining = 0; captions.ClearWaterContact(); }
    /// <inheritdoc />
    public override void _Process(double delta)
    {
        if (remaining <= 0) { return; }
        if (!double.IsFinite(delta) || delta < 0 || delta > 0.25) { Cancel(); return; }
        remaining = Math.Max(0, remaining - delta);
        if (remaining == 0) { Cancel(); }
    }
    /// <summary>Explicit stopped-lab playback; it never produces or claims a gameplay commit.</summary>
    internal void DiagnosticPlay()
    { if (Automatic) { throw new InvalidOperationException("Live action cannot use diagnostic playback."); } Present(); }
    /// <summary>Removes only this stopped diagnostic source for output-missing checks.</summary>
    internal void DiagnosticMissing(bool missing)
    {
        if (Automatic) { throw new InvalidOperationException("Live source cannot be overridden."); }
        player.Stream = missing ? null : GD.Load<AudioStreamWav>("res://assets/audio/water-contact/A01-W01.wav");
    }
    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (coordinator is not null && GodotObject.IsInstanceValid(coordinator))
        { coordinator.Committed -= Receive; coordinator.PresentationCancelled -= Cancel; }
        Cancel();
    }
}
