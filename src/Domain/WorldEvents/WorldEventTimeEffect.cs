namespace Allrpcs.Domain.WorldEvents;

/// <summary>
/// Identifies whether a world event advances meaningful world time without defining progression.
/// </summary>
public sealed class WorldEventTimeEffect : IEquatable<WorldEventTimeEffect>
{
    private WorldEventTimeEffect(string value)
    {
        Value = value;
    }

    /// <summary>Gets the effect that does not advance meaningful world time.</summary>
    public static WorldEventTimeEffect NoAdvance { get; } = new("no-advance");

    /// <summary>Gets the effect that advances meaningful world time.</summary>
    public static WorldEventTimeEffect Advance { get; } = new("advance");

    /// <summary>Gets every defined world-event time effect in stable canonical order.</summary>
    public static IReadOnlyList<WorldEventTimeEffect> All { get; } = Array.AsReadOnly(
        new[] { NoAdvance, Advance });

    /// <summary>Gets the stable canonical text for this world-event time effect.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one of the defined world-event time effects.</summary>
    public static WorldEventTimeEffect Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value switch
        {
            "no-advance" => NoAdvance,
            "advance" => Advance,
            _ => throw new ArgumentException("Unknown world-event time effect.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(WorldEventTimeEffect? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is WorldEventTimeEffect other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether two time effects are the same defined instance.</summary>
    public static bool operator ==(WorldEventTimeEffect? left, WorldEventTimeEffect? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether two time effects are different defined instances.</summary>
    public static bool operator !=(WorldEventTimeEffect? left, WorldEventTimeEffect? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
