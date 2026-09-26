using Allrpcs.Application.Journal;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalSnapshotProjectionTests
{
    [Fact]
    public void ComposesExactObservationsAndPendingSectionsInCanonicalOrder()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalObservationSectionSnapshot observations = Observations(
            catalogue,
            seekerId);

        JournalSnapshot snapshot = JournalSnapshotProjection.Compose(catalogue, observations);

        Assert.Same(catalogue, snapshot.Catalogue);
        Assert.Same(observations, snapshot.Observations);
        Assert.Same(seekerId, snapshot.SeekerId);
        Assert.Null(snapshot.Testimony);
        Assert.Null(snapshot.Hypotheses);
        Assert.Null(snapshot.Experiments);
        Assert.Null(snapshot.OpenQuestions);
        Assert.Collection(
            snapshot.PendingSections,
            section => Assert.Same(catalogue.Sections[1], section),
            section => Assert.Same(catalogue.Sections[2], section),
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
    }

    [Fact]
    public void ReturnsIndependentSnapshotsAndReadOnlyPendingCollections()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionSnapshot observations = Observations(
            catalogue,
            new CharacterId("seeker"));

        JournalSnapshot first = JournalSnapshotProjection.Compose(catalogue, observations);
        JournalSnapshot second = JournalSnapshotProjection.Compose(catalogue, observations);

        Assert.NotSame(first, second);
        Assert.NotSame(first.PendingSections, second.PendingSections);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)first.PendingSections).Clear());
        Assert.Collection(
            catalogue.Sections,
            section => Assert.Same(observations.Section, section),
            section => Assert.Same(first.PendingSections[0], section),
            section => Assert.Same(first.PendingSections[1], section),
            section => Assert.Same(first.PendingSections[2], section),
            section => Assert.Same(first.PendingSections[3], section));
    }

    [Fact]
    public void AcceptsAnEmptyObservationSectionWithoutCreatingPendingContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionSnapshot observations = Observations(
            catalogue,
            new CharacterId("seeker"));

        JournalSnapshot snapshot = JournalSnapshotProjection.Compose(catalogue, observations);

        Assert.Empty(snapshot.Observations.Entries);
        Assert.Equal(4, snapshot.PendingSections.Count);
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
    public void RejectsRebuiltAndForeignObservationSectionsWithoutChangingInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSectionCatalogue rebuiltCatalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionSnapshot rebuilt = Observations(rebuiltCatalogue, seekerId);
        JournalSectionCatalogue foreignCatalogue = new(
        [
            new("observations", "Foreign Observations"),
            new("testimony", "Foreign Testimony"),
            new("hypotheses", "Foreign Hypotheses"),
            new("experiments", "Foreign Experiments"),
            new("open-questions", "Foreign Questions"),
        ]);
        JournalObservationSectionSnapshot foreign = Observations(foreignCatalogue, seekerId);

        Assert.Throws<ArgumentException>(() =>
            JournalSnapshotProjection.Compose(catalogue, rebuilt));
        Assert.Throws<ArgumentException>(() =>
            JournalSnapshotProjection.Compose(catalogue, foreign));
        Assert.Same(seekerId, rebuilt.SeekerId);
        Assert.Empty(rebuilt.Entries);
        Assert.Same(foreignCatalogue.Sections[0], foreign.Section);
        Assert.Empty(foreign.Entries);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionSnapshot observations = Observations(
            catalogue,
            new CharacterId("seeker"));

        Assert.Throws<ArgumentNullException>(() =>
            JournalSnapshotProjection.Compose(null!, observations));
        Assert.Throws<ArgumentNullException>(() =>
            JournalSnapshotProjection.Compose(catalogue, null!));
    }

    private static JournalObservationSectionSnapshot Observations(
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
        return JournalObservationSectionSnapshotProjection.Project(world);
    }
}
