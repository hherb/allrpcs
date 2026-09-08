namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one of the initial closed immediate-intention types without executing an action.
/// </summary>
public sealed class CharacterIntentionType : IEquatable<CharacterIntentionType>
{
    private CharacterIntentionType(string value)
    {
        Value = value;
    }

    /// <summary>Gets the intention to take no immediate physical action.</summary>
    public static CharacterIntentionType Wait { get; } = new("wait");

    /// <summary>Gets the intention to examine one existing world object.</summary>
    public static CharacterIntentionType ExamineObject { get; } = new("examine-object");

    /// <summary>Gets the intention to collect one existing world object.</summary>
    public static CharacterIntentionType CollectObject { get; } = new("collect-object");

    /// <summary>Gets every initial intention type in stable scenario order.</summary>
    public static IReadOnlyList<CharacterIntentionType> All { get; } = Array.AsReadOnly(
        new[] { Wait, ExamineObject, CollectObject });

    /// <summary>Gets the stable canonical text for this intention type.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined intention types.</summary>
    public static CharacterIntentionType Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "wait" => Wait,
            "examine-object" => ExamineObject,
            "collect-object" => CollectObject,
            _ => throw new ArgumentException("Unknown character-intention type.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterIntentionType? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterIntentionType other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two intention types are the same defined instance.</summary>
    public static bool operator ==(
        CharacterIntentionType? left,
        CharacterIntentionType? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two intention types are different defined instances.</summary>
    public static bool operator !=(
        CharacterIntentionType? left,
        CharacterIntentionType? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
