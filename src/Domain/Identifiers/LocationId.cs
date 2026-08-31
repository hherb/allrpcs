namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies a location without attaching mutable location state.
/// </summary>
public sealed record LocationId
{
    /// <summary>
    /// Creates a location identifier from canonical text.
    /// </summary>
    public LocationId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into a location identifier.
    /// </summary>
    public static LocationId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
