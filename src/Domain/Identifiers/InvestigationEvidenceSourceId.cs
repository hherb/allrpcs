namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored source of investigation evidence.</summary>
public sealed record InvestigationEvidenceSourceId
{
    /// <summary>Creates an investigation-evidence source identifier from canonical text.</summary>
    public InvestigationEvidenceSourceId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an investigation-evidence source identifier.</summary>
    public static InvestigationEvidenceSourceId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
