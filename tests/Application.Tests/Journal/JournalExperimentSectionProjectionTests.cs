using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalExperimentSectionProjectionTests
{
    [Fact]
    public void BindsPostHypothesesJournalToExactCanonicalSectionAndExperiments()
    {
        JournalSnapshot journal = JournalWithHypotheses();
        ExperimentJournalRecord first = Record("stream-trial", "stream-trial-occurred");
        ExperimentJournalRecord second = Record("laundry-trial", "laundry-trial-occurred");
        ExperimentJournalSnapshot experiments = new([first, second]);

        JournalExperimentSectionBinding binding =
            JournalExperimentSectionProjection.Bind(journal, experiments);

        Assert.Same(journal, binding.Journal);
        Assert.Same(journal.Catalogue.Sections[3], binding.Section);
        Assert.Same(journal.SeekerId, binding.SeekerId);
        Assert.Same(experiments, binding.Experiments);
        Assert.Collection(
            binding.Experiments.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(second, record));
    }

    [Fact]
    public void ReturnsFreshBindingsWithoutChangingEmptySourceState()
    {
        JournalSnapshot journal = JournalWithHypotheses();
        ExperimentJournalSnapshot experiments = new([]);

        JournalExperimentSectionBinding first =
            JournalExperimentSectionProjection.Bind(journal, experiments);
        JournalExperimentSectionBinding second =
            JournalExperimentSectionProjection.Bind(journal, experiments);

        Assert.NotSame(first, second);
        Assert.Same(journal, first.Journal);
        Assert.Same(journal, second.Journal);
        Assert.Same(experiments, first.Experiments);
        Assert.Same(experiments, second.Experiments);
        Assert.Empty(experiments.Records);
    }

    [Fact]
    public void RejectsJournalBeforeHypothesesInstallationWithoutChangingIt()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithTestimony(catalogue, new CharacterId("seeker"));
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentException>(() =>
            JournalExperimentSectionProjection.Bind(journal, experiments));
        Assert.Null(journal.Hypotheses);
        Assert.Same(catalogue.Sections[2], journal.PendingSections[0]);
        Assert.Empty(experiments.Records);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithHypotheses();
        ExperimentJournalSnapshot experiments = new([]);

        Assert.Throws<ArgumentNullException>(() =>
            JournalExperimentSectionProjection.Bind(null!, experiments));
        Assert.Throws<ArgumentNullException>(() =>
            JournalExperimentSectionProjection.Bind(journal, null!));
    }

    private static JournalSnapshot JournalWithHypotheses()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithTestimony(catalogue, new CharacterId("seeker"));
        JournalHypothesisSectionSnapshot hypotheses =
            JournalHypothesisSectionSnapshotProjection.Project(
                JournalHypothesisSectionProjection.Bind(
                    journal,
                    new InvestigationHypothesisSnapshot([])));
        return JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
    }

    private static JournalSnapshot JournalWithTestimony(
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
        return JournalTestimonySnapshotProjection.Compose(journal, testimony);
    }

    private static ExperimentJournalRecord Record(string experimentId, string eventId) => new(
        new ExperimentId(experimentId),
        new WorldEvent(new WorldEventId(eventId), WorldEventTimeEffect.Advance),
        "Test a bounded procedure.",
        "Record a qualitative outcome.");
}
