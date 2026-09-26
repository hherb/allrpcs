using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Journal;

/// <summary>Projects audience-free authored testimony from validated heard-dialogue provenance.</summary>
public static class JournalTestimonyAuthoredCopyProjection
{
    /// <summary>Returns fresh read-only copies in stable heard-dialogue order.</summary>
    public static IReadOnlyList<JournalTestimonyAuthoredCopy> Project(
        JournalTestimonySectionBinding binding)
    {
        ArgumentNullException.ThrowIfNull(binding);

        List<JournalTestimonyAuthoredCopy> copies = new(binding.HeardDialogueEvents.Count);
        foreach (DialogueEvent dialogueEvent in binding.HeardDialogueEvents)
        {
            copies.Add(new JournalTestimonyAuthoredCopy(
                dialogueEvent.Utterance.SpeakerCharacterId,
                dialogueEvent.Utterance.Text,
                dialogueEvent.Delivery.Mode));
        }

        return copies.AsReadOnly();
    }
}
