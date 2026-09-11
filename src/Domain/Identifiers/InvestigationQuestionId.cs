namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored investigation question.</summary>
public sealed record InvestigationQuestionId
{
    /// <summary>Creates an investigation-question identifier from canonical text.</summary>
    public InvestigationQuestionId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an investigation-question identifier.</summary>
    public static InvestigationQuestionId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
