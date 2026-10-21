using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Application.Inventory;

/// <summary>Decides whether a saved inventory fits the authored world it is loaded into.</summary>
public static class InventoryRestorationPolicy
{
    /// <summary>
    /// Accepts only known objects carried by known characters within their limits, and requires
    /// every authored starting object to still be carried by someone (none can be dropped yet).
    /// </summary>
    public static bool CanRestore(
        InventoryOwnershipSnapshot saved,
        InventoryOwnershipSnapshot start,
        IReadOnlyCollection<ObjectId> gatherable,
        InventoryCapacitySnapshot capacities)
    {
        ArgumentNullException.ThrowIfNull(saved);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(gatherable);
        ArgumentNullException.ThrowIfNull(capacities);

        return saved.Entries.All(entry =>
                (gatherable.Contains(entry.ObjectId)
                    || start.Entries.Any(authored => authored.ObjectId == entry.ObjectId))
                && capacities.Entries.SingleOrDefault(capacity => capacity.CharacterId == entry.OwnerCharacterId)
                    is { } capacity
                && saved.Entries.Count(other => other.OwnerCharacterId == entry.OwnerCharacterId)
                    <= capacity.MaximumObjectCount)
            && start.Entries.All(authored => saved.Entries.Any(entry => entry.ObjectId == authored.ObjectId));
    }
}
