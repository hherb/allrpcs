using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Canonicalizes a proposed hypothesis match against authored investigation state.</summary>
public static class InvestigationHypothesisRecognitionPolicy
{
    /// <summary>Returns a recognition only for an authored hypothesis of an exact open question.</summary>
    public static InvestigationHypothesisRecognition? TryRecognize(
        DialogueUtterance sourceUtterance,
        InvestigationQuestionId questionId,
        InvestigationHypothesisId hypothesisId,
        OpenInvestigationQuestionSnapshot openQuestions,
        InvestigationHypothesisSnapshot hypotheses)
    {
        ArgumentNullException.ThrowIfNull(sourceUtterance);
        ArgumentNullException.ThrowIfNull(questionId);
        ArgumentNullException.ThrowIfNull(hypothesisId);
        ArgumentNullException.ThrowIfNull(openQuestions);
        ArgumentNullException.ThrowIfNull(hypotheses);

        InvestigationQuestion? question = openQuestions.Questions.FirstOrDefault(
            candidate => candidate.QuestionId == questionId);
        InvestigationHypothesis? hypothesis = hypotheses.Hypotheses.FirstOrDefault(
            candidate => candidate.HypothesisId == hypothesisId);
        if (question is null
            || hypothesis is null
            || !ReferenceEquals(hypothesis.Question, question))
        {
            return null;
        }

        return new InvestigationHypothesisRecognition(sourceUtterance, hypothesis);
    }
}
