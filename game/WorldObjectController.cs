using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>
/// Owns the scene's initialized immutable world-object snapshot.
/// </summary>
public partial class WorldObjectController : Node
{
    /// <summary>Gets the initialized immutable world-object snapshot, if any.</summary>
    public WorldObjectSnapshot? WorldObjects { get; private set; }

    /// <summary>Initializes this controller once from an existing world-object snapshot.</summary>
    public bool TryInitializeWorldObjects(WorldObjectSnapshot worldObjects)
    {
        ArgumentNullException.ThrowIfNull(worldObjects);

        if (WorldObjects is not null)
        {
            return false;
        }

        WorldObjects = worldObjects;
        return true;
    }

    /// <summary>Finds the exact initialized world object with the supplied identifier.</summary>
    public WorldObject? FindWorldObject(ObjectId objectId)
    {
        ArgumentNullException.ThrowIfNull(objectId);

        WorldObjectSnapshot? worldObjects = WorldObjects;
        return worldObjects is null
            ? null
            : WorldObjectLookup.FindById(objectId, worldObjects);
    }
}
