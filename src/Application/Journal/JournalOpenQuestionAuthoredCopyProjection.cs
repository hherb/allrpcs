using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Projects journal-safe authored copies from exact open-question provenance.</summary>
public static class JournalOpenQuestionAuthoredCopyProjection
{
    /// <summary>Returns fresh read-only copies in stable authored order.</summary>
    public static IReadOnlyList<JournalOpenQuestionAuthoredCopy> Project(
        JournalOpenQuestionSectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        List<JournalOpenQuestionAuthoredCopy> copies =
            new(binding.OpenQuestions.Questions.Count);
        foreach (InvestigationQuestion question in binding.OpenQuestions.Questions)
        {
            copies.Add(new JournalOpenQuestionAuthoredCopy(
                question.QuestionId,
                question.Prompt));
        }

        return copies.AsReadOnly();
    }
}
