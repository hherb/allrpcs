using Allrpcs.Application.Investigations;
using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Godot;

namespace Allrpcs.Game;

/// <summary>Routes one explicit authoritative observation to every exact matching route.</summary>
public partial class OpeningRegionInvestigationRouteObservationRoutingCoordinator : Node
{
    private OpeningRegionInvestigationRouteAuthorityCoordinator authority = null!;
    private OpeningRegionInvestigationRouteCompletionCoordinator completions = null!;
    private SeekerMapController map = null!;
    private OpeningRegionInvestigationRouteProgressController progress = null!;

    /// <inheritdoc />
    public override void _Ready()
    {
        authority = GetNode<OpeningRegionInvestigationRouteAuthorityCoordinator>(
            "../OpeningRegionInvestigationRouteAuthorityCoordinator");
        completions = GetNode<OpeningRegionInvestigationRouteCompletionCoordinator>(
            "../OpeningRegionInvestigationRouteCompletionCoordinator");
        map = GetNode<SeekerMapController>("../SeekerMapController");
        progress = GetNode<OpeningRegionInvestigationRouteProgressController>(
            "../OpeningRegionInvestigationRouteProgressController");
    }

    /// <summary>Routes one exact currently verified location in authored catalogue order.</summary>
    public bool TryRouteVerifiedLocation(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        SeekerMapSnapshot expectedMap,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(expectedMap);
        ArgumentNullException.ThrowIfNull(location);

        if (!ReferenceEquals(progress.Progress, expectedProgress)
            || !ReferenceEquals(map.Map, expectedMap))
        {
            return false;
        }

        OpeningRegionInvestigationRoute[] matchingRoutes = expectedProgress.Catalogue.Routes
            .Where(route => route.LocationIds.Any(candidate =>
                ReferenceEquals(candidate, location.Id)))
            .ToArray();
        if (matchingRoutes.Length == 0
            || !CanRecordLocationOnEveryRoute(
                expectedProgress,
                matchingRoutes,
                expectedMap,
                location))
        {
            return false;
        }

        foreach (OpeningRegionInvestigationRoute route in matchingRoutes)
        {
            OpeningRegionInvestigationRouteProgressSnapshot current = progress.Progress!;
            if (!authority.TryRecordVerifiedLocation(current, route, expectedMap, location))
            {
                return false;
            }

            _ = completions.TryObserve(progress.Progress!, route);
        }

        return true;
    }

    /// <summary>Routes one exact currently visible evidence identity in authored catalogue order.</summary>
    public bool TryRouteVisibleEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot expectedProgress,
        CharacterEvidenceViewSnapshot evidenceView,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(expectedProgress);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(evidenceId);

        if (!ReferenceEquals(progress.Progress, expectedProgress))
        {
            return false;
        }

        OpeningRegionInvestigationRoute[] matchingRoutes = expectedProgress.Catalogue.Routes
            .Where(route => route.EvidenceIds.Any(candidate => ReferenceEquals(candidate, evidenceId)))
            .ToArray();
        if (matchingRoutes.Length == 0
            || !CanRecordEvidenceOnEveryRoute(
                expectedProgress,
                matchingRoutes,
                evidenceView,
                evidenceId))
        {
            return false;
        }

        foreach (OpeningRegionInvestigationRoute route in matchingRoutes)
        {
            OpeningRegionInvestigationRouteProgressSnapshot current = progress.Progress!;
            if (!authority.TryRecordVisibleEvidence(current, route, evidenceView, evidenceId))
            {
                return false;
            }

            _ = completions.TryObserve(progress.Progress!, route);
        }

        return true;
    }

    private static bool CanRecordLocationOnEveryRoute(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        IEnumerable<OpeningRegionInvestigationRoute> routes,
        SeekerMapSnapshot map,
        MapLocationDefinition location)
    {
        try
        {
            foreach (OpeningRegionInvestigationRoute route in routes)
            {
                current = OpeningRegionInvestigationRouteAuthorityTransition.RecordVerifiedLocation(
                    current,
                    route,
                    map,
                    location);
            }

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }

    private static bool CanRecordEvidenceOnEveryRoute(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        IEnumerable<OpeningRegionInvestigationRoute> routes,
        CharacterEvidenceViewSnapshot evidenceView,
        EvidenceId evidenceId)
    {
        try
        {
            foreach (OpeningRegionInvestigationRoute route in routes)
            {
                current = OpeningRegionInvestigationRouteAuthorityTransition.RecordVisibleEvidence(
                    current,
                    route,
                    evidenceView,
                    evidenceId);
            }

            return true;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
