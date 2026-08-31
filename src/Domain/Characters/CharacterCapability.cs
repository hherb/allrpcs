using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Describes an authored character capability without granting permission or executing behavior.
/// </summary>
public sealed record CharacterCapability
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumDescriptionLength = 500;

    /// <summary>
    /// Creates an immutable capability from a canonical key and authored description.
    /// </summary>
    public CharacterCapability(string key, string description)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Description = AuthoredCharacterText.RequireNonBlank(
            description,
            MaximumDescriptionLength,
            nameof(description));
    }

    /// <summary>
    /// Gets the stable canonical capability key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored capability description exactly as supplied.
    /// </summary>
    public string Description { get; }
}
