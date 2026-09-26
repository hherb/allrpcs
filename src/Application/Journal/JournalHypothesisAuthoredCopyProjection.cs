using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Projects journal-safe authored copies from exact hypothesis provenance.</summary>
public static class JournalHypothesisAuthoredCopyProjection
{
    /// <summary>Returns fresh read-only copies in stable authored order.</summary>
    public static IReadOnlyList<JournalHypothesisAuthoredCopy> Project(
        JournalHypothesisSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        List<JournalHypothesisAuthoredCopy> copies =
            new(binding.Hypotheses.Hypotheses.Count);
        foreach (InvestigationHypothesis hypothesis in binding.Hypotheses.Hypotheses)
        {
            copies.Add(new JournalHypothesisAuthoredCopy(
                hypothesis.HypothesisId,
                hypothesis.Question.QuestionId,
                hypothesis.Question.Prompt,
                hypothesis.Statement));
        }

        return copies.AsReadOnly();
    }
}
