using Allrpcs.Application.Investigations;
using Allrpcs.Domain.Diseases;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Ingredients;
using Allrpcs.Domain.Investigations;

namespace Allrpcs.Application.Journal;

/// <summary>Keeps a complete journal's hypotheses, experiments and open questions current.</summary>
public static class JournalInvestigationTransition
{
    /// <summary>Returns the journal with the recognized hypothesis appended untested, or null when already noted.</summary>
    public static JournalSnapshot? Propose(
        JournalSnapshot journal,
        InvestigationHypothesisRecognition recognition)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(recognition);
        JournalHypothesisSectionSnapshot hypotheses = RequireComplete(journal).Hypotheses!;

        InvestigationHypothesis hypothesis = recognition.Hypothesis;
        if (hypotheses.Entries.Any(entry => entry.HypothesisId == hypothesis.HypothesisId))
        {
            return null;
        }

        // An untested hypothesis answers nothing, so the open questions stay as they are.
        return With(
            journal,
            hypotheses.Entries.Append(new JournalHypothesisAuthoredCopy(
                hypothesis.HypothesisId,
                hypothesis.Question.QuestionId,
                hypothesis.Question.Prompt,
                hypothesis.Statement)).ToList(),
            journal.Experiments!,
            journal.OpenQuestions!);
    }

    /// <summary>Replaces experiments with the current records and re-derives outcomes and open questions.</summary>
    /// <remarks>Returns the same journal when nothing changed, so readers holding it stay current.</remarks>
    public static JournalSnapshot Refresh(
        JournalSnapshot journal,
        IReadOnlyList<FirstPatientHypothesisTest> tests,
        FirstPatientScenario scenario,
        IngredientExperimentJournalSnapshot doses,
        ExperimentJournalSnapshot experiments)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(tests);
        ArgumentNullException.ThrowIfNull(scenario);
        ArgumentNullException.ThrowIfNull(doses);
        ArgumentNullException.ThrowIfNull(experiments);
        RequireComplete(journal);

        List<JournalHypothesisAuthoredCopy> hypotheses = journal.Hypotheses!.Entries.Select(entry =>
            new JournalHypothesisAuthoredCopy(
                entry.HypothesisId,
                entry.QuestionId,
                entry.QuestionPrompt,
                entry.Statement,
                // A saved hypothesis the current authoring no longer tests stays untested.
                tests.FirstOrDefault(test => test.Hypothesis.HypothesisId == entry.HypothesisId) is { } test
                    ? FirstPatientHypothesisTestPolicy.Evaluate(test, scenario, doses)
                    : InvestigationHypothesisTestOutcome.Untested)).ToList();
        List<InvestigationQuestion> open = Open(hypotheses, tests);
        if (journal.Hypotheses.Entries.Select(entry => entry.Outcome).SequenceEqual(hypotheses.Select(entry => entry.Outcome))
            && journal.Experiments!.Entries.Select(entry => entry.ExperimentId)
                .SequenceEqual(experiments.Records.Select(record => record.ExperimentId))
            && journal.OpenQuestions!.Entries.Select(entry => entry.QuestionId)
                .SequenceEqual(open.Select(question => question.QuestionId)))
        {
            return journal;
        }

        return With(
            journal,
            hypotheses,
            new JournalExperimentSectionSnapshot(
                journal.Experiments!.Section,
                journal.SeekerId,
                experiments.Records.Select(record => new JournalExperimentAuthoredCopy(
                    record.ExperimentId,
                    record.Procedure,
                    record.ObservedOutcome))),
            new JournalOpenQuestionSectionSnapshot(
                journal.OpenQuestions!.Section,
                journal.SeekerId,
                open.Select(question =>
                    new JournalOpenQuestionAuthoredCopy(question.QuestionId, question.Prompt))));
    }

    /// <summary>Gets the tested questions that no confirmed journal hypothesis answers, in authored order.</summary>
    public static IReadOnlyList<InvestigationQuestion> OpenQuestions(
        JournalSnapshot journal,
        IReadOnlyList<FirstPatientHypothesisTest> tests)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(tests);
        return Open(RequireComplete(journal).Hypotheses!.Entries, tests);
    }

    private static List<InvestigationQuestion> Open(
        IReadOnlyList<JournalHypothesisAuthoredCopy> hypotheses,
        IReadOnlyList<FirstPatientHypothesisTest> tests) => tests
            .Select(test => test.Hypothesis.Question)
            .Distinct()
            .Where(question => !hypotheses.Any(entry =>
                entry.QuestionId == question.QuestionId
                && entry.Outcome == InvestigationHypothesisTestOutcome.Confirmed))
            .ToList();

    private static JournalSnapshot With(
        JournalSnapshot journal,
        IReadOnlyList<JournalHypothesisAuthoredCopy> hypotheses,
        JournalExperimentSectionSnapshot experiments,
        JournalOpenQuestionSectionSnapshot openQuestions) => new(
            journal.Catalogue,
            journal.Observations,
            journal.Testimony,
            new JournalHypothesisSectionSnapshot(journal.Hypotheses!.Section, journal.SeekerId, hypotheses),
            experiments,
            openQuestions,
            []);

    private static JournalSnapshot RequireComplete(JournalSnapshot journal) =>
        journal.PendingSections.Count == 0
            && journal.Testimony is not null
            && journal.Hypotheses is not null
            && journal.Experiments is not null
            && journal.OpenQuestions is not null
            ? journal
            : throw new ArgumentException("The journal must have every section installed.", nameof(journal));
}
