namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Completes one eligible focused search and records its supplied outcome without changing either
/// input snapshot.
/// </summary>
public static class FocusedSearchOutcomeCompletionTransition
{
    /// <summary>
    /// Returns the activity and outcome snapshots produced by the existing completion and recording
    /// transitions.
    /// </summary>
    public static FocusedSearchOutcomeCompletionResult CompleteWithOutcome(
        FocusedSearchOutcome outcome,
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        if (!FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome(
                outcome,
                activities,
                outcomes))
        {
            throw new InvalidOperationException(
                "Only an active focused search without a recorded outcome may be completed.");
        }

        FocusedSearchActivitySnapshot completedActivities =
            FocusedSearchCompletionTransition.CompleteSearch(outcome.Request, activities);
        FocusedSearchOutcomeSnapshot recordedOutcomes =
            FocusedSearchOutcomeRecordingTransition.Record(outcome, outcomes);

        return new FocusedSearchOutcomeCompletionResult(completedActivities, recordedOutcomes);
    }
}
