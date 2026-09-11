namespace Allrpcs.Domain.Ingredients;

/// <summary>Identifies one closed qualitative physical condition before preparation.</summary>
public sealed class IngredientPhysicalCondition : IEquatable<IngredientPhysicalCondition>
{
    private IngredientPhysicalCondition(string value)
    {
        Value = value;
    }

    /// <summary>Gets the intact condition.</summary>
    public static IngredientPhysicalCondition Intact { get; } = new("intact");

    /// <summary>Gets the damaged but not ruined condition.</summary>
    public static IngredientPhysicalCondition Damaged { get; } = new("damaged");

    /// <summary>Gets the ruined condition.</summary>
    public static IngredientPhysicalCondition Ruined { get; } = new("ruined");

    /// <summary>Gets all defined physical conditions in stable order.</summary>
    public static IReadOnlyList<IngredientPhysicalCondition> All { get; } = Array.AsReadOnly(
        new[] { Intact, Damaged, Ruined });

    /// <summary>Gets the canonical condition text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined condition.</summary>
    public static IngredientPhysicalCondition Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "intact" => Intact,
            "damaged" => Damaged,
            "ruined" => Ruined,
            _ => throw new ArgumentException("Unknown ingredient physical condition.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(IngredientPhysicalCondition? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) =>
        obj is IngredientPhysicalCondition other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined condition.</summary>
    public static bool operator ==(
        IngredientPhysicalCondition? left,
        IngredientPhysicalCondition? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined conditions.</summary>
    public static bool operator !=(
        IngredientPhysicalCondition? left,
        IngredientPhysicalCondition? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
