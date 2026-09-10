namespace Allrpcs.Application.Dialogue;

/// <summary>Changes one provider's settings within an immutable selection snapshot.</summary>
public static class CharacterModelSettingsUpdateTransition
{
    /// <summary>Replaces existing settings in place or appends one newly configured provider.</summary>
    public static CharacterModelSettingsSelectionSnapshot Update(
        CharacterModelSettings settings,
        CharacterModelSettingsSelectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!CharacterModelProvider.All.Any(
                provider => ReferenceEquals(provider, settings.Provider)))
        {
            throw new ArgumentException(
                "Character-model settings must use a canonical provider.",
                nameof(settings));
        }

        int existingIndex = -1;
        for (int index = 0; index < snapshot.Settings.Count; index++)
        {
            CharacterModelSettings existing = snapshot.Settings[index];
            if (ReferenceEquals(existing.Provider, settings.Provider))
            {
                existingIndex = index;
                if (ReferenceEquals(existing, settings))
                {
                    return snapshot;
                }

                break;
            }
        }

        List<CharacterModelSettings> updated = new(snapshot.Settings);
        if (existingIndex >= 0)
        {
            updated[existingIndex] = settings;
        }
        else
        {
            if (updated.Count >= CharacterModelSettingsSelectionSnapshot.MaximumSettingsCount)
            {
                throw new InvalidOperationException(
                    "Character-model settings cannot exceed the configured provider bound.");
            }

            updated.Add(settings);
        }

        return new CharacterModelSettingsSelectionSnapshot(
            updated,
            snapshot.SelectedProvider);
    }
}
