using Allrpcs.Domain.Ingredients;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns one bounded first-patient ingredient-preparation lifecycle.</summary>
public partial class FirstPatientPreparationController : Node
{
    /// <summary>Gets the exact authored recipe, if initialized.</summary>
    public IngredientPreparationRecipe? Recipe { get; private set; }

    /// <summary>Gets the exact current ingredient snapshot, if initialized.</summary>
    public IngredientStateSnapshot? Ingredients { get; private set; }

    /// <summary>Gets the exact current preparation activity, if started.</summary>
    public IngredientPreparationActivity? Activity { get; private set; }

    /// <summary>Gets the exact completed preparation outcome, if completed.</summary>
    public IngredientPreparationOutcome? Outcome { get; private set; }

    /// <summary>Initializes once from one exact recipe and ingredient snapshot.</summary>
    public bool TryInitialize(
        IngredientPreparationRecipe recipe,
        IngredientStateSnapshot ingredients)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(ingredients);

        if (Recipe is not null
            || Ingredients is not null
            || Activity is not null
            || Outcome is not null)
        {
            return false;
        }

        Recipe = recipe;
        Ingredients = ingredients;
        return true;
    }

    /// <summary>Atomically consumes exact inputs and starts the exact recipe once.</summary>
    public bool TryStart(IngredientPreparationRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        IngredientStateSnapshot? ingredients = Ingredients;
        if (ingredients is null
            || !ReferenceEquals(recipe, Recipe)
            || Activity is not null
            || Outcome is not null)
        {
            return false;
        }

        IngredientPreparationStartResult result;
        try
        {
            result = IngredientPreparationStartTransition.Start(recipe, ingredients);
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        Ingredients = result.Ingredients;
        Activity = result.Activity;
        return true;
    }

    /// <summary>Advances only the exact next step of the current preparation.</summary>
    public bool TryAdvance(IngredientPreparationStep completedStep)
    {
        ArgumentNullException.ThrowIfNull(completedStep);

        IngredientPreparationActivity? activity = Activity;
        if (activity is null || Outcome is not null)
        {
            return false;
        }

        try
        {
            Activity = IngredientPreparationActivityProgressionTransition.Advance(
                activity,
                completedStep);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Completes the exact fully progressed activity once.</summary>
    public bool TryComplete()
    {
        IngredientPreparationActivity? activity = Activity;
        if (activity is null || Outcome is not null)
        {
            return false;
        }

        try
        {
            Outcome = IngredientPreparationCompletionTransition.Complete(activity);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
