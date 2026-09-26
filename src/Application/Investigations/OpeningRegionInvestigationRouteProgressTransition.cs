using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Creates and explicitly advances exact ID-only investigation-route progress.</summary>
public static class OpeningRegionInvestigationRouteProgressTransition
{
    /// <summary>Creates one empty progress entry for every exact route.</summary>
    public static OpeningRegionInvestigationRouteProgressSnapshot Initialize(
        OpeningRegionInvestigationRouteCatalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(catalogue);

        List<OpeningRegionInvestigationRouteProgress> entries = catalogue.Routes
            .Select(route => new OpeningRegionInvestigationRouteProgress(
                route,
                Array.Empty<LocationId>(),
                Array.Empty<EvidenceId>()))
            .ToList();
        return new OpeningRegionInvestigationRouteProgressSnapshot(
            catalogue,
            entries.AsReadOnly());
    }

    /// <summary>Records one exact member location without inferring any route outcome.</summary>
    public static OpeningRegionInvestigationRouteProgressSnapshot RecordLocation(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRoute route,
        LocationId locationId)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(locationId);

        OpeningRegionInvestigationRouteProgress entry = RequireEntry(current, route);
        if (!route.LocationIds.Any(candidate => ReferenceEquals(candidate, locationId))
            || entry.RecordedLocationIds.Any(candidate => ReferenceEquals(candidate, locationId)))
        {
            throw new InvalidOperationException(
                "Progress can record one unrecorded exact location from the current route.");
        }

        return Replace(
            current,
            entry,
            new OpeningRegionInvestigationRouteProgress(
                route,
                entry.RecordedLocationIds.Append(locationId).ToList().AsReadOnly(),
                entry.RecordedEvidenceIds));
    }

    /// <summary>Records one exact member evidence identity without inferring any route outcome.</summary>
    public static OpeningRegionInvestigationRouteProgressSnapshot RecordEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRoute route,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(evidenceId);

        OpeningRegionInvestigationRouteProgress entry = RequireEntry(current, route);
        if (!route.EvidenceIds.Any(candidate => ReferenceEquals(candidate, evidenceId))
            || entry.RecordedEvidenceIds.Any(candidate => ReferenceEquals(candidate, evidenceId)))
        {
            throw new InvalidOperationException(
                "Progress can record one unrecorded exact evidence identity from the current route.");
        }

        return Replace(
            current,
            entry,
            new OpeningRegionInvestigationRouteProgress(
                route,
                entry.RecordedLocationIds,
                entry.RecordedEvidenceIds.Append(evidenceId).ToList().AsReadOnly()));
    }

    private static OpeningRegionInvestigationRouteProgress RequireEntry(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRoute route) => current.Entries.SingleOrDefault(entry =>
            ReferenceEquals(entry.Route, route))
            ?? throw new InvalidOperationException(
                "Progress requires one exact route from its current catalogue.");

    private static OpeningRegionInvestigationRouteProgressSnapshot Replace(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRouteProgress existing,
        OpeningRegionInvestigationRouteProgress replacement)
    {
        List<OpeningRegionInvestigationRouteProgress> entries = current.Entries
            .Select(entry => ReferenceEquals(entry, existing) ? replacement : entry)
            .ToList();
        return new OpeningRegionInvestigationRouteProgressSnapshot(
            current.Catalogue,
            entries.AsReadOnly());
    }
}
