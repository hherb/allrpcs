namespace Allrpcs.Domain.Diseases;

/// <summary>Identifies the closed level of supportive care applied to one infection.</summary>
public sealed class DiseaseCareState : IEquatable<DiseaseCareState>
{
    private DiseaseCareState(string value)
    {
        Value = value;
    }

    /// <summary>Gets the state with no effective supportive care.</summary>
    public static DiseaseCareState Unassisted { get; } = new("unassisted");

    /// <summary>Gets the state in which comfort has been provided without stopping progression.</summary>
    public static DiseaseCareState Comforted { get; } = new("comforted");

    /// <summary>Gets the state in which authored treatment has stopped further progression.</summary>
    public static DiseaseCareState Stabilized { get; } = new("stabilized");

    /// <summary>Gets every state in increasing treatment-effect order.</summary>
    public static IReadOnlyList<DiseaseCareState> All { get; } = Array.AsReadOnly(
        new[] { Unassisted, Comforted, Stabilized });

    /// <summary>Gets the canonical state text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined state.</summary>
    public static DiseaseCareState Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "unassisted" => Unassisted,
            "comforted" => Comforted,
            "stabilized" => Stabilized,
            _ => throw new ArgumentException("Unknown disease-care state.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(DiseaseCareState? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is DiseaseCareState other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two values are the same defined state.</summary>
    public static bool operator ==(DiseaseCareState? left, DiseaseCareState? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether two values are different defined states.</summary>
    public static bool operator !=(DiseaseCareState? left, DiseaseCareState? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
