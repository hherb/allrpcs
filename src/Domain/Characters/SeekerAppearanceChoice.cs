using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one asset-neutral authored Seeker appearance choice.</summary>
public sealed record SeekerAppearanceChoice
{
    /// <summary>Gets the maximum supported authored display-name length.</summary>
    public const int MaximumDisplayNameLength = 80;

    /// <summary>Creates one canonical choice without selecting or rendering an asset.</summary>
    public SeekerAppearanceChoice(string key, string displayName)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        DisplayName = AuthoredCharacterText.RequireNonBlank(
            displayName,
            MaximumDisplayNameLength,
            nameof(displayName));
    }

    /// <summary>Gets the stable canonical choice key.</summary>
    public string Key { get; }

    /// <summary>Gets the exact authored player-facing choice name.</summary>
    public string DisplayName { get; }
}
