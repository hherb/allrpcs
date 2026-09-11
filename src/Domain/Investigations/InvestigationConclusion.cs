using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Describes an authored conclusion without deciding that evidence confirms it.</summary>
public sealed class InvestigationConclusion
{
    /// <summary>Gets the maximum authored conclusion length in UTF-16 code units.</summary>
    public const int MaximumStatementLength = 500;

    /// <summary>Creates one immutable conclusion for an exact question and hypothesis.</summary>
    public InvestigationConclusion(
        InvestigationConclusionId conclusionId,
        InvestigationQuestion question,
        InvestigationHypothesis hypothesis,
        string statement)
    {
        ArgumentNullException.ThrowIfNull(conclusionId);
        ArgumentNullException.ThrowIfNull(question);
        ArgumentNullException.ThrowIfNull(hypothesis);
        ArgumentNullException.ThrowIfNull(statement);
        if (!ReferenceEquals(question, hypothesis.Question))
        {
            throw new ArgumentException(
                "A conclusion and hypothesis must retain the same exact question.",
                nameof(hypothesis));
        }

        if (string.IsNullOrWhiteSpace(statement) || statement.Length > MaximumStatementLength)
        {
            throw new ArgumentException(
                $"A conclusion must be nonblank and at most {MaximumStatementLength} UTF-16 code units.",
                nameof(statement));
        }

        ConclusionId = conclusionId;
        Question = question;
        Hypothesis = hypothesis;
        Statement = statement;
    }

    /// <summary>Gets the exact canonical conclusion identity.</summary>
    public InvestigationConclusionId ConclusionId { get; }

    /// <summary>Gets the exact authored question answered by this conclusion.</summary>
    public InvestigationQuestion Question { get; }

    /// <summary>Gets the exact authored hypothesis associated with this conclusion.</summary>
    public InvestigationHypothesis Hypothesis { get; }

    /// <summary>Gets the authored conclusion statement exactly as supplied.</summary>
    public string Statement { get; }
}
