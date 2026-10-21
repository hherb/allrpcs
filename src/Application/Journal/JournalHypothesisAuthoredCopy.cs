using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Copies authored hypothesis text without exposing its source aggregates.</summary>
public sealed class JournalHypothesisAuthoredCopy
{
    internal JournalHypothesisAuthoredCopy(
        InvestigationHypothesisId hypothesisId,
        InvestigationQuestionId questionId,
        string questionPrompt,
        string statement,
        InvestigationHypothesisTestOutcome outcome = InvestigationHypothesisTestOutcome.Untested)
    {
        HypothesisId = hypothesisId;
        QuestionId = questionId;
        QuestionPrompt = questionPrompt;
        Statement = statement;
        Outcome = outcome;
    }

    /// <summary>Gets the exact canonical authored-hypothesis identifier.</summary>
    public InvestigationHypothesisId HypothesisId { get; }

    /// <summary>Gets the exact canonical identifier of the question being answered.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the exact authored question text.</summary>
    public string QuestionPrompt { get; }

    /// <summary>Gets the exact authored hypothesis statement.</summary>
    public string Statement { get; }

    /// <summary>Gets what the recorded experiments currently say about this hypothesis.</summary>
    public InvestigationHypothesisTestOutcome Outcome { get; }
}
