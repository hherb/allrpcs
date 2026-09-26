namespace Allrpcs.Domain.Characters;

/// <summary>Composes one Seeker profile only from exact retained authored choices.</summary>
public static class SeekerCustomizationSelection
{
    /// <summary>Gets the canonical stable identifier required for the customized Seeker.</summary>
    public const string SeekerCharacterIdValue =
        SeekerCustomizationProfile.SeekerCharacterIdValue;

    /// <summary>Composes an exact profile without applying scene, route, or capability effects.</summary>
    public static SeekerCustomizationProfile Compose(
        SeekerCustomizationOptions options,
        CharacterIdentity identity,
        SeekerPronouns pronouns,
        SeekerAppearanceChoice appearance,
        SeekerBackgroundChoice background)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(pronouns);
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(background);
        if (identity.Id.Value != SeekerCharacterIdValue)
        {
            throw new ArgumentException(
                "Seeker customization requires the canonical Seeker identity.",
                nameof(identity));
        }

        if (!options.AppearanceChoices.Any(choice => ReferenceEquals(choice, appearance)))
        {
            throw new ArgumentException(
                "The selected appearance must be an exact retained option.",
                nameof(appearance));
        }

        if (!options.BackgroundChoices.Any(choice => ReferenceEquals(choice, background)))
        {
            throw new ArgumentException(
                "The selected background must be an exact retained option.",
                nameof(background));
        }

        return new SeekerCustomizationProfile(identity, pronouns, appearance, background);
    }
}
