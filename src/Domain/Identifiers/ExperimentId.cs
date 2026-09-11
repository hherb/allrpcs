namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one recorded experiment independently of its interpretation.</summary>
public sealed record ExperimentId
{
    /// <summary>Creates an experiment identifier from canonical text.</summary>
    public ExperimentId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an experiment identifier.</summary>
    public static ExperimentId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
