using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Decides whether a character may take one existing world object under recorded inventory
/// ownership and capacity snapshots.
/// </summary>
public static class InventoryTakePolicy
{
    /// <summary>
    /// Determines whether the character may take the object. An already-owned object is
    /// ineligible; an unowned object is eligible only when the character may own one additional
    /// object.
    /// </summary>
    public static bool CanTakeObject(
        CharacterId characterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships,
        InventoryCapacitySnapshot capacities)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(ownerships);
        ArgumentNullException.ThrowIfNull(capacities);

        bool isAlreadyOwned = ownerships.Entries.Any(entry => entry.ObjectId == objectId);

        return !isAlreadyOwned
            && InventoryCapacityPolicy.CanOwnAdditionalObject(
                characterId,
                ownerships,
                capacities);
    }
}
