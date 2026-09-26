namespace Allrpcs.Domain.Characters;

/// <summary>Retains one exact immutable set of player-authored Seeker choices.</summary>
public sealed class SeekerCustomizationProfile
{
    /// <summary>Gets the canonical stable identifier required for a Seeker profile.</summary>
    public const string SeekerCharacterIdValue = "seeker";

    /// <summary>Creates a profile from existing validated exact components.</summary>
    public SeekerCustomizationProfile(
        CharacterIdentity identity,
        SeekerPronouns pronouns,
        SeekerAppearanceChoice appearance,
        SeekerBackgroundChoice background)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(pronouns);
        ArgumentNullException.ThrowIfNull(appearance);
        ArgumentNullException.ThrowIfNull(background);
        if (identity.Id.Value != SeekerCharacterIdValue)
        {
            throw new ArgumentException(
                "A Seeker customization profile requires the canonical Seeker identity.",
                nameof(identity));
        }

        Identity = identity;
        Pronouns = pronouns;
        Appearance = appearance;
        Background = background;
    }

    /// <summary>Gets the exact stable identity and player-authored display name.</summary>
    public CharacterIdentity Identity { get; }

    /// <summary>Gets the exact player-authored pronoun forms.</summary>
    public SeekerPronouns Pronouns { get; }

    /// <summary>Gets the exact selected asset-neutral appearance choice.</summary>
    public SeekerAppearanceChoice Appearance { get; }

    /// <summary>Gets the exact selected background choice.</summary>
    public SeekerBackgroundChoice Background { get; }
}
