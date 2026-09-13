namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed sabotage action kind without proposing or executing an action.</summary>
public sealed class CharacterSabotageActionKind : IEquatable<CharacterSabotageActionKind>
{
    private CharacterSabotageActionKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the action kind that conceals something.</summary>
    public static CharacterSabotageActionKind Conceal { get; } = new("conceal");

    /// <summary>Gets the action kind that moves something.</summary>
    public static CharacterSabotageActionKind Move { get; } = new("move");

    /// <summary>Gets the action kind that substitutes something.</summary>
    public static CharacterSabotageActionKind Substitute { get; } = new("substitute");

    /// <summary>Gets the action kind that contaminates something.</summary>
    public static CharacterSabotageActionKind Contaminate { get; } = new("contaminate");

    /// <summary>Gets the action kind that misquotes someone.</summary>
    public static CharacterSabotageActionKind Misquote { get; } = new("misquote");

    /// <summary>Gets the action kind that delays something.</summary>
    public static CharacterSabotageActionKind Delay { get; } = new("delay");

    /// <summary>Gets the action kind that influences someone.</summary>
    public static CharacterSabotageActionKind Influence { get; } = new("influence");

    /// <summary>Gets every sabotage action kind in stable roadmap order.</summary>
    public static IReadOnlyList<CharacterSabotageActionKind> All { get; } = Array.AsReadOnly(
        new[] { Conceal, Move, Substitute, Contaminate, Misquote, Delay, Influence });

    /// <summary>Gets the stable canonical text for this action kind.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one exact defined action-kind instance.</summary>
    public static CharacterSabotageActionKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "conceal" => Conceal,
            "move" => Move,
            "substitute" => Substitute,
            "contaminate" => Contaminate,
            "misquote" => Misquote,
            "delay" => Delay,
            "influence" => Influence,
            _ => throw new ArgumentException("Unknown sabotage action kind.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterSabotageActionKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterSabotageActionKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two values are the same defined instance.</summary>
    public static bool operator ==(
        CharacterSabotageActionKind? left,
        CharacterSabotageActionKind? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two values are different defined instances.</summary>
    public static bool operator !=(
        CharacterSabotageActionKind? left,
        CharacterSabotageActionKind? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
