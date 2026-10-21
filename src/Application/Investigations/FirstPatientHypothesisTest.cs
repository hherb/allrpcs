using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Investigations;

/// <summary>Pairs one authored hypothesis with the preparation whose dose tests it.</summary>
public sealed class FirstPatientHypothesisTest
{
    /// <summary>Creates one immutable hypothesis test.</summary>
    public FirstPatientHypothesisTest(InvestigationHypothesis hypothesis, IngredientPreparationRecipe recipe)
    {
        ArgumentNullException.ThrowIfNull(hypothesis);
        ArgumentNullException.ThrowIfNull(recipe);
        Hypothesis = hypothesis;
        Recipe = recipe;
    }

    /// <summary>Gets the authored hypothesis, which predicts that a dose stabilizes the patient.</summary>
    public InvestigationHypothesis Hypothesis { get; }

    /// <summary>Gets the exact recipe whose administered dose tests the hypothesis.</summary>
    public IngredientPreparationRecipe Recipe { get; }
}
