using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Creates the initial empty player-note state for one exact Seeker map.</summary>
public static class SeekerMapNoteSnapshotProjection
{
    /// <summary>Returns one fresh empty map-note snapshot.</summary>
    public static SeekerMapNoteSnapshot CreateEmpty(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(seekerId);

        return new SeekerMapNoteSnapshot(catalogue, seekerId, []);
    }
}
