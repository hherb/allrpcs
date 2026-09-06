using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Selects a world object by identifier without changing the snapshot.
/// </summary>
public static class WorldObjectLookup
{
    /// <summary>
    /// Returns the exact world object for a value-equal identifier, or null when no object matches.
    /// </summary>
    public static WorldObject? FindById(ObjectId objectId, WorldObjectSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.Entries.FirstOrDefault(
            worldObject => worldObject.Identity.Id == objectId);
    }
}
