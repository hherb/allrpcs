namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one significant episodic character memory.</summary>
public sealed record EpisodicMemoryId
{
    /// <summary>Creates an episodic-memory identifier from canonical text.</summary>
    public EpisodicMemoryId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an episodic-memory identifier.</summary>
    public static EpisodicMemoryId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
