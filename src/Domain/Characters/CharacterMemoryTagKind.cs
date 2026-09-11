namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed deterministic memory-retrieval tag kind.</summary>
public sealed class CharacterMemoryTagKind : IEquatable<CharacterMemoryTagKind>
{
    private CharacterMemoryTagKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the authored subject tag kind.</summary>
    public static CharacterMemoryTagKind Topic { get; } = new("topic");

    /// <summary>Gets the referenced person, object, or other entity tag kind.</summary>
    public static CharacterMemoryTagKind Entity { get; } = new("entity");

    /// <summary>Gets the referenced location tag kind.</summary>
    public static CharacterMemoryTagKind Location { get; } = new("location");

    /// <summary>Gets the directional character-relationship tag kind.</summary>
    public static CharacterMemoryTagKind Relationship { get; } = new("relationship");

    /// <summary>Gets every defined tag kind in stable retrieval order.</summary>
    public static IReadOnlyList<CharacterMemoryTagKind> All { get; } = Array.AsReadOnly(
        new[] { Topic, Entity, Location, Relationship });

    /// <summary>Gets the canonical tag-kind text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined tag kind.</summary>
    public static CharacterMemoryTagKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "topic" => Topic,
            "entity" => Entity,
            "location" => Location,
            "relationship" => Relationship,
            _ => throw new ArgumentException("Unknown character-memory tag kind.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterMemoryTagKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CharacterMemoryTagKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined tag kind.</summary>
    public static bool operator ==(CharacterMemoryTagKind? left, CharacterMemoryTagKind? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined tag kinds.</summary>
    public static bool operator !=(CharacterMemoryTagKind? left, CharacterMemoryTagKind? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
