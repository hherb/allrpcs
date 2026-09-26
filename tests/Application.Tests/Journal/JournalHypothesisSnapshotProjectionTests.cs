using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Investigations;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalHypothesisSnapshotProjectionTests
{
    [Fact]
    public void InstallsExactHypothesesAndRetainsRemainingPendingSectionsInCanonicalOrder()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithTestimony(catalogue, seekerId);
        InvestigationQuestion question = new(
            new InvestigationQuestionId("what-causes-grey-sleep"),
            "What causes the grey sleep?");
        InvestigationHypothesis hypothesis = new(
            new InvestigationHypothesisId("stream-water"),
            question,
            "The stream water may carry the illness.");
        JournalHypothesisSectionSnapshot hypotheses = Hypotheses(journal, hypothesis);

        JournalSnapshot result = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);

        Assert.NotSame(journal, result);
        Assert.Same(catalogue, result.Catalogue);
        Assert.Same(journal.Observations, result.Observations);
        Assert.Same(journal.Testimony, result.Testimony);
        Assert.Same(seekerId, result.SeekerId);
        Assert.Same(hypotheses, result.Hypotheses);
        Assert.Null(result.Experiments);
        Assert.Null(result.OpenQuestions);
        Assert.Collection(
            result.PendingSections,
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
        Assert.Collection(
            journal.PendingSections,
            section => Assert.Same(catalogue.Sections[2], section),
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
    }

    [Fact]
    public void ReturnsFreshJournalsWithIndependentReadOnlyPendingCollections()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithTestimony(catalogue, new CharacterId("seeker"));
        JournalHypothesisSectionSnapshot hypotheses = Hypotheses(journal);

        JournalSnapshot first = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);
        JournalSnapshot second = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);

        Assert.NotSame(first, second);
        Assert.NotSame(first.PendingSections, second.PendingSections);
        Assert.Same(hypotheses, first.Hypotheses);
        Assert.Same(hypotheses, second.Hypotheses);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)first.PendingSections).Clear());
    }

    [Fact]
    public void AcceptsEmptyHypothesesWithoutCreatingOtherPendingContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = JournalWithTestimony(catalogue, new CharacterId("seeker"));
        JournalHypothesisSectionSnapshot hypotheses = Hypotheses(journal);

        JournalSnapshot result = JournalHypothesisSnapshotProjection.Compose(journal, hypotheses);

        Assert.Empty(result.Hypotheses!.Entries);
        Assert.Equal(2, result.PendingSections.Count);
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
    public void RejectsForeignSectionSeekerAndAlreadyInstalledHypothesesWithoutChangingInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = JournalWithTestimony(catalogue, seekerId);
        JournalHypothesisSectionSnapshot valid = Hypotheses(journal);
        JournalSectionCatalogue rebuiltCatalogue = RepresentativeJournalSectionSource.Create();
        JournalHypothesisSectionSnapshot rebuilt = Hypotheses(
            JournalWithTestimony(rebuiltCatalogue, seekerId));
        CharacterId valueEqualSeekerId = new("seeker");
        JournalHypothesisSectionSnapshot valueEqualSeeker = Hypotheses(
            JournalWithTestimony(catalogue, valueEqualSeekerId));
        JournalSnapshot installed = JournalHypothesisSnapshotProjection.Compose(journal, valid);

        Assert.Throws<ArgumentException>(() =>
            JournalHypothesisSnapshotProjection.Compose(journal, rebuilt));
        Assert.Throws<ArgumentException>(() =>
            JournalHypothesisSnapshotProjection.Compose(journal, valueEqualSeeker));
        Assert.Throws<ArgumentException>(() =>
            JournalHypothesisSnapshotProjection.Compose(installed, valid));
        Assert.Null(journal.Hypotheses);
        Assert.Equal(3, journal.PendingSections.Count);
        Assert.Same(rebuiltCatalogue.Sections[2], rebuilt.Section);
        Assert.Same(valueEqualSeekerId, valueEqualSeeker.SeekerId);
        Assert.Same(valid, installed.Hypotheses);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = JournalWithTestimony(
            RepresentativeJournalSectionSource.Create(),
            new CharacterId("seeker"));
        JournalHypothesisSectionSnapshot hypotheses = Hypotheses(journal);

        Assert.Throws<ArgumentNullException>(() =>
            JournalHypothesisSnapshotProjection.Compose(null!, hypotheses));
        Assert.Throws<ArgumentNullException>(() =>
            JournalHypothesisSnapshotProjection.Compose(journal, null!));
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

    private static JournalHypothesisSectionSnapshot Hypotheses(
        JournalSnapshot journal,
        params InvestigationHypothesis[] hypotheses) =>
        JournalHypothesisSectionSnapshotProjection.Project(
            JournalHypothesisSectionProjection.Bind(
                journal,
                new InvestigationHypothesisSnapshot(hypotheses)));
}
