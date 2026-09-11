using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Ingredients;

/// <summary>Contains bounded ingredient-administration experiment records in authored order.</summary>
public sealed class IngredientExperimentJournalSnapshot
{
    /// <summary>Gets the maximum number of records in one journal snapshot.</summary>
    public const int MaximumRecordCount = 256;

    /// <summary>Creates an immutable copy of supplied experiment records.</summary>
    public IngredientExperimentJournalSnapshot(
        IEnumerable<IngredientAdministrationExperimentRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        List<IngredientAdministrationExperimentRecord> retainedRecords = [];
        HashSet<ExperimentId> experimentIds = [];
        foreach (IngredientAdministrationExperimentRecord? record in records)
        {
            if (record is null)
            {
                throw new ArgumentException("Experiment records cannot contain null.", nameof(records));
            }

            if (!experimentIds.Add(record.ExperimentId))
            {
                throw new ArgumentException(
                    "Experiment records cannot contain duplicate identities.",
                    nameof(records));
            }

            retainedRecords.Add(record);
            if (retainedRecords.Count > MaximumRecordCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(records),
                    $"An experiment journal accepts at most {MaximumRecordCount} records.");
            }
        }

        Records = retainedRecords.AsReadOnly();
    }

    /// <summary>Gets exact experiment records in stable authored order.</summary>
    public IReadOnlyList<IngredientAdministrationExperimentRecord> Records { get; }
}
