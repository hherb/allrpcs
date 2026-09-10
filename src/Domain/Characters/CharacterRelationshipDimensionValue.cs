namespace Allrpcs.Domain.Characters;

/// <summary>Associates one closed relationship dimension with a bounded current value.</summary>
public sealed record CharacterRelationshipDimensionValue
{
    /// <summary>Gets the minimum dimension value.</summary>
    public const int MinimumValue = -100;

    /// <summary>Gets the maximum dimension value.</summary>
    public const int MaximumValue = 100;

    /// <summary>Creates one immutable bounded dimension value.</summary>
    public CharacterRelationshipDimensionValue(CharacterRelationshipDimension dimension, int value)
    {
        ArgumentNullException.ThrowIfNull(dimension);
        if (value is < MinimumValue or > MaximumValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                $"Relationship dimension values must be between {MinimumValue} and {MaximumValue}.");
        }

        Dimension = dimension;
        Value = value;
    }

    /// <summary>Gets the exact defined dimension.</summary>
    public CharacterRelationshipDimension Dimension { get; }

    /// <summary>Gets the bounded current value.</summary>
    public int Value { get; }
}
