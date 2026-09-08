namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies whether one character-turn transcript entry is speech or a physical action.
/// </summary>
public sealed class CharacterTurnTranscriptEntryKind : IEquatable<CharacterTurnTranscriptEntryKind>
{
    private CharacterTurnTranscriptEntryKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the kind for one spoken statement.</summary>
    public static CharacterTurnTranscriptEntryKind SpokenStatement { get; } =
        new("spoken-statement");

    /// <summary>Gets the kind for one physical action.</summary>
    public static CharacterTurnTranscriptEntryKind PhysicalAction { get; } =
        new("physical-action");

    /// <summary>Gets every defined kind in stable transcript order.</summary>
    public static IReadOnlyList<CharacterTurnTranscriptEntryKind> All { get; } =
        Array.AsReadOnly(new[] { SpokenStatement, PhysicalAction });

    /// <summary>Gets the stable canonical text for this kind.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined kind instances.</summary>
    public static CharacterTurnTranscriptEntryKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "spoken-statement" => SpokenStatement,
            "physical-action" => PhysicalAction,
            _ => throw new ArgumentException(
                "Unknown character-turn transcript entry kind.",
                nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterTurnTranscriptEntryKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterTurnTranscriptEntryKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two kinds are the same defined instance.</summary>
    public static bool operator ==(
        CharacterTurnTranscriptEntryKind? left,
        CharacterTurnTranscriptEntryKind? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two kinds are different defined instances.</summary>
    public static bool operator !=(
        CharacterTurnTranscriptEntryKind? left,
        CharacterTurnTranscriptEntryKind? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
