namespace Allrpcs.Application.Dialogue;

/// <summary>Selects caller-composed character-model bindings without fallback or substitution.</summary>
public static class CharacterModelBindingSelector
{
    /// <summary>
    /// Returns the exact binding for one canonical provider, or null when it is unavailable.
    /// </summary>
    public static CharacterModelBinding? Select(
        CharacterModelProvider provider,
        CharacterModelBindingSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (!CharacterModelProvider.All.Any(candidate => ReferenceEquals(candidate, provider)))
        {
            throw new ArgumentException(
                "A canonical character-model provider is required.",
                nameof(provider));
        }

        return snapshot.Bindings.SingleOrDefault(
            binding => ReferenceEquals(binding.Provider, provider));
    }
}
