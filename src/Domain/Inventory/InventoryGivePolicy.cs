using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Decides whether one character may give one existing world object to another character under
/// recorded inventory ownership and capacity snapshots.
/// </summary>
public static class InventoryGivePolicy
{
    /// <summary>
    /// Determines whether the giver may give the object to the distinct recipient. The giver must
    /// own the object, and the recipient must be able to own one additional object.
    /// </summary>
    public static bool CanGiveObject(
        CharacterId giverCharacterId,
        CharacterId recipientCharacterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships,
        InventoryCapacitySnapshot capacities)
    {
        ArgumentNullException.ThrowIfNull(giverCharacterId);
        ArgumentNullException.ThrowIfNull(recipientCharacterId);
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(ownerships);
        ArgumentNullException.ThrowIfNull(capacities);

        return giverCharacterId != recipientCharacterId
            && InventoryDropPolicy.CanDropObject(
                giverCharacterId,
                objectId,
                ownerships)
            && InventoryCapacityPolicy.CanOwnAdditionalObject(
                recipientCharacterId,
                ownerships,
                capacities);
    }
}
