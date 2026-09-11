namespace Allrpcs.Domain.Ingredients;

/// <summary>Identifies one closed authored unit without defining implicit conversion.</summary>
public sealed class IngredientQuantityUnit : IEquatable<IngredientQuantityUnit>
{
    private IngredientQuantityUnit(string value)
    {
        Value = value;
    }

    /// <summary>Gets the unit for a discrete ingredient count.</summary>
    public static IngredientQuantityUnit Count { get; } = new("count");

    /// <summary>Gets the integer milligram unit.</summary>
    public static IngredientQuantityUnit Milligram { get; } = new("milligram");

    /// <summary>Gets the integer millilitre unit.</summary>
    public static IngredientQuantityUnit Millilitre { get; } = new("millilitre");

    /// <summary>Gets all defined units in stable order.</summary>
    public static IReadOnlyList<IngredientQuantityUnit> All { get; } = Array.AsReadOnly(
        new[] { Count, Milligram, Millilitre });

    /// <summary>Gets the canonical unit text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined unit.</summary>
    public static IngredientQuantityUnit Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "count" => Count,
            "milligram" => Milligram,
            "millilitre" => Millilitre,
            _ => throw new ArgumentException("Unknown ingredient quantity unit.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(IngredientQuantityUnit? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IngredientQuantityUnit other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined unit.</summary>
    public static bool operator ==(IngredientQuantityUnit? left, IngredientQuantityUnit? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined units.</summary>
    public static bool operator !=(IngredientQuantityUnit? left, IngredientQuantityUnit? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
