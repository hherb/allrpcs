using Allrpcs.Application.Discovery;

namespace Allrpcs.Application.Navigation;

/// <summary>Authorizes additive old-shrine inspection from exact current Scribe access.</summary>
public static class OpeningRegionOldShrineInspectionAccessPolicy
{
    private const string ScribeBackgroundKey = "scribe";

    /// <summary>Returns one inspection token without revealing or identifying the inscription.</summary>
    public static OpeningRegionOldShrineInspectionAccess? Authorize(
        SeekerBackgroundRouteAccess currentAccess,
        SeekerBackgroundRouteAccess requestedAccess,
        OpeningRegionOldShrineSceneBinding currentShrineSceneBinding,
        OpeningRegionOldShrineSceneBinding requestedShrineSceneBinding,
        OpeningRegionLayeredInscriptionBinding currentInscriptionBinding,
        OpeningRegionLayeredInscriptionBinding requestedInscriptionBinding,
        OpeningRegionOldShrineInspectionAccess? issuedAccess)
    {
        ArgumentNullException.ThrowIfNull(currentAccess);
        ArgumentNullException.ThrowIfNull(requestedAccess);
        ArgumentNullException.ThrowIfNull(currentShrineSceneBinding);
        ArgumentNullException.ThrowIfNull(requestedShrineSceneBinding);
        ArgumentNullException.ThrowIfNull(currentInscriptionBinding);
        ArgumentNullException.ThrowIfNull(requestedInscriptionBinding);

        SeekerBackgroundRouteVariationState routeState = requestedAccess.RouteState;
        SeekerBackgroundRouteVariation selected = requestedAccess.SelectedVariation;
        SeekerBackgroundRouteVariationState? previous = routeState.PreviousState;
        if (issuedAccess is not null
            || !ReferenceEquals(currentAccess, requestedAccess)
            || !ReferenceEquals(currentShrineSceneBinding, requestedShrineSceneBinding)
            || !ReferenceEquals(currentInscriptionBinding, requestedInscriptionBinding)
            || !ReferenceEquals(routeState.SelectedVariation, selected)
            || previous is null
            || previous.SelectedVariation is not null
            || !ReferenceEquals(previous.OpeningRegion, routeState.OpeningRegion)
            || !ReferenceEquals(previous.Variations, routeState.Variations)
            || !routeState.Variations.Any(candidate => ReferenceEquals(candidate, selected))
            || selected.Background.Key != ScribeBackgroundKey
            || !ReferenceEquals(selected.DestinationAnchor, requestedAccess.DestinationAnchor)
            || !ReferenceEquals(routeState.OpeningRegion, requestedShrineSceneBinding.Projection)
            || !ReferenceEquals(
                requestedAccess.DestinationAnchor,
                requestedShrineSceneBinding.OldShrineAnchor)
            || !ReferenceEquals(
                requestedInscriptionBinding.ShrineSceneBinding,
                requestedShrineSceneBinding))
        {
            return null;
        }

        return new OpeningRegionOldShrineInspectionAccess(
            requestedAccess,
            selected,
            requestedShrineSceneBinding,
            requestedInscriptionBinding);
    }
}
