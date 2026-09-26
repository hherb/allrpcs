namespace Allrpcs.Application.Navigation;

/// <summary>Authorizes one additive destination from exact current background-route state.</summary>
public static class SeekerBackgroundRouteAccessPolicy
{
    /// <summary>Returns one access token only for exact current, selected, unconsumed provenance.</summary>
    public static SeekerBackgroundRouteAccess? Authorize(
        SeekerBackgroundRouteVariationState currentState,
        SeekerBackgroundRouteVariationState requestedState,
        OpeningRegionAnchor destinationAnchor,
        SeekerBackgroundRouteAccess? issuedAccess)
    {
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(requestedState);
        ArgumentNullException.ThrowIfNull(destinationAnchor);

        SeekerBackgroundRouteVariation? selected = requestedState.SelectedVariation;
        if (issuedAccess is not null
            || !ReferenceEquals(currentState, requestedState)
            || selected is null
            || requestedState.PreviousState is null
            || requestedState.PreviousState.SelectedVariation is not null
            || !ReferenceEquals(
                requestedState.PreviousState.OpeningRegion,
                requestedState.OpeningRegion)
            || !ReferenceEquals(
                requestedState.PreviousState.Variations,
                requestedState.Variations)
            || !requestedState.Variations.Any(
                variation => ReferenceEquals(variation, selected))
            || !ReferenceEquals(selected.DestinationAnchor, destinationAnchor)
            || !requestedState.OpeningRegion.Anchors.Any(
                anchor => ReferenceEquals(anchor, destinationAnchor)))
        {
            return null;
        }

        return new SeekerBackgroundRouteAccess(
            requestedState,
            selected,
            destinationAnchor);
    }
}
