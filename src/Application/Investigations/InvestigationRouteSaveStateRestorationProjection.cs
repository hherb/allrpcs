using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Investigations;

/// <summary>Rebinds ID-only route state to fresh canonical opening-region definitions.</summary>
public static class InvestigationRouteSaveStateRestorationProjection
{
    /// <summary>Preflights and reconstructs the complete saved lifecycle without scene mutation.</summary>
    public static InvestigationRouteSaveStateRestoration Restore(
        InvestigationRouteSaveState state,
        OpeningRegionMapLocationCatalogue locations,
        DiscoveryScenario scenario)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(scenario);

        OpeningRegionInvestigationRouteCatalogue catalogue =
            RepresentativeOpeningRegionInvestigationRouteSource.Create(locations, scenario);
        if (state.Progress.Count != catalogue.Routes.Count)
        {
            throw Incompatible();
        }

        OpeningRegionInvestigationRouteProgressSnapshot progress =
            OpeningRegionInvestigationRouteProgressTransition.Initialize(catalogue);
        for (int index = 0; index < catalogue.Routes.Count; index++)
        {
            OpeningRegionInvestigationRoute route = catalogue.Routes[index];
            InvestigationRouteProgressSaveEntry entry = state.Progress[index];
            if (!string.Equals(entry.RouteKey, route.Key, StringComparison.Ordinal))
            {
                throw Incompatible();
            }

            progress = RestoreLocations(progress, route, entry.RecordedLocationIds);
            progress = RestoreEvidence(progress, route, entry.RecordedEvidenceIds);
        }

        List<OpeningRegionInvestigationRouteCompletion> completions = [];
        HashSet<OpeningRegionInvestigationRoute> accepted =
            new(ReferenceEqualityComparer.Instance);
        foreach (string? key in state.AcceptedCompletionRouteKeys)
        {
            OpeningRegionInvestigationRoute route = UniqueRoute(catalogue, key);
            OpeningRegionInvestigationRouteCompletion? completion =
                OpeningRegionInvestigationRouteCompletionPolicy.Authorize(progress, route);
            if (completion is null || !accepted.Add(route))
            {
                throw Incompatible();
            }

            completions.Add(completion);
        }

        OpeningRegionInvestigationRouteCompletion? consumed = null;
        if (state.ConsumedCompletionRouteKey is not null)
        {
            OpeningRegionInvestigationRoute route = UniqueRoute(
                catalogue,
                state.ConsumedCompletionRouteKey);
            consumed = completions.SingleOrDefault(completion =>
                ReferenceEquals(completion.Route, route)) ?? throw Incompatible();
        }

        return new InvestigationRouteSaveStateRestoration(
            progress,
            completions.AsReadOnly(),
            consumed);
    }

    private static OpeningRegionInvestigationRouteProgressSnapshot RestoreLocations(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        IEnumerable<LocationId> identifiers)
    {
        HashSet<LocationId> unique = [];
        foreach (LocationId? identifier in identifiers)
        {
            LocationId rebound = route.LocationIds.SingleOrDefault(candidate =>
                candidate == identifier) ?? throw Incompatible();
            if (identifier is null || !unique.Add(rebound))
            {
                throw Incompatible();
            }

            progress = OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
                progress,
                route,
                rebound);
        }

        return progress;
    }

    private static OpeningRegionInvestigationRouteProgressSnapshot RestoreEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route,
        IEnumerable<EvidenceId> identifiers)
    {
        HashSet<EvidenceId> unique = [];
        foreach (EvidenceId? identifier in identifiers)
        {
            EvidenceId rebound = route.EvidenceIds.SingleOrDefault(candidate =>
                candidate == identifier) ?? throw Incompatible();
            if (identifier is null || !unique.Add(rebound))
            {
                throw Incompatible();
            }

            progress = OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
                progress,
                route,
                rebound);
        }

        return progress;
    }

    private static OpeningRegionInvestigationRoute UniqueRoute(
        OpeningRegionInvestigationRouteCatalogue catalogue,
        string? key)
    {
        if (key is null)
        {
            throw Incompatible();
        }

        return catalogue.Routes.SingleOrDefault(route =>
            string.Equals(route.Key, key, StringComparison.Ordinal)) ?? throw Incompatible();
    }

    private static ArgumentException Incompatible() => new(
        "Saved route lifecycle must bind unique canonical identifiers and complete tokens.",
        "state");
}
