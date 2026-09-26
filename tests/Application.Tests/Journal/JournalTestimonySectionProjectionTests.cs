using Allrpcs.Application.Journal;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Tests.Journal;

public sealed class JournalTestimonySectionProjectionTests
{
    [Fact]
    public void BindsExactCanonicalTestimonyToDialogueHeardByTheSeekerInStableOrder()
    {
        JournalSnapshot journal = Journal(new CharacterId("seeker"));
        DialogueEvent firstHeard = Event("healer", "Watch the water.", new CharacterId("seeker"));
        DialogueEvent unrelated = Event("sage", "Keep this private.", new CharacterId("healer"));
        DialogueEvent spokenBySeeker = Event("seeker", "What happened?", new CharacterId("healer"));
        DialogueEvent secondHeard = Event(
            "traveller",
            "The wagon arrived yesterday.",
            new CharacterId("healer"),
            journal.SeekerId);
        DialogueEvent[] source = [firstHeard, unrelated, spokenBySeeker, secondHeard];

        JournalTestimonySectionBinding binding = JournalTestimonySectionProjection.Bind(
            journal,
            source);

        Assert.Same(journal, binding.Journal);
        Assert.Same(journal.Catalogue.Sections[1], binding.Section);
        Assert.Same(journal.SeekerId, binding.SeekerId);
        Assert.Collection(
            binding.HeardDialogueEvents,
            dialogueEvent => Assert.Same(firstHeard, dialogueEvent),
            dialogueEvent => Assert.Same(secondHeard, dialogueEvent));
        Assert.Collection(
            source,
            dialogueEvent => Assert.Same(firstHeard, dialogueEvent),
            dialogueEvent => Assert.Same(unrelated, dialogueEvent),
            dialogueEvent => Assert.Same(spokenBySeeker, dialogueEvent),
            dialogueEvent => Assert.Same(secondHeard, dialogueEvent));
    }

    [Fact]
    public void AcceptsValueEqualListenerIdentityWithoutAcceptingSeekerSpeech()
    {
        JournalSnapshot journal = Journal(new CharacterId("seeker"));
        CharacterId valueEqualSeeker = new("seeker");
        DialogueEvent heard = Event("healer", "Use the clean cup.", valueEqualSeeker);
        DialogueEvent spoken = Event("seeker", "I heard you.", new CharacterId("healer"));

        JournalTestimonySectionBinding binding = JournalTestimonySectionProjection.Bind(
            journal,
            [heard, spoken]);

        Assert.Same(heard, Assert.Single(binding.HeardDialogueEvents));
        Assert.NotSame(journal.SeekerId, valueEqualSeeker);
        Assert.Equal(journal.SeekerId, valueEqualSeeker);
    }

    [Fact]
    public void ReturnsIndependentReadOnlyCollectionsAndAcceptsNoDialogue()
    {
        JournalSnapshot journal = Journal(new CharacterId("seeker"));
        DialogueEvent heard = Event("healer", "Rest here.", journal.SeekerId);
        DialogueEvent[] source = [heard];

        JournalTestimonySectionBinding first = JournalTestimonySectionProjection.Bind(journal, source);
        JournalTestimonySectionBinding second = JournalTestimonySectionProjection.Bind(journal, source);
        source[0] = Event("sage", "Replacement.", journal.SeekerId);
        JournalTestimonySectionBinding empty = JournalTestimonySectionProjection.Bind(journal, []);

        Assert.NotSame(first, second);
        Assert.NotSame(first.HeardDialogueEvents, second.HeardDialogueEvents);
        Assert.Same(heard, Assert.Single(first.HeardDialogueEvents));
        Assert.Throws<NotSupportedException>(() =>
            ((IList<DialogueEvent>)first.HeardDialogueEvents).Clear());
        Assert.Empty(empty.HeardDialogueEvents);
    }

    [Fact]
    public void RejectsNullInputsAndEntriesWithoutChangingTheJournal()
    {
        JournalSnapshot journal = Journal(new CharacterId("seeker"));

        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonySectionProjection.Bind(null!, []));
        Assert.Throws<ArgumentNullException>(() =>
            JournalTestimonySectionProjection.Bind(journal, null!));
        Assert.Throws<ArgumentException>(() =>
            JournalTestimonySectionProjection.Bind(journal, [null!]));
        Assert.Empty(journal.Observations.Entries);
        Assert.Equal(4, journal.PendingSections.Count);
    }

    private static JournalSnapshot Journal(CharacterId seekerId)
    {
        JournalSectionCatalogue catalogue = RepresentativeJournalSectionSource.Create();
        JournalObservationSectionBinding observations = JournalObservationSectionProjection.Bind(
            catalogue,
            seekerId,
            new CharacterPerceptionSnapshot(seekerId, []));
        JournalObservationWorldBinding world = JournalObservationWorldProjection.Bind(
            observations,
            new WorldObjectSnapshot([]));
        JournalObservationSectionSnapshot observationSnapshot =
            JournalObservationSectionSnapshotProjection.Project(world);
        return JournalSnapshotProjection.Compose(catalogue, observationSnapshot);
    }

    private static DialogueEvent Event(
        string speaker,
        string text,
        params CharacterId[] listeners)
    {
        CharacterId speakerId = new(speaker);
        return new DialogueEvent(
            new DialogueDelivery(
                new DialogueUtterance(speakerId, text),
                DialogueDeliveryMode.OrdinarySpeech),
            new DialogueAudienceSnapshot(speakerId, listeners));
    }
}
