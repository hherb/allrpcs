namespace Allrpcs.Domain.Ingredients;

/// <summary>Appends one exact experiment record to immutable journal state.</summary>
public static class IngredientExperimentJournalRecordingTransition
{
    /// <summary>Returns a new journal with the exact record appended once.</summary>
    public static IngredientExperimentJournalSnapshot Record(
        IngredientAdministrationExperimentRecord record,
        IngredientExperimentJournalSnapshot journal)
    {
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(journal);

        return new IngredientExperimentJournalSnapshot(journal.Records.Append(record));
    }
}
