using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Associates a world object with its owning character without defining inventory mechanics or
/// cross-association policy.
/// </summary>
public sealed record InventoryOwnership
{
    /// <summary>
    /// Creates an immutable inventory-ownership association.
    /// </summary>
    public InventoryOwnership(ObjectId objectId, CharacterId ownerCharacterId)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(ownerCharacterId);

        ObjectId = objectId;
        OwnerCharacterId = ownerCharacterId;
    }

    /// <summary>
    /// Gets the stable identifier of the owned world object.
    /// </summary>
    public ObjectId ObjectId { get; }

    /// <summary>
    /// Gets the stable identifier of the owning character.
    /// </summary>
    public CharacterId OwnerCharacterId { get; }
}
