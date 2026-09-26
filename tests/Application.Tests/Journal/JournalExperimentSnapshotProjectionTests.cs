using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalExperimentSnapshotProjectionTests
{
    [Fact]
    public void InstallsExactExperimentsAndRetainsOpenQuestionsAsPending()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithHypotheses(catalogue, seekerId);
        JournalExperimentSectionSnapshot experiments = Experiments(journal);

        JournalSnapshot result = JournalExperimentSnapshotProjection.Compose(journal, experiments);

        Assert.NotSame(journal, result);
        Assert.Same(catalogue, result.Catalogue);
        Assert.Same(journal.Observations, result.Observations);
        Assert.Same(journal.Testimony, result.Testimony);
        Assert.Same(journal.Hypotheses, result.Hypotheses);
        Assert.Same(seekerId, result.SeekerId);
        Assert.Same(experiments, result.Experiments);
        Assert.Null(result.OpenQuestions);
        Assert.Same(catalogue.Sections[4], Assert.Single(result.PendingSections));
        Assert.Collection(
            journal.PendingSections,
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
    }

    [Fact]
    public void ReturnsFreshJournalsWithIndependentReadOnlyPendingCollections()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithHypotheses(catalogue, new CharacterId("seeker"));
        JournalExperimentSectionSnapshot experiments = Experiments(journal);

        JournalSnapshot first = JournalExperimentSnapshotProjection.Compose(journal, experiments);
        JournalSnapshot second = JournalExperimentSnapshotProjection.Compose(journal, experiments);

        Assert.NotSame(first, second);
        Assert.NotSame(first.PendingSections, second.PendingSections);
        Assert.Same(experiments, first.Experiments);
        Assert.Same(experiments, second.Experiments);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)first.PendingSections).Clear());
    }

    [Fact]
    public void AcceptsEmptyExperimentsWithoutCreatingOpenQuestionContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithHypotheses(catalogue, new CharacterId("seeker"));
        JournalExperimentSectionSnapshot experiments = Experiments(journal);

        JournalSnapshot result = JournalExperimentSnapshotProjection.Compose(journal, experiments);

        Assert.Empty(result.Experiments!.Entries);
        Assert.Single(result.PendingSections);
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
    public void RejectsForeignSectionSeekerAndAlreadyInstalledExperimentsWithoutChangingInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithHypotheses(catalogue, seekerId);
        JournalExperimentSectionSnapshot valid = Experiments(journal);
        JournalSectionCatalogue rebuiltCatalogue = RepresentativeJournalSectionSource.Create();
        JournalExperimentSectionSnapshot rebuilt = Experiments(
            JournalWithHypotheses(rebuiltCatalogue, seekerId));
        CharacterId valueEqualSeekerId = new("seeker");
        JournalExperimentSectionSnapshot valueEqualSeeker = Experiments(
            JournalWithHypotheses(catalogue, valueEqualSeekerId));
        JournalSnapshot installed = JournalExperimentSnapshotProjection.Compose(journal, valid);

        Assert.Throws<ArgumentException>(() =>
            JournalExperimentSnapshotProjection.Compose(journal, rebuilt));
        Assert.Throws<ArgumentException>(() =>
            JournalExperimentSnapshotProjection.Compose(journal, valueEqualSeeker));
        Assert.Throws<ArgumentException>(() =>
            JournalExperimentSnapshotProjection.Compose(installed, valid));
        Assert.Null(journal.Experiments);
        Assert.Equal(2, journal.PendingSections.Count);
        Assert.Same(rebuiltCatalogue.Sections[3], rebuilt.Section);
        Assert.Same(valueEqualSeekerId, valueEqualSeeker.SeekerId);
        Assert.Same(valid, installed.Experiments);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithHypotheses(
            RepresentativeJournalSectionSource.Create(),
            new CharacterId("seeker"));
        JournalExperimentSectionSnapshot experiments = Experiments(journal);

        Assert.Throws<ArgumentNullException>(() =>
            JournalExperimentSnapshotProjection.Compose(null!, experiments));
        Assert.Throws<ArgumentNullException>(() =>
            JournalExperimentSnapshotProjection.Compose(journal, null!));
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

    private static JournalExperimentSectionSnapshot Experiments(JournalSnapshot journal) =>
        JournalExperimentSectionSnapshotProjection.Project(
            JournalExperimentSectionProjection.Bind(
                journal,
                new ExperimentJournalSnapshot([])));
}
