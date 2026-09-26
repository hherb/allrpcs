using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalTestimonyAuthoredCopyProjectionTests
{
    [Fact]
    public void CopiesSafeHeardDialogueInStableOrderWithoutAudienceExposure()
    {
        CharacterId healerId = new("healer");
        CharacterId travellerId = new("traveller");
        string firstText = "  The stream changed after the rain.  ";
        string secondText = "I saw the wagon arrive.";
        DialogueEvent first = Event(
            healerId,
            firstText,
            DialogueDeliveryMode.Whispering,
            new CharacterId("seeker"));
        DialogueEvent second = Event(
            travellerId,
            secondText,
            DialogueDeliveryMode.OrdinarySpeech,
            new CharacterId("seeker"),
            new CharacterId("healer"));
        JournalTestimonySectionBinding binding = Binding(first, second);

        IReadOnlyList<JournalTestimonyAuthoredCopy> copies =
            JournalTestimonyAuthoredCopyProjection.Project(binding);

        Assert.Collection(
            copies,
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
        Assert.Equal(
            ["DeliveryMode", "SpeakerCharacterId", "Text"],
            typeof(JournalTestimonyAuthoredCopy)
                .GetProperties()
                .Select(property => property.Name)
                .Order());
        Assert.Collection(
            binding.HeardDialogueEvents,
            dialogueEvent => Assert.Same(first, dialogueEvent),
            dialogueEvent => Assert.Same(second, dialogueEvent));
    }

    [Fact]
    public void ReturnsFreshCopiesInIndependentReadOnlyCollections()
    {
        DialogueEvent heard = Event(
            new CharacterId("healer"),
            "Rest here.",
            DialogueDeliveryMode.OrdinarySpeech,
            new CharacterId("seeker"));
        JournalTestimonySectionBinding binding = Binding(heard);

        IReadOnlyList<JournalTestimonyAuthoredCopy> first =
            JournalTestimonyAuthoredCopyProjection.Project(binding);
        IReadOnlyList<JournalTestimonyAuthoredCopy> second =
            JournalTestimonyAuthoredCopyProjection.Project(binding);

        Assert.NotSame(first, second);
        Assert.NotSame(Assert.Single(first), Assert.Single(second));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalTestimonyAuthoredCopy>)first).Clear());
    }

    [Fact]
    public void AcceptsNoHeardDialogue()
    {
        JournalTestimonySectionBinding binding = Binding();

        IReadOnlyList<JournalTestimonyAuthoredCopy> copies =
            JournalTestimonyAuthoredCopyProjection.Project(binding);

        Assert.Empty(copies);
        Assert.Throws<NotSupportedException>(() =>
            ((IList<JournalTestimonyAuthoredCopy>)copies).Clear());
    }

    [Fact]
    public void RejectsNullBinding()
    {
        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonyAuthoredCopyProjection.Project(null!));
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
