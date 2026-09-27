using Allrpcs.Application.Discovery;
using Allrpcs.Application.Investigations;

namespace Allrpcs.Game;

/// <summary>Verifies optional investigation-route effects of the old-shrine discovery smoke.</summary>
internal static class OpeningRegionOldShrineRouteSmokeVerifier
{
    /// <summary>Reports whether absent or active route state has the exact expected outcome.</summary>
    internal static bool HasExpectedOutcome(
        bool routeProgressEnabled,
        OpeningRegionInvestigationRoute? shrineRoute,
        OpeningRegionInvestigationRouteProgressSnapshot? beforeDiscovery,
        OpeningRegionInvestigationRouteProgressSnapshot? completed,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> acceptedCompletions,
        OpeningRegionLayeredInscriptionEvidenceExposure? evidenceExposure)
    {
        if (!routeProgressEnabled)
        {
            return beforeDiscovery is null
                && completed is null
                && acceptedCompletions.Count == 0;
        }

        if (shrineRoute is null
            || beforeDiscovery is null
            || completed is null
            || evidenceExposure is null)
        {
            return false;
        }

        OpeningRegionInvestigationRouteProgress shrineProgress = completed.Entries.Single(entry =>
            ReferenceEquals(entry.Route, shrineRoute));
        return !ReferenceEquals(completed, beforeDiscovery)
            && shrineProgress.RecordedLocationIds.Count == 1
            && shrineProgress.RecordedEvidenceIds.Count == 1
            && ReferenceEquals(
                shrineProgress.RecordedEvidenceIds.Single(),
                evidenceExposure.Evidence.EvidenceId)
            && completed.Entries
                .Where(entry => !ReferenceEquals(entry.Route, shrineRoute))
                .All(entry => entry.RecordedLocationIds.Count == 0
                    && entry.RecordedEvidenceIds.Count == 0)
            && acceptedCompletions.Count == 1
            && ReferenceEquals(acceptedCompletions[0].Route, shrineRoute)
            && ReferenceEquals(acceptedCompletions[0].ProgressSnapshot, completed);
    }
}
