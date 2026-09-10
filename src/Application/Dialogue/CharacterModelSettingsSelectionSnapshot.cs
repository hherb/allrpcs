namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Retains immutable provider settings and one explicitly selected canonical provider.
/// </summary>
public sealed class CharacterModelSettingsSelectionSnapshot
{
    /// <summary>Gets the maximum number of canonical provider settings.</summary>
    public const int MaximumSettingsCount = 3;

    /// <summary>Copies and validates settings in stable caller-supplied order.</summary>
    public CharacterModelSettingsSelectionSnapshot(
        IEnumerable<CharacterModelSettings> settings,
        CharacterModelProvider selectedProvider)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(selectedProvider);
        if (!IsCanonical(selectedProvider))
        {
            throw new ArgumentException(
                "A canonical selected character-model provider is required.",
                nameof(selectedProvider));
        }

        List<CharacterModelSettings> retainedSettings = new();
        HashSet<CharacterModelProvider> providers = new();
        foreach (CharacterModelSettings providerSettings in settings)
        {
            if (providerSettings is null)
            {
                throw new ArgumentException(
                    "Character-model settings cannot contain null values.",
                    nameof(settings));
            }

            if (!IsCanonical(providerSettings.Provider))
            {
                throw new ArgumentException(
                    "Character-model settings must use a canonical provider.",
                    nameof(settings));
            }

            if (!providers.Add(providerSettings.Provider))
            {
                throw new ArgumentException(
                    "Character-model settings cannot contain duplicate providers.",
                    nameof(settings));
            }

            retainedSettings.Add(providerSettings);
            if (retainedSettings.Count > MaximumSettingsCount)
            {
                throw new ArgumentException(
                    $"Character-model settings cannot exceed {MaximumSettingsCount} entries.",
                    nameof(settings));
            }
        }

        if (retainedSettings.Count == 0)
        {
            throw new ArgumentException(
                "Character-model settings cannot be empty.",
                nameof(settings));
        }

        if (!providers.Contains(selectedProvider))
        {
            throw new ArgumentException(
                "The selected provider must have character-model settings.",
                nameof(selectedProvider));
        }

        Settings = retainedSettings.AsReadOnly();
        SelectedProvider = selectedProvider;
    }

    /// <summary>Gets the exact validated settings in stable supplied order.</summary>
    public IReadOnlyList<CharacterModelSettings> Settings { get; }

    /// <summary>Gets the exact explicitly selected canonical provider.</summary>
    public CharacterModelProvider SelectedProvider { get; }

    private static bool IsCanonical(CharacterModelProvider provider) =>
        CharacterModelProvider.All.Any(candidate => ReferenceEquals(candidate, provider));
}
