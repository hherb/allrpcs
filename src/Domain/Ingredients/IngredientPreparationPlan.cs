namespace Allrpcs.Domain.Ingredients;

/// <summary>Retains one non-empty bounded authored preparation order.</summary>
public sealed record IngredientPreparationPlan
{
    /// <summary>Gets the maximum number of steps in one preparation plan.</summary>
    public const int MaximumStepCount = 16;

    /// <summary>Creates an immutable copy of the exact steps in supplied order.</summary>
    public IngredientPreparationPlan(IEnumerable<IngredientPreparationStep> steps)
    {
        ArgumentNullException.ThrowIfNull(steps);

        List<IngredientPreparationStep> retainedSteps = [];
        foreach (IngredientPreparationStep? step in steps)
        {
            if (step is null)
            {
                throw new ArgumentException("Preparation steps cannot contain null.", nameof(steps));
            }

            retainedSteps.Add(step);
            if (retainedSteps.Count > MaximumStepCount)
            {
                throw new ArgumentException(
                    $"A preparation plan accepts at most {MaximumStepCount} steps.",
                    nameof(steps));
            }
        }

        if (retainedSteps.Count == 0)
        {
            throw new ArgumentException("A preparation plan requires at least one step.", nameof(steps));
        }

        Steps = retainedSteps.AsReadOnly();
    }

    /// <summary>Gets the exact steps in stable supplied order.</summary>
    public IReadOnlyList<IngredientPreparationStep> Steps { get; }
}
