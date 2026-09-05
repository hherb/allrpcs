using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Perception;

/// <summary>
/// Selects a world-object visibility entry by object identifier without changing perception.
/// </summary>
public static class CharacterPerceptionLookup
{
    /// <summary>
    /// Returns the exact visibility entry for a value-equal object identifier, or null when no
    /// entry matches.
    /// </summary>
    public static WorldObjectVisibility? FindByObject(
        ObjectId objectId,
        CharacterPerceptionSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(objectId);
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.Entries.FirstOrDefault(entry => entry.ObjectId == objectId);
    }
}
