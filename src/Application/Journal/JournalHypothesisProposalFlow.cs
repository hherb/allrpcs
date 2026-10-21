using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Turns one player's written hypothesis into an interpreter request and applies its answer.</summary>
public static class JournalHypothesisProposalFlow
{
    /// <summary>Builds a request offering only hypotheses of open questions, or null for unusable text or nothing open.</summary>
    public static InvestigationHypothesisRecognitionRequest? CreateRequest(
        JournalSnapshot journal,
        IReadOnlyList<FirstPatientHypothesisTest> tests,
        string text)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(text);

        string trimmed = text.Trim();
        IReadOnlyList<InvestigationQuestion> open = JournalInvestigationTransition.OpenQuestions(journal, tests);
        if (trimmed.Length == 0 || trimmed.Length > DialogueUtterance.MaximumTextLength || open.Count == 0)
        {
            return null;
        }

        return new InvestigationHypothesisRecognitionRequest(
            new DialogueUtterance(journal.SeekerId, trimmed),
            new OpenInvestigationQuestionSnapshot(open),
            new InvestigationHypothesisSnapshot(tests
                .Select(test => test.Hypothesis)
                .Where(hypothesis => open.Any(question => ReferenceEquals(question, hypothesis.Question)))));
    }

    /// <summary>Applies one finished interpretation (null for no match) to the current journal.</summary>
    public static (JournalHypothesisProposalResult Result, JournalSnapshot Journal) Apply(
        JournalSnapshot journal,
        InvestigationHypothesisRecognition? recognition)
    {
        ArgumentNullException.ThrowIfNull(journal);

        if (recognition is null)
        {
            return (JournalHypothesisProposalResult.NoMatch, journal);
        }

        return JournalInvestigationTransition.Propose(journal, recognition) is { } updated
            ? (JournalHypothesisProposalResult.Added, updated)
            : (JournalHypothesisProposalResult.AlreadyNoted, journal);
    }
}

/// <summary>Reports what one finished hypothesis proposal did to the journal.</summary>
public enum JournalHypothesisProposalResult
{
    /// <summary>The hypothesis was added untested.</summary>
    Added,

    /// <summary>The journal already holds the matched hypothesis.</summary>
    AlreadyNoted,

    /// <summary>The text proposed none of the open hypotheses.</summary>
    NoMatch,
}
