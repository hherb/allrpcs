using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Validates authored route diversity and recovery for one critical conclusion.</summary>
public static class CriticalInvestigationConclusionPlanPolicy
{
    private const int MinimumRouteCount = 2;
    private const int MinimumSourceCount = 3;

    /// <summary>Reports whether a plan retains diverse, non-cooperative, recoverable routes.</summary>
    public static bool IsAcceptable(
        InvestigationConclusionEvidencePlan plan,
        InvestigationEvidenceSourceSnapshot sources)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(sources);

        if (plan.Routes.Count < MinimumRouteCount)
        {
            return false;
        }

        Dictionary<EvidenceId, InvestigationEvidenceSource> sourcesByEvidence =
            sources.Sources.ToDictionary(source => source.EvidenceId);
        List<IReadOnlyList<InvestigationEvidenceSource>> routeSources = [];
        foreach (InvestigationConclusionEvidenceRoute route in plan.Routes)
        {
            List<InvestigationEvidenceSource> retainedRouteSources = [];
            foreach (EvidenceId evidenceId in route.RequiredEvidenceIds)
            {
                if (!sourcesByEvidence.TryGetValue(evidenceId, out InvestigationEvidenceSource? source))
                {
                    return false;
                }

                retainedRouteSources.Add(source);
            }

            routeSources.Add(retainedRouteSources);
        }

        InvestigationEvidenceSourceId[] usedSourceIds = routeSources
            .SelectMany(route => route)
            .Select(source => source.SourceId)
            .Distinct()
            .ToArray();
        return usedSourceIds.Length >= MinimumSourceCount
            && routeSources.Any(route => route.All(source => !source.RequiresCooperation))
            && usedSourceIds.All(unavailableSourceId => routeSources.Any(route =>
                route.All(source => source.SourceId != unavailableSourceId)));
    }
}
