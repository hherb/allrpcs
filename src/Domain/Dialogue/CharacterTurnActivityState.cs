namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Identifies one provider-neutral stage of a character turn without controlling runtime work.
/// </summary>
public sealed class CharacterTurnActivityState : IEquatable<CharacterTurnActivityState>
{
    private CharacterTurnActivityState(string value)
    {
        Value = value;
    }

    /// <summary>Gets the state before a complete response is available.</summary>
    public static CharacterTurnActivityState AwaitingResponse { get; } =
        new("awaiting-response");

    /// <summary>Gets the state while complete response speech is being presented.</summary>
    public static CharacterTurnActivityState Speaking { get; } = new("speaking");

    /// <summary>Gets the state after the turn finished normally.</summary>
    public static CharacterTurnActivityState Completed { get; } = new("completed");

    /// <summary>Gets the state after an exact non-dialogue action resolved the turn.</summary>
    public static CharacterTurnActivityState Resolved { get; } = new("resolved");

    /// <summary>Gets the state after pending or speaking work was cancelled.</summary>
    public static CharacterTurnActivityState Cancelled { get; } = new("cancelled");

    /// <summary>Gets every defined state in stable lifecycle order.</summary>
    public static IReadOnlyList<CharacterTurnActivityState> All { get; } = Array.AsReadOnly(
        new[] { AwaitingResponse, Speaking, Completed, Resolved, Cancelled });

    /// <summary>Gets the stable canonical text for this state.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined state instances.</summary>
    public static CharacterTurnActivityState Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "awaiting-response" => AwaitingResponse,
            "speaking" => Speaking,
            "completed" => Completed,
            "resolved" => Resolved,
            "cancelled" => Cancelled,
            _ => throw new ArgumentException("Unknown character-turn activity state.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(CharacterTurnActivityState? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is CharacterTurnActivityState other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two states are the same defined instance.</summary>
    public static bool operator ==(
        CharacterTurnActivityState? left,
        CharacterTurnActivityState? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether two states are different defined instances.</summary>
    public static bool operator !=(
        CharacterTurnActivityState? left,
        CharacterTurnActivityState? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
