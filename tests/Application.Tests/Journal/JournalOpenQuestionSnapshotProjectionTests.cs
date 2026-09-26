using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalOpenQuestionSnapshotProjectionTests
{
    [Fact]
    public void InstallsExactOpenQuestionsAndCompletesTheCanonicalJournal()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithExperiments(catalogue, seekerId);
        JournalOpenQuestionSectionSnapshot openQuestions = OpenQuestions(journal);

        JournalSnapshot result = JournalOpenQuestionSnapshotProjection.Compose(
            journal,
            openQuestions);

        Assert.NotSame(journal, result);
        Assert.Same(catalogue, result.Catalogue);
        Assert.Same(journal.Observations, result.Observations);
        Assert.Same(journal.Testimony, result.Testimony);
        Assert.Same(journal.Hypotheses, result.Hypotheses);
        Assert.Same(journal.Experiments, result.Experiments);
        Assert.Same(seekerId, result.SeekerId);
        Assert.Same(openQuestions, result.OpenQuestions);
        Assert.Empty(result.PendingSections);
        Assert.Same(catalogue.Sections[4], Assert.Single(journal.PendingSections));
    }

    [Fact]
    public void ReturnsFreshJournalsWithIndependentReadOnlyPendingCollections()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithExperiments(catalogue, new CharacterId("seeker"));
        JournalOpenQuestionSectionSnapshot openQuestions = OpenQuestions(journal);

        JournalSnapshot first = JournalOpenQuestionSnapshotProjection.Compose(
            journal,
            openQuestions);
        JournalSnapshot second = JournalOpenQuestionSnapshotProjection.Compose(
            journal,
            openQuestions);

        Assert.NotSame(first, second);
        Assert.NotSame(first.PendingSections, second.PendingSections);
        Assert.Same(openQuestions, first.OpenQuestions);
        Assert.Same(openQuestions, second.OpenQuestions);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)first.PendingSections).Clear());
    }

    [Fact]
    public void AcceptsNoOpenQuestionsWithoutAddingPendingContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithExperiments(catalogue, new CharacterId("seeker"));
        JournalOpenQuestionSectionSnapshot openQuestions = OpenQuestions(journal);

        JournalSnapshot result = JournalOpenQuestionSnapshotProjection.Compose(
            journal,
            openQuestions);

        Assert.Empty(result.OpenQuestions!.Entries);
        Assert.Empty(result.PendingSections);
        Assert.Equal(
            [
                "Catalogue",
                "Experiments",
                "Hypotheses",
                "Observations",
                "OpenQuestions",
                "PendingSections",
                "SeekerId",
                "Testimony",
            ],
            typeof(JournalSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
    }

    [Fact]
    public void RejectsForeignSectionSeekerAndAlreadyInstalledOpenQuestionsWithoutChangingInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithExperiments(catalogue, seekerId);
        JournalOpenQuestionSectionSnapshot valid = OpenQuestions(journal);
        JournalSectionCatalogue rebuiltCatalogue = RepresentativeJournalSectionSource.Create();
        JournalOpenQuestionSectionSnapshot rebuilt = OpenQuestions(
            JournalWithExperiments(rebuiltCatalogue, seekerId));
        CharacterId valueEqualSeekerId = new("seeker");
        JournalOpenQuestionSectionSnapshot valueEqualSeeker = OpenQuestions(
            JournalWithExperiments(catalogue, valueEqualSeekerId));
        JournalSnapshot installed = JournalOpenQuestionSnapshotProjection.Compose(journal, valid);

        Assert.Throws<ArgumentException>(() =>
            JournalOpenQuestionSnapshotProjection.Compose(journal, rebuilt));
        Assert.Throws<ArgumentException>(() =>
            JournalOpenQuestionSnapshotProjection.Compose(journal, valueEqualSeeker));
        Assert.Throws<ArgumentException>(() =>
            JournalOpenQuestionSnapshotProjection.Compose(installed, valid));
        Assert.Null(journal.OpenQuestions);
        Assert.Single(journal.PendingSections);
        Assert.Same(rebuiltCatalogue.Sections[4], rebuilt.Section);
        Assert.Same(valueEqualSeekerId, valueEqualSeeker.SeekerId);
        Assert.Same(valid, installed.OpenQuestions);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithExperiments(
            RepresentativeJournalSectionSource.Create(),
            new CharacterId("seeker"));
        JournalOpenQuestionSectionSnapshot openQuestions = OpenQuestions(journal);

        Assert.Throws<ArgumentNullException>(() =>
            JournalOpenQuestionSnapshotProjection.Compose(null!, openQuestions));
        Assert.Throws<ArgumentNullException>(() =>
            JournalOpenQuestionSnapshotProjection.Compose(journal, null!));
    }

    private static JournalSnapshot JournalWithExperiments(
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
        journal = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalExperimentSectionSnapshot experiments =
            JournalExperimentSectionSnapshotProjection.Project(
                JournalExperimentSectionProjection.Bind(
                    journal,
                    new ExperimentJournalSnapshot([])));
        return JournalExperimentSnapshotProjection.Compose(journal, experiments);
    }

    private static JournalOpenQuestionSectionSnapshot OpenQuestions(JournalSnapshot journal) =>
        JournalOpenQuestionSectionSnapshotProjection.Project(
            JournalOpenQuestionSectionProjection.Bind(
                journal,
                new OpenInvestigationQuestionSnapshot([])));
}
