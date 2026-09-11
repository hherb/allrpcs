namespace Allrpcs.Domain.Investigations;

/// <summary>Composes the authored route-safety inputs for critical investigation conclusions.</summary>
public sealed class CriticalInvestigationConclusionScenario
{
    /// <summary>Creates one immutable scenario from exact plan and source snapshots.</summary>
    public CriticalInvestigationConclusionScenario(
        CriticalInvestigationConclusionPlanSnapshot plans,
        InvestigationEvidenceSourceSnapshot sources)
    {
        ArgumentNullException.ThrowIfNull(plans);
        ArgumentNullException.ThrowIfNull(sources);

        Plans = plans;
        Sources = sources;
    }

    /// <summary>Gets the exact authored critical-conclusion plans.</summary>
    public CriticalInvestigationConclusionPlanSnapshot Plans { get; }

    /// <summary>Gets the exact authored evidence-source associations.</summary>
    public InvestigationEvidenceSourceSnapshot Sources { get; }
}
