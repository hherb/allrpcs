namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Resolves trusted provider settings only to an exact caller-composed binding.
/// </summary>
public static class CharacterModelSettingsBindingSelector
{
    /// <summary>
    /// Returns the exact selected binding when it retains the exact configured settings.
    /// </summary>
    public static CharacterModelBinding? Select(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterModelBindingSnapshot bindings)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(bindings);

        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        CharacterModelBinding? binding = CharacterModelBindingSelector.Select(
            selection.SelectedProvider,
            bindings);

        return binding is not null && ReferenceEquals(binding.Settings, selectedSettings)
            ? binding
            : null;
    }
}
