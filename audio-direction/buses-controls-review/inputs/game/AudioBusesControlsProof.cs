using System.Text.Json;
using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Stopped-by-default routing laboratory and deterministic offline output measurement scene.</summary>
public partial class AudioBusesControlsProof : Node
{
    private const int FramesPerStep = 60;
    private static readonly JsonSerializerOptions TimelineOptions = new() { WriteIndented = true };
    private readonly List<(string Name, AudioCategory Category, double Amplitude, bool LeftOnly)> steps = [];
    private AudioSettingsController controller = null!;
    private AudioSettingsPanel panel = null!;
    private AudioStreamPlayer player = null!;
    private Label status = null!;
    private OptionButton category = null!;
    private long speech;
    private long cue;
    private bool automated;
    private int frame;

    /// <inheritdoc />
    public override void _Ready()
    {
        controller = GetNode<AudioSettingsController>("AudioSettingsController");
        panel = GetNode<AudioSettingsPanel>("AudioSettingsPanel");
        player = new AudioStreamPlayer { Name = "DiagnosticPlayer" };
        AddChild(player);
        BuildInterface();
        foreach (AudioCategory bus in Enum.GetValues<AudioCategory>().Where(value => value != AudioCategory.Master))
        {
            foreach (string variant in new[] { "normal", "half", "zero", "mute" })
            {
                steps.Add(($"{bus}-{variant}", bus, 0.01, false));
            }
        }
        foreach (string variant in new[] { "master-zero", "master-mute", "stereo-left", "mono-left", "stereo-restored" })
        {
            steps.Add((variant, AudioCategory.Effects, 0.01, variant.Contains("left", StringComparison.Ordinal)
                || variant == "stereo-restored"));
        }
        foreach (string variant in new[] { "default-low", "default-high", "reduced-low", "reduced-high" })
        {
            steps.Add((variant, AudioCategory.Effects, variant.EndsWith("high", StringComparison.Ordinal) ? 0.12 : 0.005, false));
        }
        foreach (string variant in new[] { "duck-normal", "duck-speech", "duck-overlap", "duck-cancel-release", "duck-muted",
            "reading", "reading-release", "optional-effects", "required-effects" })
        {
            steps.Add((variant, variant.Contains("effects", StringComparison.Ordinal) ? AudioCategory.Effects : AudioCategory.Music,
                0.01, false));
        }
        automated = OS.GetEnvironment("ALLRPCS_AUDIO_CHECK") == "1";
        if (automated)
        {
            Require(OS.HasFeature("movie"), "Output checks require offline Movie Maker mode.");
            VerifyPersistenceAndInterface();
        }
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        _ = delta;
        if (!automated)
        {
            if (player.Playing && player.Bus == new StringName("Effects"))
            {
                player.VolumeDb = (float)(-controller.Ducking.ReductionDb(AudioCategory.Effects));
            }
            return;
        }
        if (frame % FramesPerStep == 0)
        {
            int index = frame / FramesPerStep;
            if (index == steps.Count)
            {
                Stop();
                string output = OS.GetEnvironment("ALLRPCS_AUDIO_TIMELINE");
                Require(!string.IsNullOrEmpty(output), "A disposable timeline path is required.");
                File.WriteAllText(output, JsonSerializer.Serialize(steps.Select((step, number) => new
                {
                    name = step.Name,
                    category = step.Category.ToString(),
                    start = number,
                    duration = 1,
                    amplitude = step.Amplitude,
                    leftOnly = step.LeftOnly,
                }), TimelineOptions));
                GD.Print("Audio output proof passed configuration, persistence and accessible control checks; analyze captured WAV.");
                GetTree().Quit();
                return;
            }
            BeginStep(steps[index]);
        }
        string current = steps[frame / FramesPerStep].Name;
        if (current == "optional-effects")
        {
            player.VolumeDb = (float)-controller.Ducking.ReductionDb(AudioCategory.Effects);
        }
        frame++;
    }

    /// <inheritdoc />
    public override void _ExitTree() => Stop();

    private void BeginStep((string Name, AudioCategory Category, double Amplitude, bool LeftOnly) step)
    {
        Stop();
        controller.RestoreDefaults();
        controller.Ducking.Clear();
        AudioPreferences preferences = controller.Preferences;
        if (step.Name.EndsWith("-half", StringComparison.Ordinal))
        {
            preferences = preferences.WithChannel(step.Category, 0.5, false);
        }
        if (step.Name.EndsWith("-zero", StringComparison.Ordinal) || step.Name.EndsWith("-mute", StringComparison.Ordinal))
        {
            AudioCategory target = step.Name.StartsWith("master", StringComparison.Ordinal) ? AudioCategory.Master : step.Category;
            preferences = preferences.WithChannel(target, step.Name.EndsWith("-zero", StringComparison.Ordinal) ? 0 : 1,
                step.Name.EndsWith("-mute", StringComparison.Ordinal));
        }
        preferences = new AudioPreferences(preferences.Channels,
            step.Name.StartsWith("reduced", StringComparison.Ordinal) ? AudioMixPreset.ReducedRange : AudioMixPreset.Default,
            step.Name == "mono-left", preferences.Captions);
        controller.SetPreferences(preferences);
        if (step.Name.StartsWith("duck", StringComparison.Ordinal) && step.Name != "duck-normal"
            || step.Name.Contains("effects", StringComparison.Ordinal))
        {
            speech = controller.Ducking.Begin(AudioForeground.Speech);
        }
        if (step.Name == "duck-overlap")
        {
            cue = controller.Ducking.Begin(AudioForeground.ConsequentialCue);
        }
        if (step.Name.StartsWith("reading", StringComparison.Ordinal))
        {
            speech = controller.Ducking.Begin(AudioForeground.Reading);
        }
        if (step.Name.EndsWith("release", StringComparison.Ordinal))
        {
            controller.Ducking.Advance(0.1);
            controller.Ducking.End(speech);
        }
        if (step.Name == "duck-muted")
        {
            controller.SetPreferences(preferences.WithChannel(AudioCategory.Music, 0.4, true));
        }
        Require(AudioServer.BusCount == 6, "Expected exactly six independent buses.");
        for (int index = 1; index < AudioServer.BusCount; index++)
        {
            Require(AudioServer.GetBusSend(index) == new StringName("Master"), "A category acquired a nested route.");
        }
        Require(Math.Abs(AudioServer.GetBusVolumeLinear((int)AudioCategory.Speech) - 1) < 0.0001
            || step.Category == AudioCategory.Speech, "Foreground ducked the Speech bus.");
        Start(step.Category, step.Amplitude, step.LeftOnly);
        status.Text = step.Name;
    }

    private void VerifyPersistenceAndInterface()
    {
        AudioPreferences changed = new(AudioPreferences.Defaults().WithChannel(AudioCategory.Music, 0.37, true).Channels,
            AudioMixPreset.ReducedRange, true, false);
        controller.SetPreferences(changed);
        Require(controller.Save(), "Preferences did not persist.");
        controller.RestoreDefaults();
        controller.Reload();
        Require(controller.Preferences.Mono && !controller.Preferences.Captions
            && controller.Preferences.Preset == AudioMixPreset.ReducedRange
            && controller.Preferences.Channels[1].Muted && controller.Preferences.Channels[1].Gain == 0.37,
            "Reload did not restore the complete preferences.");
        panel.Open();
        Control[] controls = Descendants(panel).OfType<Control>()
            .Where(control => control.FocusMode == Control.FocusModeEnum.All).ToArray();
        Require(controls.OfType<HSlider>().Count() == 6, "Expected six independent sliders.");
        Require(controls.OfType<CheckButton>().Count() == 8, "Expected six mutes, mono and captions.");
        Require(controls.All(control => !control.FocusNext.IsEmpty && !control.FocusPrevious.IsEmpty), "Missing focus neighbors.");
        Require(GetViewport().GuiGetFocusOwner()?.Name == new StringName("MasterGain"), "Opening did not focus Master.");
        panel.Close();
        Require(!panel.Visible && GetViewport().GuiGetFocusOwner() is null, "Closing did not release focus.");
        controller.RestoreDefaults();
    }

    private void BuildInterface()
    {
        CanvasLayer layer = new();
        AddChild(layer);
        ColorRect background = new() { Color = new Color(0.055f, 0.067f, 0.082f) };
        layer.AddChild(background);
        background.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.FullRect);
        VBoxContainer content = new() { Position = new Vector2(40, 40), Size = new Vector2(620, 480) };
        content.AddThemeConstantOverride("separation", 16);
        background.AddChild(content);
        content.AddChild(new Label { Text = "Audio routing laboratory" });
        content.AddChild(new Label { Text = "Temporary diagnostic signals · starts stopped · no world sounds" });
        category = new OptionButton();
        foreach (AudioCategory bus in Enum.GetValues<AudioCategory>().Where(value => value != AudioCategory.Master))
        {
            category.AddItem(bus.ToString());
        }
        content.AddChild(category);
        Button start = new() { Text = "Start selected probe" };
        start.Pressed += () => Start((AudioCategory)(category.Selected + 1));
        content.AddChild(start);
        Button stop = new() { Text = "Stop all probes" };
        stop.Pressed += Stop;
        content.AddChild(stop);
        Button settings = new() { Text = "Audio settings" };
        settings.Pressed += () => panel.Open(settings);
        content.AddChild(settings);
        Button speechButton = new() { Text = "Toggle synthetic speech foreground" };
        speechButton.Pressed += () => ToggleForeground(AudioForeground.Speech, ref speech);
        content.AddChild(speechButton);
        Button cueButton = new() { Text = "Toggle synthetic consequential cue" };
        cueButton.Pressed += () => ToggleForeground(AudioForeground.ConsequentialCue, ref cue);
        content.AddChild(cueButton);
        status = new Label { Text = "Stopped" };
        content.AddChild(status);
        if (OS.GetEnvironment("ALLRPCS_AUDIO_PANEL_PREVIEW") == "1")
        {
            panel.Open(settings);
        }
    }

    private void ToggleForeground(AudioForeground foreground, ref long token)
    {
        if (token == 0) { token = controller.Ducking.Begin(foreground); }
        else { controller.Ducking.End(token); token = 0; }
    }

    private void Start(AudioCategory bus, double amplitude = 0.01, bool leftOnly = false)
    {
        player.Stop();
        player.Bus = new StringName(bus.ToString());
        player.VolumeDb = 0;
        player.Stream = AudioDiagnosticSignal.Create(220 + (int)bus * 110, amplitude, leftOnly);
        player.Play();
        status.Text = $"Playing {bus} probe";
    }

    private void Stop()
    {
        if (player is not null) { player.Stop(); }
        controller?.Ducking.Clear();
        speech = cue = 0;
        if (status is not null) { status.Text = "Stopped"; }
    }

    private static IEnumerable<Node> Descendants(Node parent)
    {
        foreach (Node child in parent.GetChildren())
        {
            yield return child;
            foreach (Node descendant in Descendants(child)) { yield return descendant; }
        }
    }

    private static void Require(bool value, string message)
    {
        if (!value) { throw new InvalidOperationException(message); }
    }
}
