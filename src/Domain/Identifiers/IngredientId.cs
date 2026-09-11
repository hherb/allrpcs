namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored ingredient independently of world-object identity.</summary>
public sealed record IngredientId
{
    /// <summary>Creates an ingredient identifier from canonical text.</summary>
    public IngredientId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into an ingredient identifier.</summary>
    public static IngredientId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
