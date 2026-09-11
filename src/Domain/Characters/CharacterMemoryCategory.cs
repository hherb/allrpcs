namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed category of locally retained character memory.</summary>
public sealed class CharacterMemoryCategory : IEquatable<CharacterMemoryCategory>
{
    private CharacterMemoryCategory(string value)
    {
        Value = value;
    }

    /// <summary>Gets the significant-experience memory category.</summary>
    public static CharacterMemoryCategory Episodic { get; } = new("episodic");

    /// <summary>Gets the categorized fact, claim, or belief memory category.</summary>
    public static CharacterMemoryCategory Epistemic { get; } = new("epistemic");

    /// <summary>Gets the directional character-relationship memory category.</summary>
    public static CharacterMemoryCategory Relationship { get; } = new("relationship");

    /// <summary>Gets the unfinished promise or task memory category.</summary>
    public static CharacterMemoryCategory Commitment { get; } = new("commitment");

    /// <summary>Gets every defined memory category in stable order.</summary>
    public static IReadOnlyList<CharacterMemoryCategory> All { get; } = Array.AsReadOnly(
        new[] { Episodic, Epistemic, Relationship, Commitment });

    /// <summary>Gets the canonical category text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined memory category.</summary>
    public static CharacterMemoryCategory Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "episodic" => Episodic,
            "epistemic" => Epistemic,
            "relationship" => Relationship,
            "commitment" => Commitment,
            _ => throw new ArgumentException("Unknown character-memory category.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterMemoryCategory? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CharacterMemoryCategory other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined memory category.</summary>
    public static bool operator ==(CharacterMemoryCategory? left, CharacterMemoryCategory? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined memory categories.</summary>
    public static bool operator !=(CharacterMemoryCategory? left, CharacterMemoryCategory? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
