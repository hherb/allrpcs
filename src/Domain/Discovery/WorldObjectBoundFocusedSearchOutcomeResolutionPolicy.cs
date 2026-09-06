using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search outcome can resolve against its existing world object.
/// </summary>
public static class WorldObjectBoundFocusedSearchOutcomeResolutionPolicy
{
    /// <summary>
    /// Returns true when the supplied world object matches the outcome target and the existing
    /// resolution eligibility decision succeeds.
    /// </summary>
    public static bool CanResolve(
        FocusedSearchOutcome outcome,
        WorldObject worldObject,
        CharacterPerceptionSnapshot perception,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        ArgumentNullException.ThrowIfNull(outcome);
        ArgumentNullException.ThrowIfNull(worldObject);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(outcomes);

        return worldObject.Identity.Id == outcome.Request.TargetObjectId
            && FocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                perception,
                outcomes);
    }
}
