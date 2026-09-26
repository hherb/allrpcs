using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Reports or verifies one exact canonical location on a Seeker-owned map.</summary>
public static class SeekerMapSnapshotTransition
{
    /// <summary>Returns a fresh snapshot with one previously hidden exact location reported.</summary>
    public static SeekerMapSnapshot Report(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot snapshot,
        CharacterId seekerId,
        MapLocationDefinition location)
    {
        ValidateProvenance(catalogue, snapshot, seekerId, location);
        if (Contains(snapshot.ReportedLocations, location)
            || Contains(snapshot.VerifiedLocations, location))
        {
            throw new ArgumentException(
                "Only a hidden map location may be reported.",
                nameof(location));
        }

        return SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            snapshot.ReportedLocations.Append(location),
            snapshot.VerifiedLocations);
    }

    /// <summary>Returns a fresh snapshot with one hidden or reported exact location verified.</summary>
    public static SeekerMapSnapshot Verify(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot snapshot,
        CharacterId seekerId,
        MapLocationDefinition location)
    {
        ValidateProvenance(catalogue, snapshot, seekerId, location);
        if (Contains(snapshot.VerifiedLocations, location))
        {
            throw new ArgumentException(
                "A verified map location cannot be verified again.",
                nameof(location));
        }

        return SeekerMapSnapshotProjection.Project(
            catalogue,
            seekerId,
            snapshot.ReportedLocations.Where(candidate => !ReferenceEquals(candidate, location)),
            snapshot.VerifiedLocations.Append(location));
    }

    private static void ValidateProvenance(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot snapshot,
        CharacterId seekerId,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(snapshot);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(catalogue, snapshot.Catalogue)
            || !ReferenceEquals(seekerId, snapshot.SeekerId)
            || !catalogue.Locations.Any(candidate => ReferenceEquals(candidate, location)))
        {
            throw new ArgumentException(
                "Map transitions require the exact catalogue, owner, and location provenance.",
                nameof(snapshot));
        }
    }

    private static bool Contains(
        IEnumerable<MapLocationDefinition> locations,
        MapLocationDefinition location) => locations.Any(candidate => ReferenceEquals(candidate, location));
}
