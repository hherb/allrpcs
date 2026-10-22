namespace Allrpcs.Domain.Diseases;

/// <summary>Checks that a first-patient treatment context is built from one exact authored scenario.</summary>
/// <remarks>
/// No confirmed conclusion is required: the remedy dose is the experiment that confirms it, so
/// requiring it first would make the remedy unreachable through play.
/// </remarks>
public static class FirstPatientTreatmentContextPolicy
{
    /// <summary>Reports whether the requirement names the scenario's exact remedy and authored conclusion.</summary>
    public static bool IsConsistent(FirstPatientTreatmentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return FirstPatientScenarioStructuralPolicy.IsAcceptable(context.Scenario)
            && ReferenceEquals(context.EvidenceRequirement.Remedy, context.Scenario.Remedy)
            && context.Scenario.Investigation.Plans.Plans.Any(plan =>
                ReferenceEquals(plan.Conclusion, context.EvidenceRequirement.Conclusion));
    }
}
