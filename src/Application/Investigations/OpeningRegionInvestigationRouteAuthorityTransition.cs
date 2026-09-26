using Allrpcs.Application.Map;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Advances exact route progress only from explicit authoritative safe views.</summary>
public static class OpeningRegionInvestigationRouteAuthorityTransition
{
    /// <summary>Records one exact route location only after the supplied map verifies it.</summary>
    public static OpeningRegionInvestigationRouteProgressSnapshot RecordVerifiedLocation(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRoute route,
        SeekerMapSnapshot map,
        MapLocationDefinition location)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(location);

        if (!map.VerifiedLocations.Any(candidate => ReferenceEquals(candidate, location))
            || !route.LocationIds.Any(candidate => ReferenceEquals(candidate, location.Id)))
        {
            throw new InvalidOperationException(
                "Route progress requires an exact verified member location.");
        }

        return OpeningRegionInvestigationRouteProgressTransition.RecordLocation(
            current,
            route,
            location.Id);
    }

    /// <summary>Records one exact route evidence identity only when visible in the supplied view.</summary>
    public static OpeningRegionInvestigationRouteProgressSnapshot RecordVisibleEvidence(
        OpeningRegionInvestigationRouteProgressSnapshot current,
        OpeningRegionInvestigationRoute route,
        CharacterEvidenceViewSnapshot evidenceView,
        EvidenceId evidenceId)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(evidenceId);

        if (!evidenceView.EvidenceIds.Any(candidate => ReferenceEquals(candidate, evidenceId))
            || !route.EvidenceIds.Any(candidate => ReferenceEquals(candidate, evidenceId)))
        {
            throw new InvalidOperationException(
                "Route progress requires one exact visible member evidence identity.");
        }

        return OpeningRegionInvestigationRouteProgressTransition.RecordEvidence(
            current,
            route,
            evidenceId);
    }
}
