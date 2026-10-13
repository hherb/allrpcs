namespace Allrpcs.Application.Input;

/// <summary>An isolated local controls document boundary; no world/provider/audio documents.</summary>
public interface IControlPreferencesStore
{
    /// <summary>Loads complete validated values or complete defaults.</summary>
    ControlPreferencesLoadResult Load();
    /// <summary>Atomically persists a complete validated snapshot.</summary>
    bool Save(ControlPreferences preferences);
}
