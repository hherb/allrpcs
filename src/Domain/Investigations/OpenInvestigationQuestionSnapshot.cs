using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded unanswered investigation questions in authored order.</summary>
public sealed class OpenInvestigationQuestionSnapshot
{
    /// <summary>Gets the maximum number of open questions.</summary>
    public const int MaximumQuestionCount = 32;

    /// <summary>Creates an immutable copy of supplied open questions.</summary>
    public OpenInvestigationQuestionSnapshot(IEnumerable<InvestigationQuestion> questions)
    {
        ArgumentNullException.ThrowIfNull(questions);

        List<InvestigationQuestion> retainedQuestions = [];
        HashSet<InvestigationQuestionId> questionIds = [];
        foreach (InvestigationQuestion? question in questions)
        {
            if (question is null)
            {
                throw new ArgumentException("Open questions cannot contain null.", nameof(questions));
            }

            if (!questionIds.Add(question.QuestionId))
            {
                throw new ArgumentException(
                    "Open questions cannot contain duplicate identities.",
                    nameof(questions));
            }

            retainedQuestions.Add(question);
            if (retainedQuestions.Count > MaximumQuestionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(questions),
                    $"A snapshot accepts at most {MaximumQuestionCount} open questions.");
            }
        }

        Questions = retainedQuestions.AsReadOnly();
    }

    /// <summary>Gets exact open questions in stable authored order.</summary>
    public IReadOnlyList<InvestigationQuestion> Questions { get; }
}
