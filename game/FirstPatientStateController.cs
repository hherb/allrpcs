using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Persistence;
using Allrpcs.Domain.WorldEvents;
using Godot;

namespace Allrpcs.Game;

/// <summary>Owns the scene's exact first-patient disease, batch, and experiment state.</summary>
public partial class FirstPatientStateController : Node
{
    /// <summary>Gets the initialized immutable disease truth, if any.</summary>
    public DiseaseTruthSnapshot? DiseaseTruth { get; private set; }

    /// <summary>Gets the initialized immutable prepared batches, if any.</summary>
    public PreparedIngredientBatchSnapshot? PreparedBatches { get; private set; }

    /// <summary>Gets the initialized immutable administration-specific experiments, if any.</summary>
    public IngredientExperimentJournalSnapshot? AdministrationExperiments { get; private set; }

    /// <summary>Gets the initialized immutable universal experiments, if any.</summary>
    public ExperimentJournalSnapshot? Experiments { get; private set; }

    /// <summary>Captures one fresh composition of the exact initialized snapshots.</summary>
    public FirstPatientSaveState? Capture()
    {
        DiseaseTruthSnapshot? diseaseTruth = DiseaseTruth;
        PreparedIngredientBatchSnapshot? preparedBatches = PreparedBatches;
        IngredientExperimentJournalSnapshot? administrationExperiments = AdministrationExperiments;
        ExperimentJournalSnapshot? experiments = Experiments;
        if (diseaseTruth is null
            || preparedBatches is null
            || administrationExperiments is null
            || experiments is null)
        {
            return null;
        }

        return new FirstPatientSaveState(
            diseaseTruth,
            preparedBatches,
            administrationExperiments,
            experiments);
    }

    /// <summary>Rebinds and atomically installs complete compatible first-patient save state.</summary>
    public bool TryRestore(
        FirstPatientSaveState saved,
        FirstPatientTreatmentContext authoredContext,
        WorldEventSnapshot authoredWorldEvents)
    {
        ArgumentNullException.ThrowIfNull(saved);
        ArgumentNullException.ThrowIfNull(authoredContext);
        ArgumentNullException.ThrowIfNull(authoredWorldEvents);

        FirstPatientSaveState rebound;
        try
        {
            rebound = FirstPatientSaveStateRestorationTransition.Rebind(
                saved,
                authoredContext,
                authoredWorldEvents);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        DiseaseTruth = rebound.DiseaseTruth;
        PreparedBatches = rebound.PreparedBatches;
        AdministrationExperiments = rebound.AdministrationExperiments;
        Experiments = rebound.Experiments;
        return true;
    }

    /// <summary>Initializes all four state areas once from exact existing snapshots.</summary>
    public bool TryInitialize(
        DiseaseTruthSnapshot diseaseTruth,
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        if (DiseaseTruth is not null
            || PreparedBatches is not null
            || AdministrationExperiments is not null
            || Experiments is not null)
        {
            return false;
        }

        DiseaseTruth = diseaseTruth;
        PreparedBatches = preparedBatches;
        AdministrationExperiments = administrationExperiments;
        Experiments = experiments;
        return true;
    }

    /// <summary>Adds one completed preparable recipe (the remedy or a wrong one) as a full batch.</summary>
    public bool TryAddPreparedBatch(
        FirstPatientTreatmentContext context,
        IngredientPreparationOutcome preparation)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(preparation);

        DiseaseTruthSnapshot? diseaseTruth = DiseaseTruth;
        PreparedIngredientBatchSnapshot? preparedBatches = PreparedBatches;
        if (diseaseTruth is null
            || preparedBatches is null
            || !context.Scenario.IsPreparable(preparation.Activity.Recipe))
        {
            return false;
        }

        try
        {
            PreparedBatches = PreparedIngredientBatchSnapshotAppendTransition.Append(
                preparation,
                preparedBatches);
            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    /// <summary>Applies one eligible first-patient stabilization and installs its exact result.</summary>
    public bool TryStabilize(
        ExperimentId experimentId,
        FirstPatientTreatmentContext context,
        IngredientAdministrationOutcome administration,
        CharacterConditionSet observedConditions,
        string procedure,
        string observedOutcome,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);

        DiseaseTruthSnapshot? diseaseTruth = DiseaseTruth;
        PreparedIngredientBatchSnapshot? preparedBatches = PreparedBatches;
        IngredientExperimentJournalSnapshot? administrationExperiments = AdministrationExperiments;
        ExperimentJournalSnapshot? experiments = Experiments;
        if (diseaseTruth is null
            || preparedBatches is null
            || administrationExperiments is null
            || experiments is null)
        {
            return false;
        }

        // The patient may have worsened since the authored scenario; treat the live patient.
        context = context.WithDiseaseTruth(diseaseTruth);
        FirstPatientStabilizationResult result;
        try
        {
            result = FirstPatientStabilizationTransition.Apply(
                experimentId,
                context,
                administration,
                observedConditions,
                procedure,
                observedOutcome,
                worldEvents,
                eventLog,
                preparedBatches,
                administrationExperiments,
                experiments);
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }

        DiseaseTruth = result.DiseaseTruth;
        PreparedBatches = result.PreparedBatches;
        AdministrationExperiments = result.AdministrationExperiments;
        Experiments = result.Experiments;
        return true;
    }

    internal void InstallAdministrationOccurrence(
        FirstPatientAdministrationOccurrenceResult result)
    {
        DiseaseTruth = result.DiseaseTruth;
        PreparedBatches = result.PreparedBatches;
        AdministrationExperiments = result.AdministrationExperiments;
        Experiments = result.Experiments;
    }

    /// <summary>Installs the patient's worsened disease truth from the progression schedule.</summary>
    internal void InstallDiseaseTruth(DiseaseTruthSnapshot diseaseTruth)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);
        DiseaseTruth = diseaseTruth;
    }

    internal void Install(FirstPatientSaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        DiseaseTruth = state.DiseaseTruth;
        PreparedBatches = state.PreparedBatches;
        AdministrationExperiments = state.AdministrationExperiments;
        Experiments = state.Experiments;
    }
}
