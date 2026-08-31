namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies an authoritative world event without attaching mutable event state.
/// </summary>
public sealed record WorldEventId
{
    /// <summary>
    /// Creates a world-event identifier from canonical text.
    /// </summary>
    public WorldEventId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into a world-event identifier.
    /// </summary>
    public static WorldEventId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
