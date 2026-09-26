namespace Allrpcs.Application.Navigation;

/// <summary>Binds one exact selected Forester access to the existing closed-path scene.</summary>
public sealed class OpeningRegionClosedForestPathAccess
{
    /// <summary>Creates an additive scene access without changing other route definitions.</summary>
    public OpeningRegionClosedForestPathAccess(
        OpeningRegionClosedForestPathSceneBinding sceneBinding,
        SeekerBackgroundRouteAccess routeAccess)
    {
        ArgumentNullException.ThrowIfNull(sceneBinding);
        ArgumentNullException.ThrowIfNull(routeAccess);

        if (!ReferenceEquals(
                routeAccess.RouteState.OpeningRegion,
                sceneBinding.Projection)
            || !ReferenceEquals(
                routeAccess.RouteState.SelectedVariation,
                routeAccess.SelectedVariation)
            || !ReferenceEquals(
                routeAccess.SelectedVariation.DestinationAnchor,
                sceneBinding.ClosedForestPathAnchor)
            || !ReferenceEquals(
                routeAccess.DestinationAnchor,
                sceneBinding.ClosedForestPathAnchor))
        {
            throw new ArgumentException(
                "Closed-path access requires the exact selected Forester route provenance.",
                nameof(routeAccess));
        }

        SceneBinding = sceneBinding;
        RouteAccess = routeAccess;
    }

    /// <summary>Gets the exact existing closed-path scene binding.</summary>
    public OpeningRegionClosedForestPathSceneBinding SceneBinding { get; }

    /// <summary>Gets the exact additive route access consumed by the scene.</summary>
    public SeekerBackgroundRouteAccess RouteAccess { get; }
}
