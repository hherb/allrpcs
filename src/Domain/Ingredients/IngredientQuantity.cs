namespace Allrpcs.Domain.Ingredients;

/// <summary>Associates one positive bounded integer amount with one exact authored unit.</summary>
public sealed record IngredientQuantity
{
    /// <summary>Gets the maximum supported integer amount in any defined unit.</summary>
    public const int MaximumAmount = 1_000_000;

    /// <summary>Creates one immutable ingredient quantity without converting its unit.</summary>
    public IngredientQuantity(int amount, IngredientQuantityUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);
        if (amount is < 1 or > MaximumAmount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(amount),
                amount,
                $"Ingredient amount must be between 1 and {MaximumAmount}.");
        }

        Amount = amount;
        Unit = unit;
    }

    /// <summary>Gets the positive bounded integer amount.</summary>
    public int Amount { get; }

    /// <summary>Gets the exact defined unit.</summary>
    public IngredientQuantityUnit Unit { get; }
}
