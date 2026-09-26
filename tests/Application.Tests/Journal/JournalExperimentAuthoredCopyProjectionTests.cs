using Allrpcs.Application.Journal;
using Allrpcs.Domain.Experiments;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldEvents;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalExperimentAuthoredCopyProjectionTests
{
    [Fact]
    public void CopiesAuthoredExperimentsInStableOrderWithoutAuthoritativeEventExposure()
    {
        ExperimentId firstId = new("stream-trial");
        ExperimentId secondId = new("laundry-trial");
        string firstProcedure = "  Compare covered and uncovered stream water.  ";
        string firstOutcome = "The covered sample remained clear.";
        string secondProcedure = "Compare shared and separately washed cloth.";
        string secondOutcome = "Both cloth samples changed colour.";
        ExperimentJournalRecord first = Record(
            firstId,
            "stream-trial-occurred",
            firstProcedure,
            firstOutcome);
        ExperimentJournalRecord second = Record(
            secondId,
            "laundry-trial-occurred",
            secondProcedure,
            secondOutcome);
        JournalExperimentSectionBinding binding = Binding(first, second);

        IReadOnlyList<JournalExperimentAuthoredCopy> copies =
            JournalExperimentAuthoredCopyProjection.Project(binding);

        Assert.Collection(
            copies,
            copy =>
            {
                Assert.Same(firstId, copy.ExperimentId);
                Assert.Same(firstProcedure, copy.Procedure);
                Assert.Same(firstOutcome, copy.ObservedOutcome);
            },
            copy =>
            {
                Assert.Same(secondId, copy.ExperimentId);
                Assert.Same(secondProcedure, copy.Procedure);
                Assert.Same(secondOutcome, copy.ObservedOutcome);
            });
        Assert.Equal(
            ["ExperimentId", "ObservedOutcome", "Procedure"],
            typeof(JournalExperimentAuthoredCopy)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
        Assert.Collection(
            binding.Experiments.Records,
            record => Assert.Same(first, record),
            record => Assert.Same(second, record));
    }

    [Fact]
    public void ReturnsFreshCopiesInIndependentReadOnlyCollections()
    {
        ExperimentJournalRecord record = Record(
            new ExperimentId("trial"),
            "trial-occurred",
            "Test a bounded procedure.",
            "Record a qualitative outcome.");
        JournalExperimentSectionBinding binding = Binding(record);

        IReadOnlyList<JournalExperimentAuthoredCopy> first =
            JournalExperimentAuthoredCopyProjection.Project(binding);
        IReadOnlyList<JournalExperimentAuthoredCopy> second =
            JournalExperimentAuthoredCopyProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(Assert.Single(first), Assert.Single(second));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalExperimentAuthoredCopy>)first).Clear());
    }

    [Fact]
    public void AcceptsNoRecordedExperiments()
    {
        JournalExperimentSectionBinding binding = Binding();

        IReadOnlyList<JournalExperimentAuthoredCopy> copies =
            JournalExperimentAuthoredCopyProjection.Project(binding);

        Assert.Empty(copies);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalExperimentAuthoredCopy>)copies).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalExperimentAuthoredCopyProjection.Project(null!));
    }

    private static JournalExperimentSectionBinding Binding(
        params ExperimentJournalRecord[] records)
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
        return JournalExperimentSectionProjection.Bind(
            journal,
            new ExperimentJournalSnapshot(records));
    }

    private static ExperimentJournalRecord Record(
        ExperimentId experimentId,
        string eventId,
        string procedure,
        string observedOutcome) => new(
        experimentId,
        new WorldEvent(new WorldEventId(eventId), WorldEventTimeEffect.Advance),
        procedure,
        observedOutcome);
}
