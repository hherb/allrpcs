namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed dimension of character relationship state.</summary>
public sealed class CharacterRelationshipDimension : IEquatable<CharacterRelationshipDimension>
{
    private CharacterRelationshipDimension(string value)
    {
        Value = value;
    }

    /// <summary>Gets the trust dimension.</summary>
    public static CharacterRelationshipDimension Trust { get; } = new("trust");

    /// <summary>Gets the affection dimension.</summary>
    public static CharacterRelationshipDimension Affection { get; } = new("affection");

    /// <summary>Gets the respect dimension.</summary>
    public static CharacterRelationshipDimension Respect { get; } = new("respect");

    /// <summary>Gets the fear dimension.</summary>
    public static CharacterRelationshipDimension Fear { get; } = new("fear");

    /// <summary>Gets every dimension in stable order.</summary>
    public static IReadOnlyList<CharacterRelationshipDimension> All { get; } = Array.AsReadOnly(
        new[] { Trust, Affection, Respect, Fear });

    /// <summary>Gets the canonical dimension text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined dimension.</summary>
    public static CharacterRelationshipDimension Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "trust" => Trust,
            "affection" => Affection,
            "respect" => Respect,
            "fear" => Fear,
            _ => throw new ArgumentException("Unknown relationship dimension.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterRelationshipDimension? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterRelationshipDimension other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined dimension.</summary>
    public static bool operator ==(
        CharacterRelationshipDimension? left,
        CharacterRelationshipDimension? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined dimensions.</summary>
    public static bool operator !=(
        CharacterRelationshipDimension? left,
        CharacterRelationshipDimension? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
