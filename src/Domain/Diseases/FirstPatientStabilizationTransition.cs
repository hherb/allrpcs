using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Diseases;

/// <summary>Atomically stabilizes the first patient and records the observed experiment.</summary>
public static class FirstPatientStabilizationTransition
{
    /// <summary>Returns exact new disease and journal state for one eligible administration.</summary>
    public static FirstPatientStabilizationResult Apply(
        ExperimentId experimentId,
        FirstPatientTreatmentContext context,
        IngredientAdministrationOutcome administration,
        CharacterConditionSet observedConditions,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        IngredientExperimentJournalSnapshot experimentJournal)
    {
        ArgumentNullException.ThrowIfNull(experimentId);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(observedConditions);
        ArgumentNullException.ThrowIfNull(worldEvents);
        ArgumentNullException.ThrowIfNull(eventLog);
        ArgumentNullException.ThrowIfNull(experimentJournal);

        if (!StabilizingRemedyAdministrationPolicy.CanStabilize(
            context,
            administration,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException(
                "The supplied administration cannot stabilize the first patient.");
        }

        IngredientExperimentJournalSnapshot resultingJournal =
            IngredientAdministrationExperimentRecordingTransition.Record(
                experimentId,
                administration,
                observedConditions,
                worldEvents,
                eventLog,
                experimentJournal);
        DiseaseTruthSnapshot resultingTruth = StabilizingRemedyAdministrationTransition.Apply(
            context,
            administration,
            worldEvents,
            eventLog);

        return new FirstPatientStabilizationResult(resultingTruth, resultingJournal);
    }
}
