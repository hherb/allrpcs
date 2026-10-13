namespace Allrpcs.Application.Input;

/// <summary>Finite path-free local preference load outcome.</summary>
public enum ControlPreferencesLoadStatus { Loaded, Missing, Invalid }
/// <summary>Complete loaded/default preferences, never partial salvage.</summary>
public sealed record ControlPreferencesLoadResult(ControlPreferencesLoadStatus Status, ControlPreferences Preferences);
