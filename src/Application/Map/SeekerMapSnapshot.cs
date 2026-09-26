using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Exposes one immutable Seeker-owned safe opening-region map state.</summary>
public sealed class SeekerMapSnapshot
{
    /// <summary>Creates a validated safe map snapshot from exact canonical definitions.</summary>
    internal SeekerMapSnapshot(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId,
        IEnumerable<MapLocationDefinition> reportedLocations,
        IEnumerable<MapLocationDefinition> verifiedLocations)
    {
        Catalogue = catalogue;
        SeekerId = seekerId;
        ReportedLocations = reportedLocations.ToList().AsReadOnly();
        VerifiedLocations = verifiedLocations.ToList().AsReadOnly();
    }

    /// <summary>Gets the exact hidden catalogue used to validate transitions.</summary>
    internal OpeningRegionMapLocationCatalogue Catalogue { get; }

    /// <summary>Gets the exact Seeker identifier owning this map.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets safe locations reported to the Seeker but not yet verified.</summary>
    public IReadOnlyList<MapLocationDefinition> ReportedLocations { get; }

    /// <summary>Gets safe locations the Seeker has verified.</summary>
    public IReadOnlyList<MapLocationDefinition> VerifiedLocations { get; }
}
