namespace Allrpcs.Application.Dialogue;

/// <summary>Changes only the explicit selection within immutable provider settings.</summary>
public static class CharacterModelSettingsSelectionTransition
{
    /// <summary>Returns settings with one exact configured canonical provider selected.</summary>
    public static CharacterModelSettingsSelectionSnapshot Select(
        CharacterModelProvider provider,
        CharacterModelSettingsSelectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!CharacterModelProvider.All.Any(candidate => ReferenceEquals(candidate, provider)))
        {
            throw new ArgumentException(
                "A canonical character-model provider is required.",
                nameof(provider));
        }

        if (!snapshot.Settings.Any(
                settings => ReferenceEquals(settings.Provider, provider)))
        {
            throw new InvalidOperationException(
                "Only a provider with configured character-model settings can be selected.");
        }

        return ReferenceEquals(snapshot.SelectedProvider, provider)
            ? snapshot
            : new CharacterModelSettingsSelectionSnapshot(snapshot.Settings, provider);
    }
}
