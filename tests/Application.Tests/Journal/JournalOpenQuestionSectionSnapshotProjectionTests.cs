using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalOpenQuestionSectionSnapshotProjectionTests
{
    [Fact]
    public void ProjectsExactSectionSeekerAndSafeOpenQuestionsInStableOrder()
    {
        InvestigationQuestionId firstId = new("what-causes-grey-sleep");
        InvestigationQuestionId secondId = new("what-slows-grey-sleep");
        string firstPrompt = "  What causes the grey sleep?  ";
        string secondPrompt = "What slows the grey sleep?";
        InvestigationQuestion first = new(firstId, firstPrompt);
        InvestigationQuestion second = new(secondId, secondPrompt);
        JournalOpenQuestionSectionBinding binding = Binding(first, second);

        JournalOpenQuestionSectionSnapshot snapshot =
            JournalOpenQuestionSectionSnapshotProjection.Project(binding);

        Assert.Same(binding.Section, snapshot.Section);
        Assert.Same(binding.SeekerId, snapshot.SeekerId);
        Assert.Collection(
            snapshot.Entries,
            copy =>
            {
                Assert.Same(firstId, copy.QuestionId);
                Assert.Same(firstPrompt, copy.Prompt);
            },
            copy =>
            {
                Assert.Same(secondId, copy.QuestionId);
                Assert.Same(secondPrompt, copy.Prompt);
            });
        Assert.Collection(
            binding.OpenQuestions.Questions,
            question => Assert.Same(first, question),
            question => Assert.Same(second, question));
    }

    [Fact]
    public void ReturnsFreshSnapshotsCopiesAndIndependentReadOnlyCollections()
    {
        InvestigationQuestion question = new(
            new InvestigationQuestionId("question"),
            "What happened?");
        JournalOpenQuestionSectionBinding binding = Binding(question);

        JournalOpenQuestionSectionSnapshot first =
            JournalOpenQuestionSectionSnapshotProjection.Project(binding);
        JournalOpenQuestionSectionSnapshot second =
            JournalOpenQuestionSectionSnapshotProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Entries, second.Entries);
        Assert.NotSame(Assert.Single(first.Entries), Assert.Single(second.Entries));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalOpenQuestionAuthoredCopy>)first.Entries).Clear());
    }

    [Fact]
    public void AcceptsNoOpenQuestionsWithoutExposingSourceProvenance()
    {
        JournalOpenQuestionSectionBinding binding = Binding();

        JournalOpenQuestionSectionSnapshot snapshot =
            JournalOpenQuestionSectionSnapshotProjection.Project(binding);

        Assert.Empty(snapshot.Entries);
        Assert.Equal(
            ["Entries", "Section", "SeekerId"],
            typeof(JournalOpenQuestionSectionSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalOpenQuestionAuthoredCopy>)snapshot.Entries).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalOpenQuestionSectionSnapshotProjection.Project(null!));
    }

    private static JournalOpenQuestionSectionBinding Binding(
        params InvestigationQuestion[] questions)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalSnapshot journal = JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
        JournalTestimonySectionSnapshot testimony =
            JournalTestimonySectionSnapshotProjection.Project(
                JournalTestimonySectionProjection.Bind(journal, []));
        journal = JournalTestimonySnapshotProjection.Compose(journal, testimony);
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot([])));
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        journal = JournalExperimentSnapshotProjection.Compose(journal, experiments);
        return JournalOpenQuestionSectionProjection.Bind(
            journal,
            new OpenInvestigationQuestionSnapshot(questions));
    }
}
