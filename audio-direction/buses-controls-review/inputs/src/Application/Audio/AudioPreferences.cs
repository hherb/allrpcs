namespace Allrpcs.Application.Audio;

/// <summary>Stable presentation-only mixer categories, with Master first.</summary>
public enum AudioCategory { Master, Music, Ambience, Effects, Dialogue, Speech }

/// <summary>Selectable presentation dynamics; neither mode changes perception.</summary>
public enum AudioMixPreset { Default, ReducedRange }

/// <summary>Immutable user gain and independent mute for one category.</summary>
public sealed class AudioChannelSetting
{
    /// <summary>Creates a finite, bounded gain without allowing amplification.</summary>
    public AudioChannelSetting(AudioCategory category, double gain, bool muted)
    {
        if (!Enum.IsDefined(category) || !double.IsFinite(gain) || gain < 0 || gain > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(gain));
        }
        Category = category;
        Gain = gain;
        Muted = muted;
    }

    /// <summary>The stable mixer category.</summary>
    public AudioCategory Category { get; }
    /// <summary>User linear amplitude in the inclusive range zero to one.</summary>
    public double Gain { get; }
    /// <summary>Explicit mute, independent of the retained slider value.</summary>
    public bool Muted { get; }
}

/// <summary>Complete validated presentation preferences, independent of world saves.</summary>
public sealed class AudioPreferences
{
    /// <summary>Copies all categories and rejects partial or ambiguous documents.</summary>
    public AudioPreferences(IEnumerable<AudioChannelSetting> channels,
        AudioMixPreset preset = AudioMixPreset.Default, bool mono = false, bool captions = true)
    {
        ArgumentNullException.ThrowIfNull(channels);
        AudioChannelSetting[] copy = channels.ToArray();
        if (!Enum.IsDefined(preset) || copy.Length != Enum.GetValues<AudioCategory>().Length
            || copy.Any(item => item is null)
            || copy.Select(item => item.Category).Distinct().Count() != copy.Length)
        {
            throw new ArgumentException("A complete unambiguous audio snapshot is required.", nameof(channels));
        }
        Channels = Array.AsReadOnly(copy.OrderBy(item => item.Category).ToArray());
        Preset = preset;
        Mono = mono;
        Captions = captions;
    }

    /// <summary>Six immutable settings in stable category order.</summary>
    public IReadOnlyList<AudioChannelSetting> Channels { get; }
    /// <summary>Selected output dynamics.</summary>
    public AudioMixPreset Preset { get; }
    /// <summary>Fold down the common stereo output when enabled.</summary>
    public bool Mono { get; }
    /// <summary>Reserved preference for future authorized non-speech captions.</summary>
    public bool Captions { get; }
    /// <summary>A fresh complete snapshot at unity, unmuted, with captions enabled.</summary>
    public static AudioPreferences Defaults() => new(
        Enum.GetValues<AudioCategory>().Select(category => new AudioChannelSetting(category, 1, false)));

    /// <summary>Returns a replacement snapshot without mutating other channels.</summary>
    public AudioPreferences WithChannel(AudioCategory category, double gain, bool muted)
    {
        AudioChannelSetting replacement = new(category, gain, muted);
        return new(Channels.Select(item => item.Category == category ? replacement : item), Preset, Mono, Captions);
    }
}
