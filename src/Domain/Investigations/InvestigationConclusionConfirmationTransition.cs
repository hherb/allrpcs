using Allrpcs.Domain.Discovery;

namespace Allrpcs.Domain.Investigations;

/// <summary>Atomically closes one exact question and records its evidence-confirmed conclusion.</summary>
public static class InvestigationConclusionConfirmationTransition
{
    /// <summary>Returns updated immutable snapshots when one complete authored route is visible.</summary>
    public static InvestigationConclusionConfirmationResult Confirm(
        InvestigationConclusionEvidencePlan plan,
        CharacterEvidenceViewSnapshot evidence,
        OpenInvestigationQuestionSnapshot openQuestions,
        InvestigationConclusionSnapshot conclusions)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(openQuestions);
        ArgumentNullException.ThrowIfNull(conclusions);

        if (!InvestigationConclusionConfirmationPolicy.CanConfirm(plan, evidence))
        {
            throw new InvalidOperationException(
                "No complete authored evidence route is visible for the conclusion.");
        }

        InvestigationQuestion question = plan.Conclusion.Question;
        if (!openQuestions.Questions.Any(candidate => ReferenceEquals(candidate, question)))
        {
            throw new InvalidOperationException(
                "The conclusion's exact question is not currently open.");
        }

        OpenInvestigationQuestionSnapshot remainingQuestions = new(
            openQuestions.Questions.Where(candidate => !ReferenceEquals(candidate, question)));
        InvestigationConclusionSnapshot resultingConclusions = new(
            conclusions.Conclusions.Append(plan.Conclusion));

        return new InvestigationConclusionConfirmationResult(
            remainingQuestions,
            resultingConclusions);
    }
}
