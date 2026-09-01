namespace Allrpcs.Domain.Perception;

/// <summary>
/// Identifies one of the closed set of visibility states without assigning visibility or
/// defining perception mechanics.
/// </summary>
public sealed class VisibilityState : IEquatable<VisibilityState>
{
    private VisibilityState(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Gets the state in which a character has not noticed something.
    /// </summary>
    public static VisibilityState Unnoticed { get; } = new("unnoticed");

    /// <summary>
    /// Gets the state in which a character has noticed something.
    /// </summary>
    public static VisibilityState Noticed { get; } = new("noticed");

    /// <summary>
    /// Gets the state in which a character has identified something.
    /// </summary>
    public static VisibilityState Identified { get; } = new("identified");

    /// <summary>
    /// Gets the state in which a character has understood something.
    /// </summary>
    public static VisibilityState Understood { get; } = new("understood");

    /// <summary>
    /// Gets the state in which a character has altered something.
    /// </summary>
    public static VisibilityState Altered { get; } = new("altered");

    /// <summary>
    /// Gets the state in which a character has collected something.
    /// </summary>
    public static VisibilityState Collected { get; } = new("collected");

    /// <summary>
    /// Gets every defined visibility state in stable roadmap order.
    /// </summary>
    public static IReadOnlyList<VisibilityState> All { get; } = Array.AsReadOnly(
        new[] { Unnoticed, Noticed, Identified, Understood, Altered, Collected });

    /// <summary>
    /// Gets the stable canonical text for this visibility state.
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Parses canonical text into one of the defined visibility states.
    /// </summary>
    public static VisibilityState Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "unnoticed" => Unnoticed,
            "noticed" => Noticed,
            "identified" => Identified,
            "understood" => Understood,
            "altered" => Altered,
            "collected" => Collected,
            _ => throw new ArgumentException("Unknown visibility state.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(VisibilityState? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is VisibilityState other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>
    /// Reports whether two visibility states are the same defined state.
    /// </summary>
    public static bool operator ==(VisibilityState? left, VisibilityState? right) =>
        ReferenceEquals(left, right);

    /// <summary>
    /// Reports whether two visibility states are different defined states.
    /// </summary>
    public static bool operator !=(VisibilityState? left, VisibilityState? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
