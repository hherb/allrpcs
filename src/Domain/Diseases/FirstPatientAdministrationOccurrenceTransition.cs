using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>
/// Atomically occurs one authored administration: the remedy stabilizes the first patient, and an
/// alternative (wrong) preparation is recorded as an experiment that leaves the patient unchanged.
/// </summary>
public static class FirstPatientAdministrationOccurrenceTransition
{
    /// <summary>Returns exact world-event and first-patient state after one eligible dose.</summary>
    public static FirstPatientAdministrationOccurrenceResult Occur(
        ExperimentId experimentId,
        FirstPatientTreatmentContext context,
        CharacterId administratorId,
        WorldEvent worldEvent,
        PreparedIngredientBatch batch,
        IngredientQuantity dosageQuantity,
        CharacterConditionSet observedConditions,
        string procedure,
        string observedOutcome,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        WorldTimeSnapshot worldTime,
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administratorId);
        ArgumentNullException.ThrowIfNull(worldEvent);
        ArgumentNullException.ThrowIfNull(batch);
        ArgumentNullException.ThrowIfNull(dosageQuantity);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(worldTime);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        WorldEvent? authoritativeEvent = WorldEventLookup.FindById(
            worldEvent.EventId,
            worldEvents);
        bool retainsExactBatch = preparedBatches.Batches.Any(
            candidate => ReferenceEquals(candidate, batch));
        if (!ReferenceEquals(authoritativeEvent, worldEvent) || !retainsExactBatch)
        {
            throw new InvalidOperationException(
                "The exact authored event and prepared batch must be retained.");
        }

        WorldEventOccurrenceResult occurrence = WorldEventOccurrenceTransition.Occur(
            worldEvent.EventId,
            worldEvents,
            eventLog,
            worldTime);
        IngredientDosage dosage = new(batch.Preparation, dosageQuantity);
        IngredientAdministrationOutcome administration = new(
            administratorId,
            context.Scenario.PatientId,
            dosage,
            worldEvent);
        IngredientPreparationRecipe recipe = batch.Preparation.Activity.Recipe;
        if (!ReferenceEquals(recipe, context.Scenario.Remedy.Recipe)
            && context.Scenario.IsPreparable(recipe))
        {
            // A wrong remedy is still an experiment: it is recorded and changes no disease truth.
            IngredientAdministrationExperimentExecutionResult experiment =
                IngredientAdministrationExperimentExecutionTransition.Execute(
                    experimentId,
                    administration,
                    observedConditions,
                    procedure,
                    observedOutcome,
                    worldEvents,
                    occurrence.EventLog,
                    preparedBatches,
                    administrationExperiments,
                    experiments);
            return new FirstPatientAdministrationOccurrenceResult(
                occurrence.EventLog,
                occurrence.WorldTime,
                context.Scenario.DiseaseTruth,
                experiment.PreparedBatches,
                experiment.AdministrationExperiments,
                experiment.Experiments);
        }

        FirstPatientStabilizationResult stabilization = FirstPatientStabilizationTransition.Apply(
            experimentId,
            context,
            administration,
            observedConditions,
            procedure,
            observedOutcome,
            worldEvents,
            occurrence.EventLog,
            preparedBatches,
            administrationExperiments,
            experiments);

        return new FirstPatientAdministrationOccurrenceResult(
            occurrence.EventLog,
            occurrence.WorldTime,
            stabilization.DiseaseTruth,
            stabilization.PreparedBatches,
            stabilization.AdministrationExperiments,
            stabilization.Experiments);
    }
}
