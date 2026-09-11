namespace Allrpcs.Domain.Ingredients;

/// <summary>Identifies one closed qualitative ingredient contamination state.</summary>
public sealed class IngredientContamination : IEquatable<IngredientContamination>
{
    private IngredientContamination(string value)
    {
        Value = value;
    }

    /// <summary>Gets the uncontaminated state.</summary>
    public static IngredientContamination Uncontaminated { get; } = new("uncontaminated");

    /// <summary>Gets the suspected-contamination state.</summary>
    public static IngredientContamination Suspected { get; } = new("suspected");

    /// <summary>Gets the confirmed-contamination state.</summary>
    public static IngredientContamination Contaminated { get; } = new("contaminated");

    /// <summary>Gets all defined contamination states in stable order.</summary>
    public static IReadOnlyList<IngredientContamination> All { get; } = Array.AsReadOnly(
        new[] { Uncontaminated, Suspected, Contaminated });

    /// <summary>Gets the canonical contamination text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into one defined contamination state.</summary>
    public static IngredientContamination Parse(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return value switch
        {
            "uncontaminated" => Uncontaminated,
            "suspected" => Suspected,
            "contaminated" => Contaminated,
            _ => throw new ArgumentException("Unknown ingredient contamination.", nameof(value)),
        };
    }

    /// <inheritdoc />
    public bool Equals(IngredientContamination? other) => ReferenceEquals(this, other);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is IngredientContamination other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

    /// <summary>Reports whether both values are the same defined contamination state.</summary>
    public static bool operator ==(IngredientContamination? left, IngredientContamination? right) =>
        ReferenceEquals(left, right);

    /// <summary>Reports whether both values are different defined contamination states.</summary>
    public static bool operator !=(IngredientContamination? left, IngredientContamination? right) =>
        !(left == right);

    /// <inheritdoc />
    public override string ToString() => Value;
}
