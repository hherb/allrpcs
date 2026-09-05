using Allrpcs.Domain.Perception;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search outcome would change one character perception snapshot
/// without changing perception.
/// </summary>
public static class FocusedSearchOutcomeChangePolicy
{
    /// <summary>
    /// Returns true when the outcome applies and its target has a different visibility state.
    /// </summary>
    public static bool WouldChange(
        FocusedSearchOutcome outcome,
        CharacterPerceptionSnapshot perception)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(perception);

        if (!FocusedSearchOutcomeApplicabilityPolicy.CanApply(outcome, perception))
        {
            return false;
        }

        WorldObjectVisibility target = CharacterPerceptionLookup.FindByObject(
            outcome.Request.TargetObjectId,
            perception)!;

        return target.VisibilityState != outcome.ResultingVisibilityState;
    }
}
