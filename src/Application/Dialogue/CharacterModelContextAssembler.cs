using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Assembles already filtered typed context with exact untrusted player dialogue.
/// </summary>
public static class CharacterModelContextAssembler
{
    /// <summary>
    /// Returns a generation request containing authoritative identified perception, supplied
    /// filtered entries, the responding character's goals and recent local speech, then exact
    /// untrusted player dialogue.
    /// </summary>
    public static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        Character character,
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects,
        KnowledgeSnapshot knowledge,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        IEnumerable<CharacterModelContextEntry> filteredEntries)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(knowledge);
        ArgumentNullException.ThrowIfNull(recentDialogueEvents);
        ArgumentNullException.ThrowIfNull(filteredEntries);

        if (character.Identity.Id != request.RespondingCharacterId)
        {
            throw new ArgumentException(
                "The supplied character must be the request's responding character.",
                nameof(character));
        }

        List<CharacterModelContextEntry> suppliedEntries = new();
        foreach (CharacterModelContextEntry entry in filteredEntries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Filtered model context cannot contain null entries.",
                    nameof(filteredEntries));
            }

            if (entry.Kind is CharacterModelContextEntryKind.FilteredPerception
                or CharacterModelContextEntryKind.RelevantKnowledge
                or CharacterModelContextEntryKind.Goal
                or CharacterModelContextEntryKind.RecentSpeech
                or CharacterModelContextEntryKind.UntrustedPlayerDialogue)
            {
                throw new ArgumentException(
                    "The assembler alone supplies perception, knowledge, goals, speech, and player dialogue.",
                    nameof(filteredEntries));
            }

            suppliedEntries.Add(entry);
        }

        List<CharacterModelContextEntry> entries = new();
        entries.AddRange(CharacterModelPerceptionContextProjection.Project(
            character,
            perception,
            worldObjects));
        entries.AddRange(CharacterModelKnowledgeContextProjection.Project(character, knowledge));
        entries.AddRange(suppliedEntries);
        entries.AddRange(CharacterModelGoalContextProjection.Project(character));
        entries.AddRange(CharacterModelRecentSpeechContextProjection.Project(
            character,
            recentDialogueEvents));
        entries.Add(CharacterModelPlayerDialogueContextProjection.Project(request));
        CharacterModelContextSnapshot context = new(entries);

        return new CharacterModelGenerationRequest(request, context);
    }
}
