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
}
