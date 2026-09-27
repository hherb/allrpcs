namespace Allrpcs.Application.Investigations;

/// <summary>Authorizes exact route completion without prescribing observation order.</summary>
public static class OpeningRegionInvestigationRouteCompletionPolicy
{
    /// <summary>Returns exact completion provenance only when one route retains every member.</summary>
    public static OpeningRegionInvestigationRouteCompletion? Authorize(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        OpeningRegionInvestigationRoute route)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(route);

        OpeningRegionInvestigationRouteProgress? entry = progress.Entries.SingleOrDefault(
            candidate => ReferenceEquals(candidate.Route, route));
        if (entry is null
            || !HasExactMembers(route.LocationIds, entry.RecordedLocationIds)
            || !HasExactMembers(route.EvidenceIds, entry.RecordedEvidenceIds))
        {
            return null;
        }

        return new OpeningRegionInvestigationRouteCompletion(progress, entry, route);
    }

    private static bool HasExactMembers<T>(
        IReadOnlyList<T> required,
        IReadOnlyList<T> recorded)
        where T : class => required.Count == recorded.Count
            && required.All(requiredMember => recorded.Any(
                recordedMember => ReferenceEquals(recordedMember, requiredMember)));
}
