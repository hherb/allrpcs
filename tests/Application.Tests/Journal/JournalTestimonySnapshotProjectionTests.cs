using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalTestimonySnapshotProjectionTests
{
    [Fact]
    public void InstallsExactTestimonyAndRetainsRemainingPendingSectionsInCanonicalOrder()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = Journal(catalogue, seekerId);
        JournalTestimonySectionSnapshot testimony = Testimony(
            journal,
            Event(
                new CharacterId("healer"),
                "The stream changed after the rain.",
                new CharacterId("seeker")));

        JournalSnapshot result = JournalTestimonySnapshotProjection.Compose(journal, testimony);

        Assert.NotSame(journal, result);
        Assert.Same(catalogue, result.Catalogue);
        Assert.Same(journal.Observations, result.Observations);
        Assert.Same(seekerId, result.SeekerId);
        Assert.Same(testimony, result.Testimony);
        Assert.Null(result.Hypotheses);
        Assert.Null(result.Experiments);
        Assert.Null(result.OpenQuestions);
        Assert.Collection(
            result.PendingSections,
            section => Assert.Same(catalogue.Sections[2], section),
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
        Assert.Collection(
            journal.PendingSections,
            section => Assert.Same(catalogue.Sections[1], section),
            section => Assert.Same(catalogue.Sections[2], section),
            section => Assert.Same(catalogue.Sections[3], section),
            section => Assert.Same(catalogue.Sections[4], section));
    }

    [Fact]
    public void ReturnsFreshJournalsWithIndependentReadOnlyPendingCollections()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = Journal(catalogue, new CharacterId("seeker"));
        JournalTestimonySectionSnapshot testimony = Testimony(journal);

        JournalSnapshot first = JournalTestimonySnapshotProjection.Compose(journal, testimony);
        JournalSnapshot second = JournalTestimonySnapshotProjection.Compose(journal, testimony);

        Assert.NotSame(first, second);
        Assert.NotSame(first.PendingSections, second.PendingSections);
        Assert.Same(testimony, first.Testimony);
        Assert.Same(testimony, second.Testimony);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalSectionDefinition>)first.PendingSections).Clear());
    }

    [Fact]
    public void AcceptsEmptyTestimonyWithoutCreatingOtherPendingContent()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalSnapshot journal = Journal(catalogue, new CharacterId("seeker"));
        JournalTestimonySectionSnapshot testimony = Testimony(journal);

        JournalSnapshot result = JournalTestimonySnapshotProjection.Compose(journal, testimony);

        Assert.Empty(result.Testimony!.Entries);
        Assert.Equal(3, result.PendingSections.Count);
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
    public void RejectsForeignSectionSeekerAndAlreadyInstalledTestimonyWithoutChangingInputs()
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        CharacterId seekerId = new("seeker");
        JournalSnapshot journal = Journal(catalogue, seekerId);
        JournalTestimonySectionSnapshot valid = Testimony(journal);
        JournalSectionCatalogue rebuiltCatalogue = RepresentativeJournalSectionSource.Create();
        JournalTestimonySectionSnapshot rebuilt = Testimony(
            Journal(rebuiltCatalogue, seekerId));
        CharacterId valueEqualSeekerId = new("seeker");
        JournalTestimonySectionSnapshot valueEqualSeeker = Testimony(
            Journal(catalogue, valueEqualSeekerId));
        JournalSnapshot installed = JournalTestimonySnapshotProjection.Compose(journal, valid);

        Assert.Throws<ArgumentException>(() =>
            JournalTestimonySnapshotProjection.Compose(journal, rebuilt));
        Assert.Throws<ArgumentException>(() =>
            JournalTestimonySnapshotProjection.Compose(journal, valueEqualSeeker));
        Assert.Throws<ArgumentException>(() =>
            JournalTestimonySnapshotProjection.Compose(installed, valid));
        Assert.Null(journal.Testimony);
        Assert.Equal(4, journal.PendingSections.Count);
        Assert.Same(rebuiltCatalogue.Sections[1], rebuilt.Section);
        Assert.Same(valueEqualSeekerId, valueEqualSeeker.SeekerId);
        Assert.Same(valid, installed.Testimony);
    }

    [Fact]
    public void RejectsNullInputs()
    {
        JournalSnapshot journal = Journal(
            RepresentativeJournalSectionSource.Create(),
            new CharacterId("seeker"));
        JournalTestimonySectionSnapshot testimony = Testimony(journal);

        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonySnapshotProjection.Compose(null!, testimony));
        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonySnapshotProjection.Compose(journal, null!));
    }

    private static JournalSnapshot Journal(
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
        return JournalSnapshotProjection.Compose(
            catalogue,
            JournalObservationSectionSnapshotProjection.Project(world));
    }

    private static JournalTestimonySectionSnapshot Testimony(
        JournalSnapshot journal,
        params DialogueEvent[] events) => JournalTestimonySectionSnapshotProjection.Project(
        JournalTestimonySectionProjection.Bind(journal, events));

    private static DialogueEvent Event(
        CharacterId speakerId,
        string text,
        params CharacterId[] listeners) => new(
        new DialogueDelivery(
            new DialogueUtterance(speakerId, text),
            DialogueDeliveryMode.OrdinarySpeech),
        new DialogueAudienceSnapshot(speakerId, listeners));
}
