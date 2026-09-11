namespace Allrpcs.Domain.Ingredients;

/// <summary>Identifies one closed qualitative ingredient freshness.</summary>
public sealed class IngredientFreshness : IEquatable<IngredientFreshness>
{
    private IngredientFreshness(string value)
    {
        Value = value;
    }

    /// <summary>Gets the fresh state.</summary>
    public static IngredientFreshness Fresh { get; } = new("fresh");

    /// <summary>Gets the aging but not spoiled state.</summary>
    public static IngredientFreshness Aging { get; } = new("aging");

    /// <summary>Gets the spoiled state.</summary>
    public static IngredientFreshness Spoiled { get; } = new("spoiled");

    /// <summary>Gets all defined freshness values in stable order.</summary>
    public static IReadOnlyList<IngredientFreshness> All { get; } = Array.AsReadOnly(
        new[] { Fresh, Aging, Spoiled });

    /// <summary>Gets the canonical freshness text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined freshness.</summary>
    public static IngredientFreshness Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "fresh" => Fresh,
            "aging" => Aging,
            "spoiled" => Spoiled,
            _ => throw new ArgumentException("Unknown ingredient freshness.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(IngredientFreshness? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IngredientFreshness other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined freshness.</summary>
    public static bool operator ==(IngredientFreshness? left, IngredientFreshness? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined freshness values.</summary>
    public static bool operator !=(IngredientFreshness? left, IngredientFreshness? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
