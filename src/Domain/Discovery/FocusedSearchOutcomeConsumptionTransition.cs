namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Removes one eligible focused-search outcome from an outcome snapshot.
/// </summary>
public static class FocusedSearchOutcomeConsumptionTransition
{
    /// <summary>
    /// Returns a new snapshot without the retained outcome for the supplied exact request.
    /// </summary>
    public static FocusedSearchOutcomeSnapshot Consume(
        FocusedSearchOutcome outcome,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        if (!FocusedSearchOutcomeConsumptionPolicy.CanConsume(outcome, outcomes))
        {
            throw new InvalidOperationException(
                "Only an exactly retained focused-search outcome may be consumed.");
        }

        return new FocusedSearchOutcomeSnapshot(
            outcomes.Entries.Where(entry => entry.Request != outcome.Request));
    }
}
