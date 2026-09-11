using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Records one authoritative ingredient-administration experiment atomically.</summary>
public static class IngredientAdministrationExperimentRecordingTransition
{
    /// <summary>Constructs and appends one record for an eligible exact administration.</summary>
    public static IngredientExperimentJournalSnapshot Record(
        ExperimentId experimentId,
        IngredientAdministrationOutcome administration,
        CharacterConditionSet observedConditions,
        WorldEventSnapshot worldEvents,
        WorldEventLogSnapshot eventLog,
        IngredientExperimentJournalSnapshot journal)
    {
        if (!IngredientAdministrationExperimentRecordingPolicy.CanRecord(
            administration,
            worldEvents,
            eventLog))
        {
            throw new InvalidOperationException(
                "The supplied administration event is not the latest authoritative occurrence.");
        }

        IngredientAdministrationExperimentRecord record = new(
            experimentId,
            administration,
            observedConditions);
        return IngredientExperimentJournalRecordingTransition.Record(record, journal);
    }
}
