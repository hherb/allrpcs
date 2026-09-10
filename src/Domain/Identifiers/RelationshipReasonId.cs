namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one recorded reason for character relationship state.</summary>
public sealed record RelationshipReasonId
{
    /// <summary>Creates a relationship-reason identifier from canonical text.</summary>
    public RelationshipReasonId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a relationship-reason identifier.</summary>
    public static RelationshipReasonId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
