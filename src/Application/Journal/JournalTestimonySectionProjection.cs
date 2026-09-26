using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Journal;

/// <summary>Binds only dialogue explicitly heard by the Seeker to canonical testimony provenance.</summary>
public static class JournalTestimonySectionProjection
{
    private const int TestimonySectionIndex = 1;

    /// <summary>Returns exact heard events in stable order without deriving testimony content.</summary>
    public static JournalTestimonySectionBinding Bind(
        JournalSnapshot journal,
        IEnumerable<DialogueEvent> dialogueEvents)
    {
        ArgumentNullException.ThrowIfNull(journal);
        ArgumentNullException.ThrowIfNull(dialogueEvents);

        List<DialogueEvent> heardDialogueEvents = new();
        foreach (DialogueEvent dialogueEvent in dialogueEvents)
        {
            if (dialogueEvent is null)
            {
                throw new ArgumentException(
                    "Journal testimony dialogue cannot contain null events.",
                    nameof(dialogueEvents));
            }

            if (dialogueEvent.Audience.ListenerCharacterIds.Contains(journal.SeekerId))
            {
                heardDialogueEvents.Add(dialogueEvent);
            }
        }

        return new JournalTestimonySectionBinding(
            journal,
            journal.Catalogue.Sections[TestimonySectionIndex],
            heardDialogueEvents);
    }
}
