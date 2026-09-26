namespace Allrpcs.Application.Navigation;

/// <summary>Retains additive access to one exact selected background-route destination.</summary>
public sealed class SeekerBackgroundRouteAccess
{
    internal SeekerBackgroundRouteAccess(
        SeekerBackgroundRouteVariationState routeState,
        SeekerBackgroundRouteVariation selectedVariation,
        OpeningRegionAnchor destinationAnchor)
    {
        RouteState = routeState;
        SelectedVariation = selectedVariation;
        DestinationAnchor = destinationAnchor;
    }

    /// <summary>Gets the exact current selected route state that authorized access.</summary>
    public SeekerBackgroundRouteVariationState RouteState { get; }

    /// <summary>Gets the exact selected optional-route variation.</summary>
    public SeekerBackgroundRouteVariation SelectedVariation { get; }

    /// <summary>Gets the exact additive destination made accessible.</summary>
    public OpeningRegionAnchor DestinationAnchor { get; }
}
