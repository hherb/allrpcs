using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Atomically consumes a prepared dose, stabilizes the patient, and records it.</summary>
public static class FirstPatientStabilizationTransition
{
    /// <summary>Returns exact new disease, batch, and journal state for one eligible administration.</summary>
    public static FirstPatientStabilizationResult Apply(
        ExperimentId experimentId,
        FirstPatientTreatmentContext context,
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
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(procedure);
        ArgumentNullException.ThrowIfNull(observedOutcome);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(preparedBatches);
        ArgumentNullException.ThrowIfNull(administrationExperiments);
        ArgumentNullException.ThrowIfNull(experiments);

        if (!StabilizingRemedyAdministrationPolicy.CanStabilize(
            context,
            administration,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException(
                "The supplied administration cannot stabilize the first patient.");
        }

        IngredientAdministrationExperimentExecutionResult experimentResult =
            IngredientAdministrationExperimentExecutionTransition.Execute(
                experimentId,
                administration,
                observedConditions,
                procedure,
                observedOutcome,
                worldEvents,
                eventLog,
                preparedBatches,
                administrationExperiments,
                experiments);
        DiseaseTruthSnapshot resultingTruth = StabilizingRemedyAdministrationTransition.Apply(
            context,
            administration,
            worldEvents,
            eventLog);

        return new FirstPatientStabilizationResult(
            resultingTruth,
            experimentResult.PreparedBatches,
            experimentResult.AdministrationExperiments,
            experimentResult.Experiments);
    }
}
