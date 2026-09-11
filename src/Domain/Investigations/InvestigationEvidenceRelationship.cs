namespace Allrpcs.Domain.Investigations;

/// <summary>Identifies how authored evidence bears on one hypothesis.</summary>
public sealed class InvestigationEvidenceRelationship : IEquatable<InvestigationEvidenceRelationship>
{
    private InvestigationEvidenceRelationship(string value)
    {
        Value = value;
    }

    /// <summary>Gets the supporting-evidence relationship.</summary>
    public static InvestigationEvidenceRelationship Supports { get; } = new("supports");

    /// <summary>Gets the contradicting-evidence relationship.</summary>
    public static InvestigationEvidenceRelationship Contradicts { get; } = new("contradicts");

    /// <summary>Gets all defined evidence relationships in stable order.</summary>
    public static IReadOnlyList<InvestigationEvidenceRelationship> All { get; } = Array.AsReadOnly(
        new[] { Supports, Contradicts });

    /// <summary>Gets the canonical relationship text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined relationship.</summary>
    public static InvestigationEvidenceRelationship Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "supports" => Supports,
            "contradicts" => Contradicts,
            _ => throw new ArgumentException("Unknown investigation evidence relationship.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(InvestigationEvidenceRelationship? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is InvestigationEvidenceRelationship other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined relationship.</summary>
    public static bool operator ==(
        InvestigationEvidenceRelationship? left,
        InvestigationEvidenceRelationship? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined relationships.</summary>
    public static bool operator !=(
        InvestigationEvidenceRelationship? left,
        InvestigationEvidenceRelationship? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
