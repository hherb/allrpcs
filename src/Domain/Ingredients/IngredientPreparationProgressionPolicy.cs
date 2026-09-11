namespace Allrpcs.Domain.Ingredients;

/// <summary>Decides whether one exact authored preparation step can advance progress.</summary>
public static class IngredientPreparationProgressionPolicy
{
    /// <summary>Returns whether the exact next step may advance the supplied progress.</summary>
    public static bool CanAdvance(
        IngredientPreparationProgress progress,
        IngredientPreparationStep completedStep)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(completedStep);

        return ReferenceEquals(progress.NextStep, completedStep);
    }
}
