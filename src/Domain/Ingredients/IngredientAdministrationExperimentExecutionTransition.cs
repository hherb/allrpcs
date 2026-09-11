using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Consumes and records one ingredient-administration experiment atomically.</summary>
public static class IngredientAdministrationExperimentExecutionTransition
{
    /// <summary>Returns exact resulting batch and journal state for one eligible administration.</summary>
    public static IngredientAdministrationExperimentExecutionResult Execute(
        ExperimentId experimentId,
        IngredientAdministrationOutcome administration,
        CharacterConditionSet observedConditions,
        string procedure,
        string observedOutcome,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        PreparedIngredientBatchSnapshot preparedBatches,
        IngredientExperimentJournalSnapshot administrationExperiments,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        if (!PreparedIngredientBatchSnapshotConsumptionPolicy.CanConsume(
                administration.Dosage,
                preparedBatches)
            || !IngredientAdministrationExperimentRecordingPolicy.CanRecord(
                administration,
                worldEvents,
                eventLog)
            || !ExperimentRecordingPolicy.CanRecord(
                experimentId,
                administration.WorldEvent,
                worldEvents,
                eventLog,
                experiments))
        {
            throw new InvalidOperationException(
                "The administration cannot be consumed and recorded in both journals.");
        }

        _ = new ExperimentJournalRecord(
            experimentId,
            administration.WorldEvent,
            procedure,
            observedOutcome);
        PreparedIngredientBatchSnapshot resultingBatches =
            PreparedIngredientBatchSnapshotConsumptionTransition.Consume(
                administration.Dosage,
                preparedBatches);
        IngredientExperimentJournalSnapshot resultingAdministrationExperiments =
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId,
                administration,
                observedConditions,
                worldEvents,
                eventLog,
                administrationExperiments);
        ExperimentJournalSnapshot resultingExperiments = ExperimentRecordingTransition.Record(
            experimentId,
            administration.WorldEvent,
            procedure,
            observedOutcome,
            worldEvents,
            eventLog,
            experiments);

        return new IngredientAdministrationExperimentExecutionResult(
            resultingBatches,
            resultingAdministrationExperiments,
            resultingExperiments);
    }
}
