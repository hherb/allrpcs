using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>
/// Describes an authored qualitative environmental state without defining simulation mechanics.
/// </summary>
public sealed record EnvironmentalState
{
    /// <summary>
    /// Gets the maximum supported authored description length.
    /// </summary>
    public const int MaximumDescriptionLength = 500;

    /// <summary>
    /// Creates an immutable environmental state from a canonical key and authored description.
    /// </summary>
    public EnvironmentalState(string key, string description)
    {
        Key = CanonicalIdentifierText.RequireCanonical(key, nameof(key));
        Description = RequireDescription(description);
    }

    /// <summary>
    /// Gets the stable canonical environmental-state key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the authored qualitative environmental-state description exactly as supplied.
    /// </summary>
    public string Description { get; }

    private static string RequireDescription(string description)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (string.IsNullOrWhiteSpace(description)
            || description.Length > MaximumDescriptionLength)
        {
            throw new ArgumentException(
                $"Description must contain a non-whitespace character and be at most "
                    + $"{MaximumDescriptionLength} UTF-16 code units.",
                nameof(description));
        }

        return description;
    }
}
