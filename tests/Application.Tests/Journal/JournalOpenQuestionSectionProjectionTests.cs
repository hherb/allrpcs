using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalOpenQuestionSectionProjectionTests
{
    [Fact]
    public void BindsPostExperimentsJournalToExactCanonicalSectionAndOpenQuestions()
    {
        JournalSnapshot journal = JournalWithExperiments();
        InvestigationQuestion first = new(
            new InvestigationQuestionId("what-causes-grey-sleep"),
            "What causes the grey sleep?");
        InvestigationQuestion second = new(
            new InvestigationQuestionId("what-slows-grey-sleep"),
            "What slows the grey sleep?");
        OpenInvestigationQuestionSnapshot openQuestions = new([first, second]);

        JournalOpenQuestionSectionBinding binding =
            JournalOpenQuestionSectionProjection.Bind(journal, openQuestions);

        Assert.Same(journal, binding.Journal);
        Assert.Same(journal.Catalogue.Sections[4], binding.Section);
        Assert.Same(journal.SeekerId, binding.SeekerId);
        Assert.Same(openQuestions, binding.OpenQuestions);
        Assert.Collection(
            binding.OpenQuestions.Questions,
            question => Assert.Same(first, question),
            question => Assert.Same(second, question));
    }

    [Fact]
    public void ReturnsFreshBindingsWithoutChangingEmptySourceState()
    {
        JournalSnapshot journal = JournalWithExperiments();
        OpenInvestigationQuestionSnapshot openQuestions = new([]);

        JournalOpenQuestionSectionBinding first =
            JournalOpenQuestionSectionProjection.Bind(journal, openQuestions);
        JournalOpenQuestionSectionBinding second =
            JournalOpenQuestionSectionProjection.Bind(journal, openQuestions);

        Assert.NotSame(first, second);
        Assert.Same(journal, first.Journal);
        Assert.Same(journal, second.Journal);
        Assert.Same(openQuestions, first.OpenQuestions);
        Assert.Same(openQuestions, second.OpenQuestions);
        Assert.Empty(openQuestions.Questions);
    }

    [Fact]
    public void RejectsJournalBeforeExperimentInstallationWithoutChangingIt()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithHypotheses(catalogue, new CharacterId("seeker"));
        OpenInvestigationQuestionSnapshot openQuestions = new([]);

        Assert.Throws<ArgumentException>(() =>
            JournalOpenQuestionSectionProjection.Bind(journal, openQuestions));
        Assert.Null(journal.Experiments);
        Assert.Same(catalogue.Sections[3], journal.PendingSections[0]);
        Assert.Empty(openQuestions.Questions);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithExperiments();
        OpenInvestigationQuestionSnapshot openQuestions = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            JournalOpenQuestionSectionProjection.Bind(null!, openQuestions));
        Assert.Throws<ArgumentNullException>(() =>
            JournalOpenQuestionSectionProjection.Bind(journal, null!));
    }

    private static JournalSnapshot JournalWithExperiments()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithHypotheses(catalogue, new CharacterId("seeker"));
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        return JournalExperimentSnapshotProjection.Compose(journal, experiments);
    }

    private static JournalSnapshot JournalWithHypotheses(
        JournalSectionCatalogue catalogue,
        CharacterId seekerId)
    {
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
        return JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
    }
}
