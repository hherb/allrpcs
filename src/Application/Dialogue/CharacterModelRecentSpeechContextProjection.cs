using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects only recent in-world speech heard or spoken by one character into model context.
/// </summary>
public static class CharacterModelRecentSpeechContextProjection
{
    /// <summary>
    /// Returns fresh recent-speech entries in stable supplied order with exact utterance text.
    /// </summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        Character character,
        IEnumerable<DialogueEvent> recentDialogueEvents)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(recentDialogueEvents);

        List<CharacterModelContextEntry> entries = new();
        foreach (DialogueEvent dialogueEvent in recentDialogueEvents)
        {
            if (dialogueEvent is null)
            {
                throw new ArgumentException(
                    "Recent dialogue cannot contain null events.",
                    nameof(recentDialogueEvents));
            }

            bool isSpeaker = dialogueEvent.Utterance.SpeakerCharacterId == character.Identity.Id;
            bool isListener = dialogueEvent.Audience.ListenerCharacterIds.Contains(
                character.Identity.Id);
            if (isSpeaker || isListener)
            {
                entries.Add(new CharacterModelContextEntry(
                    CharacterModelContextEntryKind.RecentSpeech,
                    dialogueEvent.Utterance.Text,
                    dialogueEvent.Delivery.Mode));
            }
        }

        return entries.AsReadOnly();
    }
}
