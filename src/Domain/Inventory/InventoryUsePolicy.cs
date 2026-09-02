using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Decides whether a character may use one existing world object under recorded inventory
/// ownership.
/// </summary>
public static class InventoryUsePolicy
{
    /// <summary>
    /// Determines whether the character may use the object. The object is eligible exactly when
    /// the ownership snapshot records that character as its owner.
    /// </summary>
    public static bool CanUseObject(
        CharacterId characterId,
        ObjectId objectId,
        InventoryOwnershipSnapshot ownerships)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(ownerships);

        return ownerships.Entries.Any(
            entry => entry.ObjectId == objectId
                && entry.OwnerCharacterId == characterId);
    }
}
