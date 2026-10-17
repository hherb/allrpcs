using Allrpcs.Application.Input;
using Allrpcs.Infrastructure.Input;
using Godot;

namespace Allrpcs.Game;

/// <summary>Composes controls-only storage, isolated from world/audio/provider documents.</summary>
public partial class ControlSettingsStorageController : Node
{
    private FileControlPreferencesStore store = null!;
    /// <inheritdoc />
    public override void _Ready()
    {
        bool check = DebugTestEnvironment.GetEnvironment("ALLRPCS_CONTROLS_MAIN_CHECK") == "1";
        string path = check ? Path.Combine(Path.GetTempPath(), $"allrpcs-controls-{OS.GetProcessId()}.json")
            : ProjectSettings.GlobalizePath("user://controls-settings-v1.json");
        store = new(path);
    }
    /// <summary>Loads a complete snapshot or defaults.</summary>
    public ControlPreferencesLoadResult Load() => store.Load();
    /// <summary>Saves a complete snapshot atomically.</summary>
    public bool Save(ControlPreferences value) => store.Save(value);
}
