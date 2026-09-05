namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search outcome can be recorded without changing recorded outcomes.
/// </summary>
public static class FocusedSearchOutcomeRecordingPolicy
{
    /// <summary>
    /// Returns true when no outcome for the supplied outcome's request is already retained.
    /// </summary>
    public static bool CanRecord(
        FocusedSearchOutcome outcome,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        return FocusedSearchOutcomeLookup.FindByRequest(outcome.Request, outcomes) is null;
    }
}
