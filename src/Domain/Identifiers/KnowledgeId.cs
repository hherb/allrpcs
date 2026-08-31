namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies knowledge without attaching mutable knowledge state.
/// </summary>
public sealed record KnowledgeId
{
    /// <summary>
    /// Creates a knowledge identifier from canonical text.
    /// </summary>
    public KnowledgeId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>
    /// Gets the canonical text.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into a knowledge identifier.
    /// </summary>
    public static KnowledgeId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
