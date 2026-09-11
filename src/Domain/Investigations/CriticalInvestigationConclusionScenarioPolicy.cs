namespace Allrpcs.Domain.Investigations;

/// <summary>Validates route safety for every authored critical conclusion in one scenario.</summary>
public static class CriticalInvestigationConclusionScenarioPolicy
{
    /// <summary>Reports whether at least one critical plan exists and every plan is acceptable.</summary>
    public static bool IsAcceptable(
        CriticalInvestigationConclusionPlanSnapshot plans,
        InvestigationEvidenceSourceSnapshot sources)
    {
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(sources);

        return plans.Plans.Count > 0
            && plans.Plans.All(plan =>
                CriticalInvestigationConclusionPlanPolicy.IsAcceptable(plan, sources));
    }
}
