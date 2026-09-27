using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Investigations;

/// <summary>Projects one complete exact route lifecycle into bounded ID-only save state.</summary>
public static class InvestigationRouteSaveStateCaptureProjection
{
    /// <summary>Captures only a complete internally consistent initialized lifecycle.</summary>
    public static InvestigationRouteSaveState Capture(
        OpeningRegionInvestigationRouteProgressSnapshot progress,
        IReadOnlyList<OpeningRegionInvestigationRouteCompletion> acceptedCompletions,
        OpeningRegionInvestigationRouteCompletion? consumedCompletion)
    {
        ArgumentNullException.ThrowIfNull(progress);
        ArgumentNullException.ThrowIfNull(acceptedCompletions);

        if (progress.Entries.Count != progress.Catalogue.Routes.Count)
        {
            throw Incompatible();
        }

        List<InvestigationRouteProgressSaveEntry> entries = [];
        for (int index = 0; index < progress.Catalogue.Routes.Count; index++)
        {
            OpeningRegionInvestigationRoute route = progress.Catalogue.Routes[index];
            OpeningRegionInvestigationRouteProgress entry = progress.Entries[index];
            if (!ReferenceEquals(entry.Route, route)
                || !AreUniqueExactMembers(entry.RecordedLocationIds, route.LocationIds)
                || !AreUniqueExactMembers(entry.RecordedEvidenceIds, route.EvidenceIds))
            {
                throw Incompatible();
            }

            entries.Add(new InvestigationRouteProgressSaveEntry(
                route.Key,
                entry.RecordedLocationIds,
                entry.RecordedEvidenceIds));
        }

        List<string> completionKeys = [];
        HashSet<OpeningRegionInvestigationRoute> completedRoutes =
            new(ReferenceEqualityComparer.Instance);
        OpeningRegionInvestigationRouteProgressSnapshot? priorCompletionProgress = null;
        foreach (OpeningRegionInvestigationRouteCompletion? completion in acceptedCompletions)
        {
            if (completion is null
                || !progress.Catalogue.Routes.Any(route => ReferenceEquals(route, completion.Route))
                || !IsSameOrEarlier(completion.ProgressSnapshot, progress)
                || priorCompletionProgress is not null
                    && !IsSameOrEarlier(priorCompletionProgress, completion.ProgressSnapshot)
                || !ReferenceEquals(
                    completion.ProgressSnapshot.Entries.SingleOrDefault(entry =>
                        ReferenceEquals(entry.Route, completion.Route)),
                    completion.RouteProgress)
                || OpeningRegionInvestigationRouteCompletionPolicy.Authorize(
                    completion.ProgressSnapshot,
                    completion.Route) is not { } authorized
                || !ReferenceEquals(authorized.RouteProgress, completion.RouteProgress)
                || !completedRoutes.Add(completion.Route))
            {
                throw Incompatible();
            }

            completionKeys.Add(completion.Route.Key);
            priorCompletionProgress = completion.ProgressSnapshot;
        }

        if (consumedCompletion is not null
            && !acceptedCompletions.Any(completion =>
                ReferenceEquals(completion, consumedCompletion)))
        {
            throw Incompatible();
        }

        return new InvestigationRouteSaveState(
            entries,
            completionKeys,
            consumedCompletion?.Route.Key);
    }

    private static bool AreUniqueExactMembers<T>(
        IReadOnlyList<T> recorded,
        IReadOnlyList<T> authored)
        where T : class
    {
        HashSet<T> retained = new(ReferenceEqualityComparer.Instance);
        return recorded.All(value => value is not null
            && authored.Any(candidate => ReferenceEquals(candidate, value))
            && retained.Add(value));
    }

    private static bool IsSameOrEarlier(
        OpeningRegionInvestigationRouteProgressSnapshot earlier,
        OpeningRegionInvestigationRouteProgressSnapshot later)
    {
        if (!ReferenceEquals(earlier.Catalogue, later.Catalogue)
            || earlier.Entries.Count != later.Entries.Count)
        {
            return false;
        }

        for (int index = 0; index < earlier.Entries.Count; index++)
        {
            OpeningRegionInvestigationRouteProgress first = earlier.Entries[index];
            OpeningRegionInvestigationRouteProgress second = later.Entries[index];
            if (!ReferenceEquals(first.Route, second.Route)
                || !IsExactPrefix(first.RecordedLocationIds, second.RecordedLocationIds)
                || !IsExactPrefix(first.RecordedEvidenceIds, second.RecordedEvidenceIds))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsExactPrefix<T>(IReadOnlyList<T> prefix, IReadOnlyList<T> values)
        where T : class => prefix.Count <= values.Count
            && prefix.Select((value, index) => ReferenceEquals(value, values[index])).All(equal => equal);

    private static ArgumentException Incompatible() => new(
        "Route lifecycle capture requires complete exact internally consistent provenance.");
}
