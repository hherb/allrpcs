using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Investigations;

/// <summary>Retains only the bounded authored identifiers proposed by a language interpreter.</summary>
public sealed class InvestigationHypothesisProposal
{
    /// <summary>Creates one immutable provider-neutral proposal without accepting it.</summary>
    public InvestigationHypothesisProposal(
        InvestigationQuestionId questionId,
        InvestigationHypothesisId hypothesisId)
    {
        ArgumentNullException.ThrowIfNull(questionId);
        ArgumentNullException.ThrowIfNull(hypothesisId);

        QuestionId = questionId;
        HypothesisId = hypothesisId;
    }

    /// <summary>Gets the proposed authored question identity.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the proposed authored hypothesis identity.</summary>
    public InvestigationHypothesisId HypothesisId { get; }
}
