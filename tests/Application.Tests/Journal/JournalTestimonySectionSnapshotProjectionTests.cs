using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalTestimonySectionSnapshotProjectionTests
{
    [Fact]
    public void ProjectsExactSectionSeekerAndSafeTestimonyInStableOrder()
    {
        CharacterId healerId = new("healer");
        CharacterId travellerId = new("traveller");
        string firstText = "  Rest by the stream.  ";
        string secondText = "The wagon arrived before dawn.";
        DialogueEvent first = Event(
            healerId,
            firstText,
            DialogueDeliveryMode.Whispering,
            new CharacterId("seeker"));
        DialogueEvent second = Event(
            travellerId,
            secondText,
            DialogueDeliveryMode.OrdinarySpeech,
            new CharacterId("seeker"));
        JournalTestimonySectionBinding binding = Binding(first, second);

        JournalTestimonySectionSnapshot snapshot =
            JournalTestimonySectionSnapshotProjection.Project(binding);

        Assert.Same(binding.Section, snapshot.Section);
        Assert.Same(binding.SeekerId, snapshot.SeekerId);
        Assert.Collection(
            snapshot.Entries,
            copy =>
            {
                Assert.Same(healerId, copy.SpeakerCharacterId);
                Assert.Same(firstText, copy.Text);
                Assert.Same(first.Delivery.Mode, copy.DeliveryMode);
            },
            copy =>
            {
                Assert.Same(travellerId, copy.SpeakerCharacterId);
                Assert.Same(secondText, copy.Text);
                Assert.Same(second.Delivery.Mode, copy.DeliveryMode);
            });
        Assert.Collection(
            binding.HeardDialogueEvents,
            dialogueEvent => Assert.Same(first, dialogueEvent),
            dialogueEvent => Assert.Same(second, dialogueEvent));
    }

    [Fact]
    public void ReturnsFreshSnapshotsCopiesAndIndependentReadOnlyCollections()
    {
        DialogueEvent heard = Event(
            new CharacterId("healer"),
            "Rest here.",
            DialogueDeliveryMode.OrdinarySpeech,
            new CharacterId("seeker"));
        JournalTestimonySectionBinding binding = Binding(heard);

        JournalTestimonySectionSnapshot first =
            JournalTestimonySectionSnapshotProjection.Project(binding);
        JournalTestimonySectionSnapshot second =
            JournalTestimonySectionSnapshotProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(first.Entries, second.Entries);
        Assert.NotSame(Assert.Single(first.Entries), Assert.Single(second.Entries));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalTestimonyAuthoredCopy>)first.Entries).Clear());
    }

    [Fact]
    public void AcceptsEmptyTestimonyWithoutExposingSourceProvenance()
    {
        JournalTestimonySectionBinding binding = Binding();

        JournalTestimonySectionSnapshot snapshot =
            JournalTestimonySectionSnapshotProjection.Project(binding);

        Assert.Empty(snapshot.Entries);
        Assert.Equal(
            ["Entries", "Section", "SeekerId"],
            typeof(JournalTestimonySectionSnapshot)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalTestimonyAuthoredCopy>)snapshot.Entries).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonySectionSnapshotProjection.Project(null!));
    }

    private static JournalTestimonySectionBinding Binding(params DialogueEvent[] events)
    {
        CharacterId seekerId = new("seeker");
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
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
        return JournalTestimonySectionProjection.Bind(journal, events);
    }

    private static DialogueEvent Event(
        CharacterId speakerId,
        string text,
        DialogueDeliveryMode mode,
        params CharacterId[] listeners) => new(
        new DialogueDelivery(new DialogueUtterance(speakerId, text), mode),
        new DialogueAudienceSnapshot(speakerId, listeners));
}
