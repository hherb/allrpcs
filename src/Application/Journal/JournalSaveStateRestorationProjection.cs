using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Journal;

/// <summary>Restores closed journal persistence values into one fresh complete safe journal.</summary>
public static class JournalSaveStateRestorationProjection
{
    private const int ObservationsSectionIndex = 0;
    private const int TestimonySectionIndex = 1;
    private const int HypothesesSectionIndex = 2;
    private const int ExperimentsSectionIndex = 3;
    private const int OpenQuestionsSectionIndex = 4;

    /// <summary>Returns a fresh safe journal and its optional exact canonical selection.</summary>
    public static JournalSaveStateRestoration Restore(JournalSaveState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(state.SeekerId);
        RequireUniqueIdentities(state);
        RequireConsistentQuestions(state);

        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSectionDefinition? selectedSection = state.SelectedSectionKey is null
            ? null
            : catalogue.Sections.SingleOrDefault(section =>
                string.Equals(section.Key, state.SelectedSectionKey, StringComparison.Ordinal))
                ?? throw Incompatible();

        JournalSnapshot journal = new(
            catalogue,
            new JournalObservationSectionSnapshot(
                catalogue.Sections[ObservationsSectionIndex],
                state.SeekerId,
                state.Observations.Select(entry => new JournalObservationAuthoredCopy(
                    entry.ObjectId,
                    entry.DisplayName,
                    entry.Description))),
            new JournalTestimonySectionSnapshot(
                catalogue.Sections[TestimonySectionIndex],
                state.SeekerId,
                state.Testimony.Select(entry => new JournalTestimonyAuthoredCopy(
                    entry.SpeakerCharacterId,
                    entry.Text,
                    entry.DeliveryMode))),
            new JournalHypothesisSectionSnapshot(
                catalogue.Sections[HypothesesSectionIndex],
                state.SeekerId,
                state.Hypotheses.Select(entry => new JournalHypothesisAuthoredCopy(
                    entry.HypothesisId,
                    entry.QuestionId,
                    entry.QuestionPrompt,
                    entry.Statement))),
            new JournalExperimentSectionSnapshot(
                catalogue.Sections[ExperimentsSectionIndex],
                state.SeekerId,
                state.Experiments.Select(entry => new JournalExperimentAuthoredCopy(
                    entry.ExperimentId,
                    entry.Procedure,
                    entry.ObservedOutcome))),
            new JournalOpenQuestionSectionSnapshot(
                catalogue.Sections[OpenQuestionsSectionIndex],
                state.SeekerId,
                state.OpenQuestions.Select(entry => new JournalOpenQuestionAuthoredCopy(
                    entry.QuestionId,
                    entry.Prompt))),
            []);

        return new JournalSaveStateRestoration(journal, selectedSection);
    }

    private static void RequireUniqueIdentities(JournalSaveState state)
    {
        RequireUnique(state.Observations.Select(entry => entry.ObjectId));
        RequireUnique(state.Hypotheses.Select(entry => entry.HypothesisId));
        RequireUnique(state.Experiments.Select(entry => entry.ExperimentId));
        RequireUnique(state.OpenQuestions.Select(entry => entry.QuestionId));
    }

    private static void RequireConsistentQuestions(JournalSaveState state)
    {
        Dictionary<InvestigationQuestionId, string> prompts = [];
        foreach (JournalHypothesisSaveEntry hypothesis in state.Hypotheses)
        {
            RequireConsistentPrompt(prompts, hypothesis.QuestionId, hypothesis.QuestionPrompt);
        }

        foreach (JournalOpenQuestionSaveEntry question in state.OpenQuestions)
        {
            RequireConsistentPrompt(prompts, question.QuestionId, question.Prompt);
        }
    }

    private static void RequireConsistentPrompt(
        Dictionary<InvestigationQuestionId, string> prompts,
        InvestigationQuestionId questionId,
        string prompt)
    {
        if (prompts.TryGetValue(questionId, out string? priorPrompt)
            && !string.Equals(priorPrompt, prompt, StringComparison.Ordinal))
        {
            throw Incompatible();
        }

        prompts[questionId] = prompt;
    }

    private static void RequireUnique<T>(IEnumerable<T> identities)
        where T : notnull
    {
        HashSet<T> retained = [];
        if (identities.Any(identity => !retained.Add(identity)))
        {
            throw Incompatible();
        }
    }

    private static ArgumentException Incompatible() => new(
        "Saved journal state must contain canonical, unique, cross-section-consistent values.",
        "state");
}
