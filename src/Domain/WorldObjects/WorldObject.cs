namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Composes the complete immutable authored world-object model without defining runtime behavior.
/// </summary>
public sealed class WorldObject
{
    /// <summary>
    /// Creates an immutable world object from its existing validated components.
    /// </summary>
    public WorldObject(WorldObjectIdentity identity, WorldObjectDescription description)
    {
        ArgumentNullException.ThrowIfNull(identity);
        ArgumentNullException.ThrowIfNull(description);

        Identity = identity;
        Description = description;
    }

    /// <summary>
    /// Gets the world object's stable identity.
    /// </summary>
    public WorldObjectIdentity Identity { get; }

    /// <summary>
    /// Gets the world object's authoritative authored description.
    /// </summary>
    public WorldObjectDescription Description { get; }
}
