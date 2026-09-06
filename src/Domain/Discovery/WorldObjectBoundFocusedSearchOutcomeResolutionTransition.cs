using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Resolves one eligible focused-search outcome against its existing world object.
/// </summary>
public static class WorldObjectBoundFocusedSearchOutcomeResolutionTransition
{
    /// <summary>
    /// Returns the exact existing resolution result after validating the supplied world object.
    /// </summary>
    public static FocusedSearchOutcomeResolutionResult Resolve(
        FocusedSearchOutcome outcome,
        WorldObject worldObject,
        CharacterPerceptionSnapshot perception,
        FocusedSearchOutcomeSnapshot outcomes)
    {
        if (!WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve(
                outcome,
                worldObject,
                perception,
                outcomes))
        {
            throw new InvalidOperationException(
                "Only a world-object-bound perception-changing retained focused-search outcome "
                + "may be resolved.");
        }

        return FocusedSearchOutcomeResolutionTransition.Resolve(
            outcome,
            perception,
            outcomes);
    }
}
