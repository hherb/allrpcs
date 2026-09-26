namespace Allrpcs.Application.Navigation;

/// <summary>Retains exact additive Traveller access to the southern road and wagon.</summary>
public sealed class OpeningRegionSouthernRoadWagonAccess
{
    internal OpeningRegionSouthernRoadWagonAccess(
        SeekerBackgroundRouteAccess routeAccess,
        SeekerBackgroundRouteVariation travellerVariation,
        OpeningRegionSouthernRoadWagonSceneBinding sceneBinding)
    {
        RouteAccess = routeAccess;
        TravellerVariation = travellerVariation;
        SceneBinding = sceneBinding;
    }

    /// <summary>Gets the exact current selected background-route access.</summary>
    public SeekerBackgroundRouteAccess RouteAccess { get; }

    /// <summary>Gets the exact selected Traveller route variation.</summary>
    public SeekerBackgroundRouteVariation TravellerVariation { get; }

    /// <summary>Gets the exact existing southern-road and wagon scene binding.</summary>
    public OpeningRegionSouthernRoadWagonSceneBinding SceneBinding { get; }
}
