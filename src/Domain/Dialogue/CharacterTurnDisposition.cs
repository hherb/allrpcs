namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one of the closed character-turn dispositions without composing a response.
/// </summary>
public sealed class CharacterTurnDisposition : IEquatable<CharacterTurnDisposition>
{
    private CharacterTurnDisposition(string value)
    {
        Value = value;
    }

    /// <summary>Gets the disposition to accept the proposed turn.</summary>
    public static CharacterTurnDisposition Accept { get; } = new("accept");

    /// <summary>Gets the disposition to refuse the proposed turn.</summary>
    public static CharacterTurnDisposition Refuse { get; } = new("refuse");

    /// <summary>Gets the disposition to seek clarification before proceeding.</summary>
    public static CharacterTurnDisposition Clarify { get; } = new("clarify");

    /// <summary>Gets the disposition to accept only under an explicit condition.</summary>
    public static CharacterTurnDisposition ConditionallyAccept { get; } =
        new("conditionally-accept");

    /// <summary>Gets every defined disposition in stable behavior order.</summary>
    public static IReadOnlyList<CharacterTurnDisposition> All { get; } = Array.AsReadOnly(
        new[] { Accept, Refuse, Clarify, ConditionallyAccept });

    /// <summary>Gets the stable canonical text for this disposition.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined dispositions.</summary>
    public static CharacterTurnDisposition Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "accept" => Accept,
            "refuse" => Refuse,
            "clarify" => Clarify,
            "conditionally-accept" => ConditionallyAccept,
            _ => throw new ArgumentException("Unknown character-turn disposition.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterTurnDisposition? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterTurnDisposition other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two dispositions are the same defined instance.</summary>
    public static bool operator ==(
        CharacterTurnDisposition? left,
        CharacterTurnDisposition? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two dispositions are different defined instances.</summary>
    public static bool operator !=(
        CharacterTurnDisposition? left,
        CharacterTurnDisposition? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
