using System.Numerics;
using Allrpcs.Domain.Ingredients;

namespace Allrpcs.Application.Audio;

/// <summary>Recognizes only the existing exact second-step commit, without progressing preparation.</summary>
public static class RemedyHeatingPresentationProjection
{
    /// <summary>Anonymous local action text, independent of output availability.</summary>
    public const string Phrase = "Heating preparation";
    /// <summary>One short nonlooping passage and caption interval.</summary>
    public const double PassageSeconds = 3;
    /// <summary>Retained inclusive preparation input distance.</summary>
    public const float InteractionPixels = 48;
    /// <summary>Discontinuity cancelling a current passage.</summary>
    public const float TeleportPixels = 96;

    /// <summary>Checks exact recipe, step and immutable before/after transition identities.</summary>
    public static bool IsCommit(IngredientPreparationRecipe? recipe, IngredientPreparationStep? step,
        IngredientPreparationActivity? before, IngredientPreparationActivity? after) =>
        recipe is not null && step is not null && before is not null && after is not null
        && recipe.RecipeId.Value == "cooling-moonwort-remedy" && recipe.Plan.Steps.Count == 4
        && ReferenceEquals(recipe.Plan.Steps[1], step)
        && ReferenceEquals(step.Kind, IngredientPreparationKind.Heating)
        && ReferenceEquals(before.Recipe, recipe) && ReferenceEquals(after.Recipe, recipe)
        && ReferenceEquals(before.Progress.Plan, recipe.Plan) && ReferenceEquals(after.Progress.Plan, recipe.Plan)
        && !ReferenceEquals(before, after) && before.Progress.NextStepIndex == 1 && after.Progress.NextStepIndex == 2;

    /// <summary>Rejects nonfinite, out-of-range or forged supplied positions without changing input results.</summary>
    public static bool Local(Vector2 actual, Vector2 supplied, Vector2 station) =>
        Finite(actual) && Finite(supplied) && Finite(station)
        && Vector2.DistanceSquared(actual, station) <= InteractionPixels * InteractionPixels
        && Vector2.DistanceSquared(actual, supplied) <= 1;

    /// <summary>Only ordinary finite frame deltas may retain presentation.</summary>
    public static bool ValidDelta(double delta) => double.IsFinite(delta) && delta > 0 && delta <= 0.1;
    /// <summary>Finite coordinates never create physical or hearing authority.</summary>
    public static bool Finite(Vector2 point) => float.IsFinite(point.X) && float.IsFinite(point.Y);
}
