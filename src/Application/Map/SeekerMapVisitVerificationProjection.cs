using Allrpcs.Application.Navigation;

namespace Allrpcs.Application.Map;

/// <summary>Verifies one exact canonical map location from a coordinate-free safe visit.</summary>
public static class SeekerMapVisitVerificationProjection
{
    /// <summary>Returns a fresh map only for exact map, projection, owner, and location provenance.</summary>
    public static SeekerMapSnapshot Project(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        OpeningRegionProjection projection,
        OpeningRegionLocationVisit visit,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentNullException.ThrowIfNull(visit);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(map.Catalogue, catalogue)
            || !ReferenceEquals(visit.Projection, projection)
            || !ReferenceEquals(visit.SeekerId, map.SeekerId)
            || visit.LocationId != location.Id)
        {
            throw new ArgumentException(
                "Map verification requires exact visit and map provenance for one location.",
                nameof(visit));
        }

        return SeekerMapSnapshotTransition.Verify(
            catalogue,
            map,
            map.SeekerId,
            location);
    }
}
