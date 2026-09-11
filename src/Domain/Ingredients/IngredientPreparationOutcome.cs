namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one completed preparation and its exact recipe-authored result.</summary>
public sealed class IngredientPreparationOutcome
{
    /// <summary>Creates an immutable outcome for a completed exact recipe result.</summary>
    public IngredientPreparationOutcome(
        IngredientPreparationActivity activity,
        IngredientDefinition definition,
        IngredientQuantity quantity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(quantity);
        if (!IngredientPreparationCompletionPolicy.CanComplete(activity))
        {
            throw new ArgumentException("Preparation activity must be complete.", nameof(activity));
        }

        if (!ReferenceEquals(activity.Recipe.ResultDefinition, definition))
        {
            throw new ArgumentException(
                "Preparation outcome must retain the recipe's exact result definition.",
                nameof(definition));
        }

        if (!ReferenceEquals(activity.Recipe.ResultQuantity, quantity))
        {
            throw new ArgumentException(
                "Preparation outcome must retain the recipe's exact result quantity.",
                nameof(quantity));
        }

        Activity = activity;
        Definition = definition;
        Quantity = quantity;
    }

    /// <summary>Gets the exact completed preparation activity.</summary>
    public IngredientPreparationActivity Activity { get; }

    /// <summary>Gets the exact recipe-authored result ingredient definition.</summary>
    public IngredientDefinition Definition { get; }

    /// <summary>Gets the exact recipe-authored result quantity.</summary>
    public IngredientQuantity Quantity { get; }
}
