using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Decides whether one focused-search request may start under the active searches and authored
/// discovery conditions.
/// </summary>
public static class FocusedSearchEligibilityPolicy
{
    /// <summary>
    /// Determines whether the request may start and its target's discovery conditions are satisfied.
    /// </summary>
    public static bool CanStartSearch(
        FocusedSearchRequest request,
        FocusedSearchActivitySnapshot activities,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(activities);
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(inventory);

        return FocusedSearchStartPolicy.CanStartSearch(request, activities) &&
            DiscoveryConditionPolicy.IsSatisfied(
                request.TargetObjectId,
                conditions,
                capabilities,
                inventory);
    }
}
