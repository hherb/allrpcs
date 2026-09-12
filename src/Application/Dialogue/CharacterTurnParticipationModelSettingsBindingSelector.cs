namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Resolves trusted provider settings only to an exact caller-composed participation-model binding.
/// </summary>
public static class CharacterTurnParticipationModelSettingsBindingSelector
{
    /// <summary>
    /// Returns the exact selected binding when it retains the exact configured settings.
    /// </summary>
    public static CharacterTurnParticipationModelBinding? Select(
        CharacterModelSettingsSelectionSnapshot selection,
        CharacterTurnParticipationModelBindingSnapshot bindings)
    {
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(bindings);

        CharacterModelSettings selectedSettings = selection.Settings.Single(
            settings => ReferenceEquals(settings.Provider, selection.SelectedProvider));
        CharacterTurnParticipationModelBinding? binding =
            CharacterTurnParticipationModelBindingSelector.Select(
                selection.SelectedProvider,
                bindings);

        return binding is not null && ReferenceEquals(binding.Settings, selectedSettings)
            ? binding
            : null;
    }
}
