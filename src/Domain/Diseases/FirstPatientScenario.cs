using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Composes exact authored state for the first patient before stabilization.</summary>
public sealed class FirstPatientScenario
{
    /// <summary>Creates one immutable first-patient scenario without applying any outcome.</summary>
    public FirstPatientScenario(
        CharacterId patientId,
        DiseaseTruthSnapshot diseaseTruth,
        IngredientStateSnapshot ingredients,
        StabilizingRemedyDefinition remedy,
        CriticalInvestigationConclusionScenario investigation,
        IngredientStateSnapshot? gatheredIngredients = null,
        IEnumerable<IngredientPreparationRecipe>? alternativeRecipes = null)
    {
        ArgumentNullException.ThrowIfNull(patientId);
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(ingredients);
        ArgumentNullException.ThrowIfNull(remedy);
        ArgumentNullException.ThrowIfNull(investigation);

        PatientId = patientId;
        DiseaseTruth = diseaseTruth;
        Ingredients = ingredients;
        Remedy = remedy;
        Investigation = investigation;
        GatheredIngredients = gatheredIngredients ?? new IngredientStateSnapshot([]);
        StationIngredients = new IngredientStateSnapshot(ingredients.States.Where(state =>
            !GatheredIngredients.States.Any(gathered =>
                ReferenceEquals(gathered.Definition, state.Definition))));
        List<IngredientPreparationRecipe> alternatives = [];
        foreach (IngredientPreparationRecipe alternative in alternativeRecipes ?? [])
        {
            alternatives.Add(alternative ?? throw new ArgumentException(
                "Alternative recipes cannot contain null values.",
                nameof(alternativeRecipes)));
        }

        AlternativeRecipes = alternatives.AsReadOnly();
    }

    /// <summary>Gets the exact authored first-patient identity.</summary>
    public CharacterId PatientId { get; }

    /// <summary>Gets the exact hidden authoritative disease truth.</summary>
    public DiseaseTruthSnapshot DiseaseTruth { get; }

    /// <summary>Gets the exact available ingredient state.</summary>
    public IngredientStateSnapshot Ingredients { get; }

    /// <summary>Gets the exact authored stabilizing-remedy definition.</summary>
    public StabilizingRemedyDefinition Remedy { get; }

    /// <summary>Gets the exact recoverable critical investigation.</summary>
    public CriticalInvestigationConclusionScenario Investigation { get; }

    /// <summary>
    /// Gets the ingredients that must be gathered in the world; an empty snapshot means every
    /// ingredient is already at the preparation station.
    /// </summary>
    public IngredientStateSnapshot GatheredIngredients { get; }

    /// <summary>Gets the ingredients kept at the preparation station: all but the gathered ones.</summary>
    public IngredientStateSnapshot StationIngredients { get; }

    /// <summary>
    /// Gets other recipes the station can prepare. None of them is the stabilizing remedy, so a
    /// player can prepare and administer a wrong remedy.
    /// </summary>
    public IReadOnlyList<IngredientPreparationRecipe> AlternativeRecipes { get; }

    /// <summary>Reports whether the recipe is the remedy or one of the alternatives, by reference.</summary>
    public bool IsPreparable(IngredientPreparationRecipe recipe) =>
        ReferenceEquals(recipe, Remedy.Recipe)
        || AlternativeRecipes.Any(alternative => ReferenceEquals(alternative, recipe));
}
