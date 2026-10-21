using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Application.Inventory;

/// <summary>Decides whether a saved inventory fits the authored world it is loaded into.</summary>
public static class InventoryRestorationPolicy
{
    /// <summary>
    /// Accepts only authored starting objects and the gathered objects the saved history says are
    /// still held, each carried by a known character within its limit, and requires every one of
    /// them to be carried by someone, so the inventory agrees with what the history records.
    /// </summary>
    // shortcut: no starting object can be dropped or used up yet, so each must still be carried;
    // upgrade when one can, by deriving the expected starting objects from history too.
    public static bool CanRestore(
        InventoryOwnershipSnapshot saved,
        InventoryOwnershipSnapshot start,
        IReadOnlyCollection<ObjectId> heldGathered,
        InventoryCapacitySnapshot capacities)
    {
        ArgumentNullException.ThrowIfNull(saved);
        ArgumentNullException.ThrowIfNull(start);
        ArgumentNullException.ThrowIfNull(heldGathered);
        ArgumentNullException.ThrowIfNull(capacities);

        IEnumerable<ObjectId> expected = start.Entries.Select(authored => authored.ObjectId).Concat(heldGathered);
        return saved.Entries.All(entry =>
                expected.Contains(entry.ObjectId)
                && capacities.Entries.SingleOrDefault(capacity => capacity.CharacterId == entry.OwnerCharacterId)
                    is { } capacity
                && saved.Entries.Count(other => other.OwnerCharacterId == entry.OwnerCharacterId)
                    <= capacity.MaximumObjectCount)
            && expected.All(objectId => saved.Entries.Any(entry => entry.ObjectId == objectId));
    }
}
