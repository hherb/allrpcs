using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Journal;

/// <summary>Copies one authored open question without exposing its source aggregate.</summary>
public sealed class JournalOpenQuestionAuthoredCopy
{
    internal JournalOpenQuestionAuthoredCopy(
        InvestigationQuestionId questionId,
        string prompt)
    {
        QuestionId = questionId;
        Prompt = prompt;
    }

    /// <summary>Gets the exact canonical question identifier.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the exact authored question prompt.</summary>
    public string Prompt { get; }
}
