using Allrpcs.Application.Audio;
using Allrpcs.Infrastructure.Audio;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes isolated presentation preferences, without opening world or provider documents.</summary>
public partial class AudioSettingsStorageController : Node
{
    /// <summary>Uses temporary settings for the standalone diagnostic scene.</summary>
    [Export] public bool Disposable { get; set; }
    private FileAudioPreferencesStore store = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        bool check = DebugTestEnvironment.GetEnvironment("ALLRPCS_AUDIO_MAIN_CHECK") == "1";
        string path = Disposable || check
            ? Path.Combine(Path.GetTempPath(), $"allrpcs-audio-settings-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath("user://audio-settings-v1.json");
        store = new FileAudioPreferencesStore(path);
    }

    /// <summary>Loads a complete validated snapshot or complete defaults.</summary>
    public AudioPreferencesLoadResult Load() => store.Load();
    /// <summary>Atomically saves presentation preferences.</summary>
    public bool Save(AudioPreferences preferences) => store.Save(preferences);
}
