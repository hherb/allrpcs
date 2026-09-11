using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Defines one authored question without supplying or inferring an answer.</summary>
public sealed class InvestigationQuestion
{
    /// <summary>Gets the maximum authored question length in UTF-16 code units.</summary>
    public const int MaximumPromptLength = 500;

    /// <summary>Creates one immutable authored investigation question.</summary>
    public InvestigationQuestion(InvestigationQuestionId questionId, string prompt)
    {
        ArgumentNullException.ThrowIfNull(questionId);
        ArgumentNullException.ThrowIfNull(prompt);
        if (string.IsNullOrWhiteSpace(prompt) || prompt.Length > MaximumPromptLength)
        {
            throw new ArgumentException(
                $"A question must be nonblank and at most {MaximumPromptLength} UTF-16 code units.",
                nameof(prompt));
        }

        QuestionId = questionId;
        Prompt = prompt;
    }

    /// <summary>Gets the exact canonical question identity.</summary>
    public InvestigationQuestionId QuestionId { get; }

    /// <summary>Gets the authored question exactly as supplied.</summary>
    public string Prompt { get; }
}
