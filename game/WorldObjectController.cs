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
}
