namespace Allrpcs.Domain.Ingredients;

/// <summary>Identifies one closed ingredient-preparation process.</summary>
public sealed class IngredientPreparationKind : IEquatable<IngredientPreparationKind>
{
    private IngredientPreparationKind(string value)
    {
        Value = value;
    }

    /// <summary>Gets the grinding process.</summary>
    public static IngredientPreparationKind Grinding { get; } = new("grinding");

    /// <summary>Gets the heating process.</summary>
    public static IngredientPreparationKind Heating { get; } = new("heating");

    /// <summary>Gets the steeping process.</summary>
    public static IngredientPreparationKind Steeping { get; } = new("steeping");

    /// <summary>Gets the cooling process.</summary>
    public static IngredientPreparationKind Cooling { get; } = new("cooling");

    /// <summary>Gets the fermentation process.</summary>
    public static IngredientPreparationKind Fermentation { get; } = new("fermentation");

    /// <summary>Gets all defined preparation processes in stable order.</summary>
    public static IReadOnlyList<IngredientPreparationKind> All { get; } = Array.AsReadOnly(
        new[] { Grinding, Heating, Steeping, Cooling, Fermentation });

    /// <summary>Gets the canonical process text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined preparation process.</summary>
    public static IngredientPreparationKind Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "grinding" => Grinding,
            "heating" => Heating,
            "steeping" => Steeping,
            "cooling" => Cooling,
            "fermentation" => Fermentation,
            _ => throw new ArgumentException("Unknown ingredient preparation process.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(IngredientPreparationKind? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IngredientPreparationKind other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined preparation process.</summary>
    public static bool operator ==(
        IngredientPreparationKind? left,
        IngredientPreparationKind? right) => ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined preparation processes.</summary>
    public static bool operator !=(
        IngredientPreparationKind? left,
        IngredientPreparationKind? right) => !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
