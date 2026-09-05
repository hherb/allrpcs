namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused search can be completed with an outcome without changing activity
/// or outcome state.
/// </summary>
public static class FocusedSearchOutcomeCompletionPolicy
{
    /// <summary>
    /// Returns true when the outcome's request is active and has no recorded outcome.
    /// </summary>
    public static bool CanCompleteWithOutcome(
        FocusedSearchOutcome outcome,
        FocusedSearchActivitySnapshot activities,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(outcomes);

        return FocusedSearchCompletionPolicy.CanCompleteSearch(outcome.Request, activities)
            && FocusedSearchOutcomeRecordingPolicy.CanRecord(outcome, outcomes);
    }
}
