namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies a world object without attaching mutable object state.
/// </summary>
public sealed record ObjectId
{
    /// <summary>
    /// Creates an object identifier from canonical text.
    /// </summary>
    public ObjectId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into an object identifier.
    /// </summary>
    public static ObjectId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
