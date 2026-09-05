namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one complete focused-search outcome is retained for consumption without
/// changing recorded outcomes.
/// </summary>
public static class FocusedSearchOutcomeConsumptionPolicy
{
    /// <summary>
    /// Returns true when the recorded outcome for the supplied request is value-equal to the
    /// supplied outcome, including its resulting visibility state.
    /// </summary>
    public static bool CanConsume(
        FocusedSearchOutcome outcome,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcome);

        FocusedSearchOutcome? retainedOutcome = FocusedSearchOutcomeLookup.FindByRequest(
            outcome.Request,
            outcomes);

        return retainedOutcome == outcome;
    }
}
