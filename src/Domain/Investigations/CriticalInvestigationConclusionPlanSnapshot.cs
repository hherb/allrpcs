using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded critical-conclusion evidence plans in stable authored order.</summary>
public sealed class CriticalInvestigationConclusionPlanSnapshot
{
    /// <summary>Gets the maximum number of retained critical-conclusion plans.</summary>
    public const int MaximumPlanCount = 32;

    /// <summary>Creates an immutable copy with unique conclusion identities.</summary>
    public CriticalInvestigationConclusionPlanSnapshot(
        IEnumerable<InvestigationConclusionEvidencePlan> plans)
    {
        ArgumentNullException.ThrowIfNull(plans);

        List<InvestigationConclusionEvidencePlan> retainedPlans = [];
        HashSet<InvestigationConclusionId> conclusionIds = [];
        foreach (InvestigationConclusionEvidencePlan? plan in plans)
        {
            if (plan is null)
            {
                throw new ArgumentException(
                    "Critical-conclusion plans cannot contain null.",
                    nameof(plans));
            }

            if (!conclusionIds.Add(plan.Conclusion.ConclusionId))
            {
                throw new ArgumentException(
                    "Critical-conclusion plans cannot repeat a conclusion identity.",
                    nameof(plans));
            }

            retainedPlans.Add(plan);
            if (retainedPlans.Count > MaximumPlanCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(plans),
                    $"A snapshot accepts at most {MaximumPlanCount} critical-conclusion plans.");
            }
        }

        Plans = retainedPlans.AsReadOnly();
    }

    /// <summary>Gets exact critical-conclusion plans in stable authored order.</summary>
    public IReadOnlyList<InvestigationConclusionEvidencePlan> Plans { get; }
}
