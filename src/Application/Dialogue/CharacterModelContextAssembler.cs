using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Assembles already filtered typed context with exact untrusted player dialogue.
/// </summary>
public static class CharacterModelContextAssembler
{
    /// <summary>
    /// Returns a generation request retaining supplied filtered entries in stable order, then
    /// appending the responding character's authored goals, recent local speech, and exact
    /// untrusted player dialogue.
    /// </summary>
    public static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        Character character,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        IEnumerable<CharacterModelContextEntry> filteredEntries)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(recentDialogueEvents);
        ArgumentNullException.ThrowIfNull(filteredEntries);

        if (character.Identity.Id != request.RespondingCharacterId)
        {
            throw new ArgumentException(
                "The supplied character must be the request's responding character.",
                nameof(character));
        }

        List<CharacterModelContextEntry> entries = new();
        foreach (CharacterModelContextEntry entry in filteredEntries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Filtered model context cannot contain null entries.",
                    nameof(filteredEntries));
            }

            if (entry.Kind is CharacterModelContextEntryKind.Goal
                or CharacterModelContextEntryKind.RecentSpeech
                or CharacterModelContextEntryKind.UntrustedPlayerDialogue)
            {
                throw new ArgumentException(
                    "The assembler alone supplies goals, recent speech, and untrusted player dialogue.",
                    nameof(filteredEntries));
            }

            entries.Add(entry);
        }

        entries.AddRange(CharacterModelGoalContextProjection.Project(character));
        entries.AddRange(CharacterModelRecentSpeechContextProjection.Project(
            character,
            recentDialogueEvents));
        entries.Add(CharacterModelPlayerDialogueContextProjection.Project(request));
        CharacterModelContextSnapshot context = new(entries);

        return new CharacterModelGenerationRequest(request, context);
    }
}
