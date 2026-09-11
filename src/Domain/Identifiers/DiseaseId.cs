namespace Allrpcs.Domain.Identifiers;

/// <summary>
/// Identifies an authoritative disease definition without exposing disease truth to a character.
/// </summary>
public sealed record DiseaseId
{
    /// <summary>Creates a disease identifier from canonical text.</summary>
    public DiseaseId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a disease identifier.</summary>
    public static DiseaseId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
