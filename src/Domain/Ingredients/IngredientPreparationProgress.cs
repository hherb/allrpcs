namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains immutable progress through one exact ingredient-preparation plan.</summary>
public sealed record IngredientPreparationProgress
{
    /// <summary>Creates progress positioned at one valid next-step index.</summary>
    public IngredientPreparationProgress(
        IngredientPreparationPlan plan,
        int nextStepIndex)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (nextStepIndex < 0 || nextStepIndex > plan.Steps.Count)
        {
            throw new ArgumentOutOfRangeException(
                nameof(nextStepIndex),
                nextStepIndex,
                "The next-step index must be inside the plan or at its end.");
        }

        Plan = plan;
        NextStepIndex = nextStepIndex;
    }

    /// <summary>Gets the exact preparation plan whose progress is represented.</summary>
    public IngredientPreparationPlan Plan { get; }

    /// <summary>Gets the zero-based index of the next step to complete.</summary>
    public int NextStepIndex { get; }

    /// <summary>Gets whether every authored step has completed.</summary>
    public bool IsComplete => NextStepIndex == Plan.Steps.Count;

    /// <summary>Gets the exact next step, or <see langword="null"/> after completion.</summary>
    public IngredientPreparationStep? NextStep =>
        IsComplete ? null : Plan.Steps[NextStepIndex];
}
