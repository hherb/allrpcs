using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Authors the pure representative investigation for the first patient.</summary>
public static class RepresentativeFirstPatientInvestigationSource
{
    /// <summary>Creates one immutable critical-conclusion scenario with recoverable routes.</summary>
    public static CriticalInvestigationConclusionScenario Create()
    {
        InvestigationQuestion question = new(
            new InvestigationQuestionId("what-stabilizes-grey-sleep"),
            "What preparation can stabilize a patient with the observed sleeping illness?");
        InvestigationHypothesis hypothesis = new(
            new InvestigationHypothesisId("cooled-moonwort-stabilizes"),
            question,
            "A cooled moonwort preparation may stabilize the patient's breathing.");
        InvestigationConclusion conclusion = new(
            new InvestigationConclusionId("cooling-moonwort-stabilizes"),
            question,
            hypothesis,
            "Cooling moonwort after steeping preserves a stabilizing preparation.");
        InvestigationConclusionEvidencePlan plan = new(
            conclusion,
            [
                Route(conclusion, "laundry-cooling-residue", "shrine-cooling-inscription"),
                Route(conclusion, "assistant-preparation-note", "cooled-dose-observation"),
            ]);
        InvestigationEvidenceSourceSnapshot sources = new(
            [
                Source("abandoned-laundry", "laundry-cooling-residue", false),
                Source("old-shrine", "shrine-cooling-inscription", false),
                Source("healer-garden", "assistant-preparation-note", true),
                Source("patient-experiment", "cooled-dose-observation", false),
            ]);

        return new CriticalInvestigationConclusionScenario(
            new CriticalInvestigationConclusionPlanSnapshot([plan]),
            sources);
    }

    private static InvestigationConclusionEvidenceRoute Route(
        InvestigationConclusion conclusion,
        params string[] evidenceIds) =>
        new(conclusion, evidenceIds.Select(evidenceId => new EvidenceId(evidenceId)));

    private static InvestigationEvidenceSource Source(
        string sourceId,
        string evidenceId,
        bool requiresCooperation) =>
        new(
            new InvestigationEvidenceSourceId(sourceId),
            new EvidenceId(evidenceId),
            requiresCooperation);
}
