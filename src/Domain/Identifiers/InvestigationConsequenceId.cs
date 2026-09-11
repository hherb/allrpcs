namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored non-executable investigation consequence.</summary>
public sealed record InvestigationConsequenceId
{
    /// <summary>Creates an investigation-consequence identifier from canonical text.</summary>
    public InvestigationConsequenceId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an investigation-consequence identifier.</summary>
    public static InvestigationConsequenceId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
