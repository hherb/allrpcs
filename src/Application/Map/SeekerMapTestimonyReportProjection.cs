using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Reports an explicitly chosen location from exact already-safe heard testimony.</summary>
public static class SeekerMapTestimonyReportProjection
{
    /// <summary>Returns a fresh map after validating exact safe testimony and map provenance.</summary>
    public static SeekerMapSnapshot Project(
        OpeningRegionMapLocationCatalogue catalogue,
        SeekerMapSnapshot map,
        JournalTestimonySectionSnapshot testimony,
        CharacterId seekerId,
        JournalTestimonyAuthoredCopy sourceEntry,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(testimony);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(sourceEntry);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(map.Catalogue, catalogue)
            || !ReferenceEquals(map.SeekerId, seekerId)
            || !ReferenceEquals(testimony.SeekerId, seekerId)
            || !testimony.Entries.Any(entry => ReferenceEquals(entry, sourceEntry)))
        {
            throw new ArgumentException(
                "Map testimony reports require exact map-owner and safe-entry provenance.",
                nameof(testimony));
        }

        return SeekerMapSnapshotTransition.Report(catalogue, map, seekerId, location);
    }
}
