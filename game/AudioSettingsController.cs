using Allrpcs.Application.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Applies user settings and presentation ducking to the six dedicated mixer buses.</summary>
public partial class AudioSettingsController : Node
{
    private const double GainRampSeconds = 0.05;
    private readonly double[] currentGain = new double[6];
    private AudioSettingsStorageController storage = null!;

    /// <summary>The current complete immutable user preferences.</summary>
    public AudioPreferences Preferences { get; private set; } = AudioPreferences.Defaults();
    /// <summary>Presentation-only foreground leases; clients end them on every completion path.</summary>
    public AudioDuckingEnvelope Ducking { get; } = new();
    /// <summary>A readable load/save outcome without file paths.</summary>
    public string Status { get; private set; } = "Audio: defaults";
    /// <summary>Raised when preference values or status need refreshing.</summary>
    public event Action? Changed;

    /// <inheritdoc />
    public override void _Ready()
    {
        AudioServer.SetBusLayout(GD.Load<AudioBusLayout>("res://AudioBusLayout.tres"));
        storage = GetNode<AudioSettingsStorageController>("../AudioSettingsStorageController");
        Reload();
        for (int index = 0; index < currentGain.Length; index++)
        {
            currentGain[index] = Preferences.Channels[index].Gain;
        }
        ApplyMix();
    }

    /// <summary>Loads the complete stored snapshot before applying its mix.</summary>
    public void Reload()
    {
        AudioPreferencesLoadResult loaded = storage.Load();
        Preferences = loaded.Preferences;
        Status = loaded.Status switch
        {
            AudioPreferencesLoadStatus.Loaded => "Audio: loaded",
            AudioPreferencesLoadStatus.Invalid => "Audio: unreadable settings · defaults restored",
            _ => "Audio: defaults",
        };
        Changed?.Invoke();
    }

    /// <summary>Changes only presentation preferences; no world events are read or created.</summary>
    public void SetPreferences(AudioPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        Preferences = preferences;
        Status = "Audio: applied · save to keep";
        ApplyMix();
        Changed?.Invoke();
    }

    /// <summary>Restores a complete default snapshot.</summary>
    public void RestoreDefaults() => SetPreferences(AudioPreferences.Defaults());
    /// <summary>Saves the current snapshot without overwriting other local documents.</summary>
    public bool Save()
    {
        bool saved = storage.Save(Preferences);
        Status = saved ? "Audio: saved" : "Audio: save failed · current settings retained";
        Changed?.Invoke();
        return saved;
    }

    /// <inheritdoc />
    public override void _Process(double delta)
    {
        Ducking.Advance(delta);
        for (int index = 0; index < currentGain.Length; index++)
        {
            double target = Preferences.Channels[index].Gain;
            currentGain[index] += Math.Clamp(target - currentGain[index],
                -delta / GainRampSeconds, delta / GainRampSeconds);
        }
        ApplyMix();
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        Ducking.Clear();
        // Remove transient foreground attenuation while retaining the chosen user settings.
        ApplyMix();
    }

    private void ApplyMix()
    {
        AudioServer.SetBusEffectEnabled(0, 0, Preferences.Preset == AudioMixPreset.ReducedRange);
        AudioServer.SetBusEffectEnabled(0, 1, Preferences.Mono);
        foreach (AudioChannelSetting channel in Preferences.Channels)
        {
            int index = (int)channel.Category;
            // Only Music/Ambience can duck globally. Required effects must remain intact.
            double duckGain = channel.Category is AudioCategory.Music or AudioCategory.Ambience
                ? Ducking.Gain(channel.Category) : 1;
            double gain = currentGain[index] * duckGain;
            bool silent = channel.Muted || channel.Gain == 0;
            AudioServer.SetBusMute(index, silent);
            AudioServer.SetBusVolumeLinear(index, (float)Math.Max(gain, 0.000001));
        }
    }
}
