namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored investigation hypothesis.</summary>
public sealed record InvestigationHypothesisId
{
    /// <summary>Creates an investigation-hypothesis identifier from canonical text.</summary>
    public InvestigationHypothesisId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an investigation-hypothesis identifier.</summary>
    public static InvestigationHypothesisId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
