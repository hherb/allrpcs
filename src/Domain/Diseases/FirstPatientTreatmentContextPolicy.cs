namespace Allrpcs.Domain.Diseases;

/// <summary>Checks whether exact authored evidence currently authorizes first-patient treatment.</summary>
public static class FirstPatientTreatmentContextPolicy
{
    /// <summary>Reports whether the scenario's exact remedy has its exact authored conclusion.</summary>
    public static bool IsAuthorized(FirstPatientTreatmentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return FirstPatientScenarioStructuralPolicy.IsAcceptable(context.Scenario)
            && ReferenceEquals(context.EvidenceRequirement.Remedy, context.Scenario.Remedy)
            && context.Scenario.Investigation.Plans.Plans.Any(plan =>
                ReferenceEquals(plan.Conclusion, context.EvidenceRequirement.Conclusion))
            && context.Conclusions.Conclusions.Any(conclusion =>
                ReferenceEquals(conclusion, context.EvidenceRequirement.Conclusion));
    }
}
