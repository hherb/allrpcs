using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one authored Seeker background without granting route effects.</summary>
public sealed record SeekerBackgroundChoice
{
    /// <summary>Gets the maximum supported authored display-name length.</summary>
    public const int MaximumDisplayNameLength = 80;

    /// <summary>Creates one canonical choice and its exact public character background.</summary>
    public SeekerBackgroundChoice(
        string key,
        string displayName,
        PublicCharacterBackground publicBackground)
    {
        ArgumentNullException.ThrowIfNull(publicBackground);
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        DisplayName = AuthoredCharacterText.RequireNonBlank(
            displayName,
            MaximumDisplayNameLength,
            nameof(displayName));
        PublicBackground = publicBackground;
    }

    /// <summary>Gets the stable canonical choice key.</summary>
    public string Key { get; }

    /// <summary>Gets the exact authored player-facing choice name.</summary>
    public string DisplayName { get; }

    /// <summary>Gets the exact public background applied by later composition.</summary>
    public PublicCharacterBackground PublicBackground { get; }
}
