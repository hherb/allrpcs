namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies evidence without attaching mutable evidence state.
/// </summary>
public sealed record EvidenceId
{
    /// <summary>
    /// Creates an evidence identifier from canonical text.
    /// </summary>
    public EvidenceId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into an evidence identifier.
    /// </summary>
    public static EvidenceId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
