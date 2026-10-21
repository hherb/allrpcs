using Allrpcs.Application.Diseases;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Godot;

namespace Allrpcs.Game;

/// <summary>Hosts the first-patient preparation lifecycle at the healer-garden station.</summary>
public partial class FirstPatientPreparationController : Node
{
    private RemedyPreparationLifecycle? lifecycle;

    /// <summary>Gets the stabilizing remedy recipe the station is authored for, if initialized.</summary>
    public IngredientPreparationRecipe? Recipe => lifecycle?.Recipe;

    /// <summary>Gets the current station stock, if initialized.</summary>
    public IngredientStateSnapshot? Ingredients => lifecycle?.Ingredients;

    /// <summary>Gets the current preparation activity, if started.</summary>
    public IngredientPreparationActivity? Activity => lifecycle?.Activity;

    /// <summary>Gets the completed preparation outcome, if completed.</summary>
    public IngredientPreparationOutcome? Outcome => lifecycle?.Outcome;

    /// <summary>Initializes once with the scenario's station stock and preparable recipes.</summary>
    public bool TryInitialize(FirstPatientScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        if (lifecycle is not null)
        {
            return false;
        }

        lifecycle = new(scenario);
        return true;
    }

    /// <summary>Starts a preparable recipe from the station stock, plus one gathered herb when given.</summary>
    public bool TryStart(IngredientPreparationRecipe recipe, IngredientState? gathered = null) =>
        lifecycle?.TryStart(recipe, gathered) == true;

    /// <summary>Advances only the exact next step of the current preparation.</summary>
    public bool TryAdvance(IngredientPreparationStep completedStep) =>
        lifecycle?.TryAdvance(completedStep) == true;

    /// <summary>Completes the exact fully progressed activity once.</summary>
    public bool TryComplete() => lifecycle?.TryComplete() == true;

    /// <summary>Returns to idle after a wrong remedy has been given.</summary>
    public bool TryRestartAfterFailedDose(IngredientExperimentJournalSnapshot administrations) =>
        lifecycle?.TryRestartAfterFailedDose(administrations) == true;

    /// <summary>Reconstructs transient preparation state from durable first-patient state.</summary>
    internal void InstallRestored(FirstPatientSaveState state) => PrepareRestoration(state)();

    internal Action PrepareRestoration(FirstPatientSaveState state) =>
        (lifecycle ?? throw new InvalidOperationException(
            "Preparation restoration requires initialized authored ingredients."))
        .PrepareRestoration(state);
}
