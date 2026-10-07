namespace Allrpcs.Application.Audio;

/// <summary>Explicit outcomes for a missing, valid or unreadable local document.</summary>
public enum AudioPreferencesLoadStatus { Missing, Loaded, Invalid }

/// <summary>A coherent snapshot plus its storage outcome.</summary>
public sealed record AudioPreferencesLoadResult(AudioPreferencesLoadStatus Status, AudioPreferences Preferences);

/// <summary>Persists presentation preferences independently of world and provider state.</summary>
public interface IAudioPreferencesStore
{
    /// <summary>Loads the complete document or a complete default fallback.</summary>
    AudioPreferencesLoadResult Load();
    /// <summary>Atomically saves a validated snapshot, reporting storage failure.</summary>
    bool Save(AudioPreferences preferences);
}
