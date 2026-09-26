using Allrpcs.Application.Navigation;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Exposes one coordinate-free exact Seeker visit to an opening-region location.</summary>
public sealed class OpeningRegionLocationVisit
{
    /// <summary>Creates one validated visit from exact projection provenance.</summary>
    internal OpeningRegionLocationVisit(
        OpeningRegionProjection projection,
        CharacterId seekerId,
        LocationId locationId)
    {
        Projection = projection;
        SeekerId = seekerId;
        LocationId = locationId;
    }

    /// <summary>Gets the exact hidden opening-region projection used to validate the visit.</summary>
    internal OpeningRegionProjection Projection { get; }

    /// <summary>Gets the exact Seeker identifier whose position satisfied the visit boundary.</summary>
    public CharacterId SeekerId { get; }

    /// <summary>Gets the exact authored location identifier reached by the Seeker.</summary>
    public LocationId LocationId { get; }
}
