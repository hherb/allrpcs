using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Persistence;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSaveStateRestorationProjectionTests
{
    [Fact]
    public void RestoresEverySafeSectionInStableOrderWithExactCanonicalSelection()
    {
        JournalSaveState state = PopulatedState("experiments");

        JournalSaveStateRestoration restored =
            JournalSaveStateRestorationProjection.Restore(state);

        JournalSnapshot journal = restored.Journal;
        Assert.Same(state.SeekerId, journal.SeekerId);
        Assert.Empty(journal.PendingSections);
        Assert.Same(journal.Catalogue.Sections[0], journal.Observations.Section);
        Assert.Same(journal.Catalogue.Sections[1], journal.Testimony!.Section);
        Assert.Same(journal.Catalogue.Sections[2], journal.Hypotheses!.Section);
        Assert.Same(journal.Catalogue.Sections[3], journal.Experiments!.Section);
        Assert.Same(journal.Catalogue.Sections[4], journal.OpenQuestions!.Section);
        Assert.Same(journal.Catalogue.Sections[3], restored.SelectedSection);

        JournalObservationAuthoredCopy observation = Assert.Single(journal.Observations.Entries);
        Assert.Equal("old-shrine", observation.ObjectId.Value);
        Assert.Equal("Old Shrine", observation.DisplayName);
        Assert.Equal("Layered marks cross the weathered stone.", observation.Description);
        JournalTestimonyAuthoredCopy testimony = Assert.Single(journal.Testimony.Entries);
        Assert.Equal("healer", testimony.SpeakerCharacterId.Value);
        Assert.Equal("The stream changed after the rain.", testimony.Text);
        Assert.Same(DialogueDeliveryMode.Whispering, testimony.DeliveryMode);
        JournalHypothesisAuthoredCopy hypothesis = Assert.Single(journal.Hypotheses.Entries);
        Assert.Equal("stream-water", hypothesis.HypothesisId.Value);
        Assert.Equal("grey-sleep-cause", hypothesis.QuestionId.Value);
        Assert.Equal("What causes the grey sleep?", hypothesis.QuestionPrompt);
        Assert.Equal("The stream water may carry it.", hypothesis.Statement);
        JournalExperimentAuthoredCopy experiment = Assert.Single(journal.Experiments.Entries);
        Assert.Equal("covered-water", experiment.ExperimentId.Value);
        Assert.Equal("Compare covered water.", experiment.Procedure);
        Assert.Equal("The covered sample remained clear.", experiment.ObservedOutcome);
        JournalOpenQuestionAuthoredCopy question = Assert.Single(journal.OpenQuestions.Entries);
        Assert.Equal("grey-sleep-cause", question.QuestionId.Value);
        Assert.Equal("What causes the grey sleep?", question.Prompt);
    }

    [Fact]
    public void RestoresExplicitNoSelectionAndEmptySectionArms()
    {
        JournalSaveStateRestoration restored =
            JournalSaveStateRestorationProjection.Restore(EmptyState());

        Assert.Null(restored.SelectedSection);
        Assert.Empty(restored.Journal.Observations.Entries);
        Assert.Empty(restored.Journal.Testimony!.Entries);
        Assert.Empty(restored.Journal.Hypotheses!.Entries);
        Assert.Empty(restored.Journal.Experiments!.Entries);
        Assert.Empty(restored.Journal.OpenQuestions!.Entries);
    }

    [Fact]
    public void ReturnsFreshIndependentImmutableJournals()
    {
        JournalSaveState state = PopulatedState(null);

        JournalSaveStateRestoration first =
            JournalSaveStateRestorationProjection.Restore(state);
        JournalSaveStateRestoration second =
            JournalSaveStateRestorationProjection.Restore(state);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Journal, second.Journal);
        Assert.NotSame(first.Journal.Catalogue, second.Journal.Catalogue);
        Assert.NotSame(first.Journal.Catalogue.Sections[0], second.Journal.Catalogue.Sections[0]);
        Assert.NotSame(first.Journal.Observations, second.Journal.Observations);
        Assert.NotSame(first.Journal.Observations.Entries[0], second.Journal.Observations.Entries[0]);
        Assert.NotSame(first.Journal.Testimony!.Entries[0], second.Journal.Testimony!.Entries[0]);
        Assert.NotSame(first.Journal.Hypotheses!.Entries[0], second.Journal.Hypotheses!.Entries[0]);
        Assert.NotSame(first.Journal.Experiments!.Entries[0], second.Journal.Experiments!.Entries[0]);
        Assert.NotSame(first.Journal.OpenQuestions!.Entries[0], second.Journal.OpenQuestions!.Entries[0]);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalObservationAuthoredCopy>)first.Journal.Observations.Entries).Clear());
    }

    [Fact]
    public void RejectsDuplicateIdentitiesWithoutPartialOutput()
    {
        JournalSaveState baseline = PopulatedState(null);

        JournalSaveState[] invalidStates =
        [
            Copy(baseline, observations: [baseline.Observations[0], baseline.Observations[0]]),
            Copy(baseline, hypotheses: [baseline.Hypotheses[0], baseline.Hypotheses[0]]),
            Copy(baseline, experiments: [baseline.Experiments[0], baseline.Experiments[0]]),
            Copy(baseline, openQuestions: [baseline.OpenQuestions[0], baseline.OpenQuestions[0]]),
        ];

        Assert.All(invalidStates, state =>
            Assert.Throws<ArgumentException>(() =>
                JournalSaveStateRestorationProjection.Restore(state)));
        Assert.Single(baseline.Observations);
        Assert.Single(baseline.Hypotheses);
        Assert.Single(baseline.Experiments);
        Assert.Single(baseline.OpenQuestions);
    }

    [Fact]
    public void RejectsCrossSectionQuestionPromptInconsistency()
    {
        JournalSaveState state = PopulatedState(null);
        JournalOpenQuestionSaveEntry inconsistent = new(
            state.Hypotheses[0].QuestionId,
            "Which other cause should be tested?");

        Assert.Throws<ArgumentException>(() =>
            JournalSaveStateRestorationProjection.Restore(
                Copy(state, openQuestions: [inconsistent])));
        Assert.Equal("What causes the grey sleep?", state.Hypotheses[0].QuestionPrompt);
    }

    [Fact]
    public void RejectsNullState()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalSaveStateRestorationProjection.Restore(null!));
    }

    [Fact]
    public void ExposesOnlyTheRestoredSafeJournalAndCanonicalSelection()
    {
        Assert.Equal(
            ["Journal", "SelectedSection"],
            typeof(JournalSaveStateRestoration)
                .GetProperties()
                .Select(property => property.Name)
                .Order(StringComparer.Ordinal));
        Assert.All(
            typeof(JournalSaveStateRestoration).GetProperties(),
            property => Assert.False(property.CanWrite));
        Assert.Equal(
            typeof(JournalSnapshot),
            typeof(JournalSaveStateRestoration).GetProperty("Journal")!.PropertyType);
        Assert.Equal(
            typeof(JournalSectionDefinition),
            typeof(JournalSaveStateRestoration).GetProperty("SelectedSection")!.PropertyType);
    }

    private static JournalSaveState PopulatedState(string? selectedSectionKey) => new(
        new CharacterId("seeker"),
        [
            new JournalObservationSaveEntry(
                new ObjectId("old-shrine"),
                "Old Shrine",
                "Layered marks cross the weathered stone."),
        ],
        [
            new JournalTestimonySaveEntry(
                new CharacterId("healer"),
                "The stream changed after the rain.",
                DialogueDeliveryMode.Whispering),
        ],
        [
            new JournalHypothesisSaveEntry(
                new InvestigationHypothesisId("stream-water"),
                new InvestigationQuestionId("grey-sleep-cause"),
                "What causes the grey sleep?",
                "The stream water may carry it."),
        ],
        [
            new JournalExperimentSaveEntry(
                new ExperimentId("covered-water"),
                "Compare covered water.",
                "The covered sample remained clear."),
        ],
        [
            new JournalOpenQuestionSaveEntry(
                new InvestigationQuestionId("grey-sleep-cause"),
                "What causes the grey sleep?"),
        ],
        selectedSectionKey);

    private static JournalSaveState EmptyState() => new(
        new CharacterId("seeker"),
        [],
        [],
        [],
        [],
        [],
        null);

    private static JournalSaveState Copy(
        JournalSaveState state,
        IEnumerable<JournalObservationSaveEntry>? observations = null,
        IEnumerable<JournalHypothesisSaveEntry>? hypotheses = null,
        IEnumerable<JournalExperimentSaveEntry>? experiments = null,
        IEnumerable<JournalOpenQuestionSaveEntry>? openQuestions = null) => new(
            state.SeekerId,
            observations ?? state.Observations,
            state.Testimony,
            hypotheses ?? state.Hypotheses,
            experiments ?? state.Experiments,
            openQuestions ?? state.OpenQuestions,
            state.SelectedSectionKey);
}
