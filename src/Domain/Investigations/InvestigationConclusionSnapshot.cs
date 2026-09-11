using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Investigations;

/// <summary>Contains bounded confirmed conclusions in stable supplied order.</summary>
public sealed class InvestigationConclusionSnapshot
{
    /// <summary>Gets the maximum number of retained conclusions.</summary>
    public const int MaximumConclusionCount = 32;

    /// <summary>Creates an immutable copy of supplied conclusions.</summary>
    public InvestigationConclusionSnapshot(IEnumerable<InvestigationConclusion> conclusions)
    {
        ArgumentNullException.ThrowIfNull(conclusions);

        List<InvestigationConclusion> retainedConclusions = [];
        HashSet<InvestigationConclusionId> conclusionIds = [];
        HashSet<InvestigationQuestionId> questionIds = [];
        foreach (InvestigationConclusion? conclusion in conclusions)
        {
            if (conclusion is null)
            {
                throw new ArgumentException("Conclusions cannot contain null.", nameof(conclusions));
            }

            if (!conclusionIds.Add(conclusion.ConclusionId))
            {
                throw new ArgumentException(
                    "Conclusions cannot contain duplicate identities.",
                    nameof(conclusions));
            }

            if (!questionIds.Add(conclusion.Question.QuestionId))
            {
                throw new ArgumentException(
                    "A question can have only one retained conclusion.",
                    nameof(conclusions));
            }

            retainedConclusions.Add(conclusion);
            if (retainedConclusions.Count > MaximumConclusionCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(conclusions),
                    $"A snapshot accepts at most {MaximumConclusionCount} conclusions.");
            }
        }

        Conclusions = retainedConclusions.AsReadOnly();
    }

    /// <summary>Gets exact conclusions in stable supplied order.</summary>
    public IReadOnlyList<InvestigationConclusion> Conclusions { get; }
}
