using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Authors the evidence-confirmed representative first-patient treatment context.</summary>
public static class RepresentativeFirstPatientTreatmentContextSource
{
    /// <summary>Creates fresh exact scenario, remedy requirement, and confirmed conclusion state.</summary>
    public static FirstPatientTreatmentContext Create()
    {
        FirstPatientScenario scenario = RepresentativeFirstPatientScenarioSource.Create();
        InvestigationConclusion conclusion = GetSingleConclusion(scenario);
        StabilizingRemedyEvidenceRequirement requirement = new(scenario.Remedy, conclusion);

        return new FirstPatientTreatmentContext(
            scenario,
            requirement,
            new InvestigationConclusionSnapshot([conclusion]));
    }

    private static InvestigationConclusion GetSingleConclusion(FirstPatientScenario scenario)
    {
        if (scenario.Investigation.Plans.Plans.Count != 1)
        {
            throw new InvalidOperationException(
                "The representative first-patient scenario must author exactly one critical conclusion.");
        }

        return scenario.Investigation.Plans.Plans[0].Conclusion;
    }
}
