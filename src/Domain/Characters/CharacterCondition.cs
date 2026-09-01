using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Describes an authored qualitative character condition without defining health mechanics.
/// </summary>
public sealed record CharacterCondition
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumDescriptionLength = 500;

    /// <summary>
    /// Creates an immutable condition from a canonical key and authored description.
    /// </summary>
    public CharacterCondition(string key, string description)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Description = AuthoredCharacterText.RequireNonBlank(
            description,
            MaximumDescriptionLength,
            nameof(description));
    }

    /// <summary>
    /// Gets the stable canonical condition key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored qualitative condition description exactly as supplied.
    /// </summary>
    public string Description { get; }
}
