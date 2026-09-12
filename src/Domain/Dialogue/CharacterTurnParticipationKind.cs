namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one closed character-turn participation choice without selecting a speaker.
/// </summary>
public sealed class CharacterTurnParticipationKind : IEquatable<CharacterTurnParticipationKind>
{
    private CharacterTurnParticipationKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the choice to answer a directly addressed turn.</summary>
    public static CharacterTurnParticipationKind Respond { get; } = new("respond");

    /// <summary>Gets the choice to enter a turn for an explicit motive.</summary>
    public static CharacterTurnParticipationKind Interject { get; } = new("interject");

    /// <summary>Gets the choice to remain silent for an explicit motive.</summary>
    public static CharacterTurnParticipationKind RemainSilent { get; } = new("remain-silent");

    /// <summary>Gets every defined choice in stable participation order.</summary>
    public static IReadOnlyList<CharacterTurnParticipationKind> All { get; } =
        Array.AsReadOnly(new[] { Respond, Interject, RemainSilent });

    /// <summary>Gets the stable canonical text for this choice.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined choices.</summary>
    public static CharacterTurnParticipationKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "respond" => Respond,
            "interject" => Interject,
            "remain-silent" => RemainSilent,
            _ => throw new ArgumentException(
                "Unknown character-turn participation kind.",
                nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterTurnParticipationKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterTurnParticipationKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two choices are the same defined instance.</summary>
    public static bool operator ==(
        CharacterTurnParticipationKind? left,
        CharacterTurnParticipationKind? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two choices are different defined instances.</summary>
    public static bool operator !=(
        CharacterTurnParticipationKind? left,
        CharacterTurnParticipationKind? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
