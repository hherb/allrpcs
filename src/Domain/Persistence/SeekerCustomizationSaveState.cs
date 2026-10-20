using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Persistence;

/// <summary>Retains a selected Seeker profile as authored text and option keys for persistence.</summary>
public sealed record SeekerCustomizationSaveState
{
    /// <summary>Creates one profile save state for the canonical Seeker identity.</summary>
    public SeekerCustomizationSaveState(
        CharacterIdentity identity,
        SeekerPronouns pronouns,
        string appearanceKey,
        string backgroundKey)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(pronouns);
        if (identity.Id.Value != SeekerCustomizationProfile.SeekerCharacterIdValue)
        {
            throw new ArgumentException(
                "A saved Seeker profile requires the canonical Seeker identity.",
                nameof(identity));
        }

        Identity = identity;
        Pronouns = pronouns;
        AppearanceKey = CanonicalIdentifierText.RequireCanonical(appearanceKey, nameof(appearanceKey));
        BackgroundKey = CanonicalIdentifierText.RequireCanonical(backgroundKey, nameof(backgroundKey));
    }

    /// <summary>Gets the exact Seeker identity and player-authored display name.</summary>
    public CharacterIdentity Identity { get; }

    /// <summary>Gets the exact player-authored pronoun forms.</summary>
    public SeekerPronouns Pronouns { get; }

    /// <summary>Gets the selected appearance option key.</summary>
    public string AppearanceKey { get; }

    /// <summary>Gets the selected background option key.</summary>
    public string BackgroundKey { get; }

    /// <summary>Captures one selected profile.</summary>
    public static SeekerCustomizationSaveState From(SeekerCustomizationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return new SeekerCustomizationSaveState(
            profile.Identity,
            profile.Pronouns,
            profile.Appearance.Key,
            profile.Background.Key);
    }

    /// <summary>Returns whether a selected profile holds exactly these authored values and keys.</summary>
    public bool Matches(SeekerCustomizationProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        return profile.Identity.Id == Identity.Id
            && string.Equals(profile.Identity.DisplayName, Identity.DisplayName, StringComparison.Ordinal)
            && profile.Pronouns == Pronouns
            && string.Equals(profile.Appearance.Key, AppearanceKey, StringComparison.Ordinal)
            && string.Equals(profile.Background.Key, BackgroundKey, StringComparison.Ordinal);
    }
}
