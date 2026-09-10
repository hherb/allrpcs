namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one locally retained epistemic memory.</summary>
public sealed record EpistemicMemoryId
{
    /// <summary>Creates an epistemic-memory identifier from canonical text.</summary>
    public EpistemicMemoryId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an epistemic-memory identifier.</summary>
    public static EpistemicMemoryId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
