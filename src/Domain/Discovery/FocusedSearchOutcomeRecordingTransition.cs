namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Records one eligible focused-search outcome in an outcome snapshot.
/// </summary>
public static class FocusedSearchOutcomeRecordingTransition
{
    /// <summary>
    /// Returns a new snapshot with the supplied outcome appended after all retained outcomes.
    /// </summary>
    public static FocusedSearchOutcomeSnapshot Record(
        FocusedSearchOutcome outcome,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        if (!FocusedSearchOutcomeRecordingPolicy.CanRecord(outcome, outcomes))
        {
            throw new InvalidOperationException(
                "A focused-search outcome for the supplied request is already recorded.");
        }

        return new FocusedSearchOutcomeSnapshot(outcomes.Entries.Append(outcome));
    }
}
