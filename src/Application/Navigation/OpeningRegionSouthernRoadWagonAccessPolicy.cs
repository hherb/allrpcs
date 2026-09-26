namespace Allrpcs.Application.Navigation;

/// <summary>Authorizes southern-road and wagon access from exact current Traveller access.</summary>
public static class OpeningRegionSouthernRoadWagonAccessPolicy
{
    private const string TravellerBackgroundKey = "traveller";

    /// <summary>Returns one exact access token without changing geometry or discovery.</summary>
    public static OpeningRegionSouthernRoadWagonAccess? Authorize(
        SeekerBackgroundRouteAccess currentAccess,
        SeekerBackgroundRouteAccess requestedAccess,
        OpeningRegionSouthernRoadWagonSceneBinding currentSceneBinding,
        OpeningRegionSouthernRoadWagonSceneBinding requestedSceneBinding,
        OpeningRegionSouthernRoadWagonAccess? issuedAccess)
    {
        ArgumentNullException.ThrowIfNull(currentAccess);
        ArgumentNullException.ThrowIfNull(requestedAccess);
        ArgumentNullException.ThrowIfNull(currentSceneBinding);
        ArgumentNullException.ThrowIfNull(requestedSceneBinding);

        SeekerBackgroundRouteVariationState routeState = requestedAccess.RouteState;
        SeekerBackgroundRouteVariation selected = requestedAccess.SelectedVariation;
        SeekerBackgroundRouteVariationState? previous = routeState.PreviousState;
        if (issuedAccess is not null
            || !ReferenceEquals(currentAccess, requestedAccess)
            || !ReferenceEquals(currentSceneBinding, requestedSceneBinding)
            || !ReferenceEquals(routeState.SelectedVariation, selected)
            || previous is null
            || previous.SelectedVariation is not null
            || !ReferenceEquals(previous.OpeningRegion, routeState.OpeningRegion)
            || !ReferenceEquals(previous.Variations, routeState.Variations)
            || !routeState.Variations.Any(candidate => ReferenceEquals(candidate, selected))
            || selected.Background.Key != TravellerBackgroundKey
            || !ReferenceEquals(selected.DestinationAnchor, requestedAccess.DestinationAnchor)
            || !ReferenceEquals(routeState.OpeningRegion, requestedSceneBinding.Projection)
            || !ReferenceEquals(
                requestedAccess.DestinationAnchor,
                requestedSceneBinding.SouthernRoadAnchor))
        {
            return null;
        }

        return new OpeningRegionSouthernRoadWagonAccess(
            requestedAccess,
            selected,
            requestedSceneBinding);
    }
}
