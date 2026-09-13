namespace Allrpcs.Domain.Characters;

/// <summary>Identifies one provider-neutral stage of an independent character task.</summary>
public sealed class CharacterIndependentTaskActivityState :
    IEquatable<CharacterIndependentTaskActivityState>
{
    private CharacterIndependentTaskActivityState(string value)
    {
        Value = value;
    }

    /// <summary>Gets the state after a task is assigned but before it starts.</summary>
    public static CharacterIndependentTaskActivityState Assigned { get; } = new("assigned");

    /// <summary>Gets the state while the assigned intention is active.</summary>
    public static CharacterIndependentTaskActivityState Active { get; } = new("active");

    /// <summary>Gets the state after the assigned intention completed.</summary>
    public static CharacterIndependentTaskActivityState Completed { get; } = new("completed");

    /// <summary>Gets the state after assigned or active work was cancelled.</summary>
    public static CharacterIndependentTaskActivityState Cancelled { get; } = new("cancelled");

    /// <summary>Gets every defined state in stable lifecycle order.</summary>
    public static IReadOnlyList<CharacterIndependentTaskActivityState> All { get; } =
        Array.AsReadOnly(new[] { Assigned, Active, Completed, Cancelled });

    /// <summary>Gets the stable canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined state instances.</summary>
    public static CharacterIndependentTaskActivityState Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "assigned" => Assigned,
            "active" => Active,
            "completed" => Completed,
            "cancelled" => Cancelled,
            _ => throw new ArgumentException(
                "Unknown independent-task activity state.",
                nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterIndependentTaskActivityState? other) =>
        ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterIndependentTaskActivityState other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two states are the same defined instance.</summary>
    public static bool operator ==(
        CharacterIndependentTaskActivityState? left,
        CharacterIndependentTaskActivityState? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two states are different defined instances.</summary>
    public static bool operator !=(
        CharacterIndependentTaskActivityState? left,
        CharacterIndependentTaskActivityState? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
