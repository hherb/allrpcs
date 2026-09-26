using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.Map;

/// <summary>Authors the safe map-location catalogue from the representative opening region.</summary>
public static class RepresentativeOpeningRegionMapLocationSource
{
    /// <summary>Creates one fresh complete opening-region map-location catalogue.</summary>
    public static OpeningRegionMapLocationCatalogue Create()
    {
        OpeningRegionSnapshot region = RepresentativeOpeningRegionSource.Create();
        return new OpeningRegionMapLocationCatalogue(region.Locations.Select(location =>
            new MapLocationDefinition(location.Identity.Id, location.Identity.DisplayName)));
    }
}
