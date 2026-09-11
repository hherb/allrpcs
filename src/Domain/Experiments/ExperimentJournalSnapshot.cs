using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Experiments;

/// <summary>Contains bounded universal experiment records in stable authored order.</summary>
public sealed class ExperimentJournalSnapshot
{
    /// <summary>Gets the maximum number of experiment records in one journal.</summary>
    public const int MaximumRecordCount = 256;

    /// <summary>Creates an immutable copy of distinct supplied experiment records.</summary>
    public ExperimentJournalSnapshot(IEnumerable<ExperimentJournalRecord> records)
    {
        ArgumentNullException.ThrowIfNull(records);

        List<ExperimentJournalRecord> retainedRecords = [];
        HashSet<ExperimentId> experimentIds = [];
        HashSet<WorldEventId> eventIds = [];
        foreach (ExperimentJournalRecord? record in records)
        {
            if (record is null)
            {
                throw new ArgumentException(
                    "Experiment journal records cannot contain null.",
                    nameof(records));
            }

            if (!experimentIds.Add(record.ExperimentId))
            {
                throw new ArgumentException(
                    "Experiment journal records cannot contain duplicate experiment identities.",
                    nameof(records));
            }

            if (!eventIds.Add(record.WorldEvent.EventId))
            {
                throw new ArgumentException(
                    "Experiment journal records cannot contain duplicate event identities.",
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
    public IReadOnlyList<ExperimentJournalRecord> Records { get; }
}
