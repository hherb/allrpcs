using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Inventory;

namespace Allrpcs.Application.Inventory;

/// <summary>
/// Applies one Take, Give or Use to the single inventory-ownership snapshot through the Domain
/// policies. Each operation returns the next snapshot, or <c>null</c> when its policy refuses.
/// </summary>
public static class InventoryExchangeTransition
{
    /// <summary>Records an unowned object as owned by the taking character.</summary>
    public static InventoryOwnershipSnapshot? Take(
        CharacterId characterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships,
        InventoryCapacitySnapshot capacities) =>
        InventoryTakePolicy.CanTakeObject(characterId, objectId, ownerships, capacities)
            ? new([.. ownerships.Entries, new InventoryOwnership(objectId, characterId)])
            : null;

    /// <summary>Moves an owned object to a distinct recipient, keeping its position in order.</summary>
    public static InventoryOwnershipSnapshot? Give(
        CharacterId giverCharacterId,
        CharacterId recipientCharacterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships,
        InventoryCapacitySnapshot capacities) =>
        InventoryGivePolicy.CanGiveObject(
            giverCharacterId,
            recipientCharacterId,
            objectId,
            ownerships,
            capacities)
            ? new(ownerships.Entries.Select(entry => entry.ObjectId == objectId
                ? new InventoryOwnership(objectId, recipientCharacterId)
                : entry))
            : null;

    /// <summary>Uses up an owned object, such as an ingredient that went into a preparation.</summary>
    public static InventoryOwnershipSnapshot? Use(
        CharacterId characterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships) =>
        InventoryUsePolicy.CanUseObject(characterId, objectId, ownerships)
            ? new(ownerships.Entries.Where(entry => entry.ObjectId != objectId))
            : null;
}
