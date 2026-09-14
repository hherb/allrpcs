using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>Binds one exact authoritative world object to one exact authored location.</summary>
public sealed class WorldObjectPlacement
{
    /// <summary>Creates one immutable world-object placement without changing scene state.</summary>
    public WorldObjectPlacement(WorldObject worldObject, Location location)
    {
        ArgumentNullException.ThrowIfNull(worldObject);
        ArgumentNullException.ThrowIfNull(location);

        WorldObject = worldObject;
        Location = location;
    }

    /// <summary>Gets the exact authoritative world object.</summary>
    public WorldObject WorldObject { get; }

    /// <summary>Gets the exact authored location.</summary>
    public Location Location { get; }
}
