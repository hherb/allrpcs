using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>Checks explicit physical access without deriving it from perception or position.</summary>
public static class CharacterPhysicalAccessPolicy
{
    /// <summary>Returns whether the value-matching character can access the value-matching object.</summary>
    public static bool CanAccess(
        CharacterPhysicalAccessSnapshot snapshot,
        CharacterId characterId,
        ObjectId objectId)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(objectId);

        return snapshot.CharacterId == characterId
            && snapshot.AccessibleObjectIds.Contains(objectId);
    }
}
