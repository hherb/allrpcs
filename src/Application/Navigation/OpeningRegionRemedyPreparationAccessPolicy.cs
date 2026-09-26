namespace Allrpcs.Application.Navigation;

/// <summary>Authorizes preparation-station access from exact current Apothecary selection.</summary>
public static class OpeningRegionRemedyPreparationAccessPolicy
{
    private const string ApothecaryBackgroundKey = "apothecarys-assistant";

    /// <summary>Returns one exact access token without starting or advancing preparation.</summary>
    public static OpeningRegionRemedyPreparationAccess? Authorize(
        SeekerBackgroundRouteAccess currentAccess,
        SeekerBackgroundRouteAccess requestedAccess,
        OpeningRegionRemedyPreparationSceneBinding currentSceneBinding,
        OpeningRegionRemedyPreparationSceneBinding requestedSceneBinding,
        OpeningRegionRemedyPreparationAccess? issuedAccess)
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
            || selected.Background.Key != ApothecaryBackgroundKey
            || !ReferenceEquals(selected.DestinationAnchor, requestedAccess.DestinationAnchor)
            || !ReferenceEquals(routeState.OpeningRegion, requestedSceneBinding.Projection)
            || !ReferenceEquals(
                requestedAccess.DestinationAnchor,
                requestedSceneBinding.GardenAnchor)
            || !ReferenceEquals(
                requestedSceneBinding.Recipe,
                requestedSceneBinding.TreatmentContext.Scenario.Remedy.Recipe))
        {
            return null;
        }

        return new OpeningRegionRemedyPreparationAccess(
            requestedAccess,
            selected,
            requestedSceneBinding);
    }
}
