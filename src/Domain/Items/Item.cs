using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Items;

/// <summary>
/// Classifies an existing immutable world object as an item without defining item behavior.
/// </summary>
public sealed class Item
{
    /// <summary>
    /// Creates an immutable item classification for an existing world object.
    /// </summary>
    public Item(WorldObject worldObject)
    {
        ArgumentNullException.ThrowIfNull(worldObject);

        WorldObject = worldObject;
    }

    /// <summary>
    /// Gets the existing world object classified as an item.
    /// </summary>
    public WorldObject WorldObject { get; }
}
