namespace Allrpcs.Application.Dialogue;

/// <summary>Removes one unselected provider from an immutable settings selection.</summary>
public static class CharacterModelSettingsRemovalTransition
{
    /// <summary>Removes one configured canonical provider without changing the selection.</summary>
    public static CharacterModelSettingsSelectionSnapshot Remove(
        CharacterModelProvider provider,
        CharacterModelSettingsSelectionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(snapshot);
        if (!CharacterModelProvider.All.Any(
                candidate => ReferenceEquals(candidate, provider)))
        {
            throw new ArgumentException(
                "A canonical character-model provider is required.",
                nameof(provider));
        }

        if (ReferenceEquals(snapshot.SelectedProvider, provider))
        {
            throw new InvalidOperationException(
                "The selected character-model provider cannot be removed.");
        }

        int existingIndex = -1;
        for (int index = 0; index < snapshot.Settings.Count; index++)
        {
            if (ReferenceEquals(snapshot.Settings[index].Provider, provider))
            {
                existingIndex = index;
                break;
            }
        }

        if (existingIndex < 0)
        {
            throw new InvalidOperationException(
                "Only configured character-model provider settings can be removed.");
        }

        List<CharacterModelSettings> updated = new(snapshot.Settings);
        updated.RemoveAt(existingIndex);
        return new CharacterModelSettingsSelectionSnapshot(
            updated,
            snapshot.SelectedProvider);
    }
}
