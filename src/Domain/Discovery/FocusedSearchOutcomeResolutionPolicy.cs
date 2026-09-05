using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search outcome can change perception and then be consumed without
/// changing either input snapshot.
/// </summary>
public static class FocusedSearchOutcomeResolutionPolicy
{
    /// <summary>
    /// Returns true when the outcome would change the supplied perception and the complete outcome
    /// is retained for consumption.
    /// </summary>
    public static bool CanResolve(
        FocusedSearchOutcome outcome,
        CharacterPerceptionSnapshot perception,
        FocusedSearchOutcomeSnapshot outcomes) =>
        FocusedSearchOutcomeChangePolicy.WouldChange(outcome, perception)
        && FocusedSearchOutcomeConsumptionPolicy.CanConsume(outcome, outcomes);
}
