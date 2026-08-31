using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Describes an authored character limitation without defining executable policy.
/// </summary>
public sealed record CharacterLimitation
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumDescriptionLength = CharacterCapability.MaximumDescriptionLength;

    /// <summary>
    /// Creates an immutable limitation from a canonical key and authored description.
    /// </summary>
    public CharacterLimitation(string key, string description)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Description = AuthoredCharacterText.RequireNonBlank(
            description,
            MaximumDescriptionLength,
            nameof(description));
    }

    /// <summary>
    /// Gets the stable canonical limitation key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored limitation description exactly as supplied.
    /// </summary>
    public string Description { get; }
}
