namespace Allrpcs.Application.Investigations;

/// <summary>Retains exact provenance for one fully observed opening-region route.</summary>
public sealed class OpeningRegionInvestigationRouteCompletion
{
    internal OpeningRegionInvestigationRouteCompletion(
        OpeningRegionInvestigationRouteProgressSnapshot progressSnapshot,
        OpeningRegionInvestigationRouteProgress routeProgress,
        OpeningRegionInvestigationRoute route)
    {
        ProgressSnapshot = progressSnapshot;
        RouteProgress = routeProgress;
        Route = route;
    }

    /// <summary>Gets the exact complete progress snapshot that authorized this token.</summary>
    public OpeningRegionInvestigationRouteProgressSnapshot ProgressSnapshot { get; }

    /// <summary>Gets the exact complete entry retained by the progress snapshot.</summary>
    public OpeningRegionInvestigationRouteProgress RouteProgress { get; }

    /// <summary>Gets the exact authored route whose observations are complete.</summary>
    public OpeningRegionInvestigationRoute Route { get; }
}
