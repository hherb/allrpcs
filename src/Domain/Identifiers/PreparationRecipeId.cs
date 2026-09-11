namespace Allrpcs.Domain.Identifiers;

/// <summary>Identifies one authored ingredient preparation recipe.</summary>
public sealed record PreparationRecipeId
{
    /// <summary>Creates a preparation-recipe identifier from canonical text.</summary>
    public PreparationRecipeId(string value)
    {
        Value = CanonicalIdentifierText.RequireCanonical(value, nameof(value));
    }

    /// <summary>Gets the canonical text.</summary>
    public string Value { get; }

    /// <summary>Parses canonical text into a preparation-recipe identifier.</summary>
    public static PreparationRecipeId Parse(string value) => new(value);

    /// <inheritdoc />
    public override string ToString() => Value;
}
