namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Selects caller-composed participation-model bindings without fallback or substitution.
/// </summary>
public static class CharacterTurnParticipationModelBindingSelector
{
    /// <summary>
    /// Returns the exact binding for one canonical provider, or null when it is unavailable.
    /// </summary>
    public static CharacterTurnParticipationModelBinding? Select(
        CharacterModelProvider provider,
        CharacterTurnParticipationModelBindingSnapshot snapshot)
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
