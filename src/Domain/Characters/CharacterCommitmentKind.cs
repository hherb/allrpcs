namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one closed kind of unfinished character commitment.</summary>
public sealed class CharacterCommitmentKind : IEquatable<CharacterCommitmentKind>
{
    private CharacterCommitmentKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets a promise made by the responsible character.</summary>
    public static CharacterCommitmentKind Promise { get; } = new("promise");

    /// <summary>Gets a task assigned to the responsible character.</summary>
    public static CharacterCommitmentKind AssignedTask { get; } = new("assigned-task");

    /// <summary>Gets another explicitly recorded unfinished commitment.</summary>
    public static CharacterCommitmentKind Other { get; } = new("other");

    /// <summary>Gets every defined kind in stable order.</summary>
    public static IReadOnlyList<CharacterCommitmentKind> All { get; } = Array.AsReadOnly(
        new[] { Promise, AssignedTask, Other });

    /// <summary>Gets the canonical kind text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined kind.</summary>
    public static CharacterCommitmentKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "promise" => Promise,
            "assigned-task" => AssignedTask,
            "other" => Other,
            _ => throw new ArgumentException("Unknown character-commitment kind.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterCommitmentKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is CharacterCommitmentKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined kind.</summary>
    public static bool operator ==(CharacterCommitmentKind? left, CharacterCommitmentKind? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined kinds.</summary>
    public static bool operator !=(CharacterCommitmentKind? left, CharacterCommitmentKind? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
