using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Diseases;

/// <summary>
/// Owns the first-patient preparation at the station: one preparation at a time, from the station
/// stock plus any gathered herb the Seeker brings, and a fresh start after a wrong dose.
/// </summary>
public sealed class RemedyPreparationLifecycle
{
    private readonly FirstPatientScenario scenario;

    /// <summary>Creates an idle lifecycle holding only the scenario's station stock.</summary>
    public RemedyPreparationLifecycle(FirstPatientScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        this.scenario = scenario;
        Ingredients = scenario.StationIngredients;
    }

    /// <summary>Gets the stabilizing remedy recipe the station is authored for.</summary>
    public IngredientPreparationRecipe Recipe => scenario.Remedy.Recipe;

    /// <summary>Gets the current station stock.</summary>
    public IngredientStateSnapshot Ingredients { get; private set; }

    /// <summary>Gets the current preparation activity, if started.</summary>
    public IngredientPreparationActivity? Activity { get; private set; }

    /// <summary>Gets the completed preparation outcome, if completed.</summary>
    public IngredientPreparationOutcome? Outcome { get; private set; }

    /// <summary>
    /// Consumes the recipe's inputs from the station stock, plus one gathered herb when given, and
    /// starts the recipe. Only an idle station starts, only a recipe the scenario can prepare, and
    /// a gathered herb only into the remedy (the restore rebuilds no other recipe's herb).
    /// </summary>
    public bool TryStart(IngredientPreparationRecipe recipe, IngredientState? gathered = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (Activity is not null
            || Outcome is not null
            || !scenario.IsPreparable(recipe)
            || (gathered is not null
                && (!ReferenceEquals(recipe, scenario.Remedy.Recipe) || !IsGathered(gathered))))
        {
            return false;
        }

        IngredientStateSnapshot stock = gathered is null
            ? Ingredients
            : new IngredientStateSnapshot([.. Ingredients.States, gathered]);
        try
        {
            IngredientPreparationStartResult result =
                IngredientPreparationStartTransition.Start(recipe, stock);
            Ingredients = result.Ingredients;
            Activity = result.Activity;
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
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

    /// <summary>
    /// Returns the station to idle when the completed preparation was a wrong remedy that has
    /// already been given: only the remedy stabilizes, so that dose changed nothing.
    /// </summary>
    public bool TryRestartAfterFailedDose(IngredientExperimentJournalSnapshot administrations)
    {
        ArgumentNullException.ThrowIfNull(administrations);

        IngredientPreparationOutcome? outcome = Outcome;
        if (outcome is null
            || ReferenceEquals(outcome.Activity.Recipe, scenario.Remedy.Recipe)
            || !administrations.Records.Any(record =>
                ReferenceEquals(record.Administration.Dosage.Preparation, outcome)))
        {
            return false;
        }

        Ingredients = scenario.StationIngredients;
        Activity = null;
        Outcome = null;
        return true;
    }

    /// <summary>
    /// Prepares the station state implied by durable first-patient state: the newest retained
    /// preparation (an unused batch first, else the last dose), or idle. Throws when the retained
    /// preparation is not one this scenario prepares, so the whole restore can be rejected.
    /// </summary>
    public Action PrepareRestoration(FirstPatientSaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        IngredientPreparationOutcome? retained = state.PreparedBatches.Batches.Count > 0
            ? state.PreparedBatches.Batches[^1].Preparation
            : state.AdministrationExperiments.Records.Count > 0
                ? state.AdministrationExperiments.Records[^1].Administration.Dosage.Preparation
                : null;
        if (retained is null)
        {
            return () =>
            {
                Ingredients = scenario.StationIngredients;
                Activity = null;
                Outcome = null;
            };
        }

        IngredientPreparationRecipe recipe = retained.Activity.Recipe;
        if (!scenario.IsPreparable(recipe))
        {
            throw new InvalidOperationException(
                "Restored preparation must retain an authored recipe.");
        }

        IngredientStateSnapshot stock = ReferenceEquals(recipe, scenario.Remedy.Recipe)
            ? new IngredientStateSnapshot(
                [.. scenario.StationIngredients.States, .. scenario.GatheredIngredients.States])
            : scenario.StationIngredients;
        IngredientPreparationStartResult started = IngredientPreparationStartTransition.Start(
            recipe,
            stock);
        return () =>
        {
            Ingredients = started.Ingredients;
            Activity = retained.Activity;
            Outcome = retained;
        };
    }

    // By id, as the stock snapshot rejects duplicates, so an unchecked scenario refuses instead of throwing.
    private bool IsGathered(IngredientState state) =>
        scenario.GatheredIngredients.States.Any(gathered => ReferenceEquals(gathered, state))
        && !Ingredients.States.Any(stocked => stocked.Definition.IngredientId == state.Definition.IngredientId);
}
