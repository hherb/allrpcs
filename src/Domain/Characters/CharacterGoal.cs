using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Describes an authored character goal without defining progress or executable policy.
/// </summary>
public sealed record CharacterGoal
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumDescriptionLength = 500;

    /// <summary>
    /// Creates an immutable goal from a canonical key and authored description.
    /// </summary>
    public CharacterGoal(string key, string description)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Description = AuthoredCharacterText.RequireNonBlank(
            description,
            MaximumDescriptionLength,
            nameof(description));
    }

    /// <summary>
    /// Gets the stable canonical goal key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored goal description exactly as supplied.
    /// </summary>
    public string Description { get; }
}
