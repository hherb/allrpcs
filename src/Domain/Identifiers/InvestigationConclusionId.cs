namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored investigation conclusion.</summary>
public sealed record InvestigationConclusionId
{
    /// <summary>Creates an investigation-conclusion identifier from canonical text.</summary>
    public InvestigationConclusionId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an investigation-conclusion identifier.</summary>
    public static InvestigationConclusionId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
