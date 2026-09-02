using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Decides whether a character may own one additional distinct object under recorded inventory
/// ownership and capacity snapshots.
/// </summary>
public static class InventoryCapacityPolicy
{
    /// <summary>
    /// Determines whether the character may own one additional distinct object. A character with
    /// no recorded capacity fails closed.
    /// </summary>
    public static bool CanOwnAdditionalObject(
        CharacterId characterId,
        InventoryOwnershipSnapshot ownerships,
        InventoryCapacitySnapshot capacities)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(ownerships);
        ArgumentNullException.ThrowIfNull(capacities);

        InventoryCapacity? capacity = capacities.Entries.FirstOrDefault(
            entry => entry.CharacterId == characterId);

        if (capacity is null)
        {
            return false;
        }

        int ownedObjectCount = ownerships.Entries.Count(
            entry => entry.OwnerCharacterId == characterId);

        return ownedObjectCount < capacity.MaximumObjectCount;
    }
}
