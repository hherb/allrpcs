namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies a character without attaching mutable character state.
/// </summary>
public sealed record CharacterId
{
    /// <summary>
    /// Creates a character identifier from canonical text.
    /// </summary>
    public CharacterId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into a character identifier.
    /// </summary>
    public static CharacterId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
