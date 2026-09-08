using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Persistence;

/// <summary>Composes the complete pure restoration decision for an immutable save root.</summary>
public static class SaveSnapshotRestorationPolicy
{
    /// <summary>
    /// Returns whether the versioned save state can restore against authoritative definitions.
    /// </summary>
    public static bool CanRestore(
        SaveSnapshot snapshot,
        CharacterId searcherCharacterId,
        WorldObjectSnapshot worldObjects,
        WorldEventSnapshot worldEvents)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(searcherCharacterId);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(worldEvents);

        return SaveSnapshotLoadPolicy.CanLoad(snapshot)
            && DiscoverySaveStateRestorationPolicy.CanRestore(
                snapshot.Discovery,
                searcherCharacterId,
                worldObjects)
            && WorldEventSaveStateRestorationPolicy.CanRestore(
                snapshot.WorldEvents,
                worldEvents);
    }
}
