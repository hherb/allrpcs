using Allrpcs.Domain.Investigations;

namespace Allrpcs.Domain.Diseases;

/// <summary>Composes exact scenario and confirmed-evidence state for first-patient treatment.</summary>
public sealed class FirstPatientTreatmentContext
{
    /// <summary>Creates one immutable treatment context without applying a remedy.</summary>
    public FirstPatientTreatmentContext(
        FirstPatientScenario scenario,
        StabilizingRemedyEvidenceRequirement evidenceRequirement,
        InvestigationConclusionSnapshot conclusions)
    {
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(evidenceRequirement);
        ArgumentNullException.ThrowIfNull(conclusions);

        Scenario = scenario;
        EvidenceRequirement = evidenceRequirement;
        Conclusions = conclusions;
    }

    /// <summary>Gets the exact authored first-patient scenario.</summary>
    public FirstPatientScenario Scenario { get; }

    /// <summary>Gets the exact remedy-to-conclusion requirement.</summary>
    public StabilizingRemedyEvidenceRequirement EvidenceRequirement { get; }

    /// <summary>Gets the exact currently confirmed conclusions.</summary>
    public InvestigationConclusionSnapshot Conclusions { get; }

    /// <summary>
    /// Returns this context over the live disease truth once the patient has progressed: every
    /// other authored reference is kept, so prepared batches and dose records stay valid. The same
    /// truth returns this exact context.
    /// </summary>
    public FirstPatientTreatmentContext WithDiseaseTruth(DiseaseTruthSnapshot diseaseTruth)
    {
        ArgumentNullException.ThrowIfNull(diseaseTruth);

        if (ReferenceEquals(diseaseTruth, Scenario.DiseaseTruth))
        {
            return this;
        }

        FirstPatientScenario live = new(
            Scenario.PatientId,
            diseaseTruth,
            Scenario.Ingredients,
            Scenario.Remedy,
            Scenario.Investigation,
            Scenario.GatheredIngredients,
            Scenario.AlternativeRecipes);
        return new FirstPatientTreatmentContext(live, EvidenceRequirement, Conclusions);
    }
}
