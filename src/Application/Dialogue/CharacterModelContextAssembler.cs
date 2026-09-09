using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Knowledge;
using Allrpcs.Domain.Perception;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Assembles authoritative and separately typed supplied context with exact untrusted player dialogue.
/// </summary>
public static class CharacterModelContextAssembler
{
    /// <summary>
    /// Returns a generation request containing authoritative perception, knowledge and goals,
    /// supplied memories, recent local speech, supplied valid affordances, and exact untrusted
    /// player dialogue in roadmap order.
    /// </summary>
    public static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        Character character,
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects,
        KnowledgeSnapshot knowledge,
        IEnumerable<CharacterModelContextEntry> memoryEntries,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        IEnumerable<CharacterModelContextEntry> validAffordanceEntries)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(knowledge);
        ArgumentNullException.ThrowIfNull(memoryEntries);
        ArgumentNullException.ThrowIfNull(recentDialogueEvents);
        ArgumentNullException.ThrowIfNull(validAffordanceEntries);

        if (character.Identity.Id != request.RespondingCharacterId)
        {
            throw new ArgumentException(
                "The supplied character must be the request's responding character.",
                nameof(character));
        }

        List<CharacterModelContextEntry> memories = RetainExactKind(
            memoryEntries,
            CharacterModelContextEntryKind.Memory,
            nameof(memoryEntries));
        List<CharacterModelContextEntry> validAffordances = RetainExactKind(
            validAffordanceEntries,
            CharacterModelContextEntryKind.ValidAffordance,
            nameof(validAffordanceEntries));

        List<CharacterModelContextEntry> entries = new();
        entries.AddRange(CharacterModelPerceptionContextProjection.Project(
            character,
            perception,
            worldObjects));
        entries.AddRange(CharacterModelKnowledgeContextProjection.Project(character, knowledge));
        entries.AddRange(CharacterModelGoalContextProjection.Project(character));
        entries.AddRange(memories);
        entries.AddRange(CharacterModelRecentSpeechContextProjection.Project(
            character,
            recentDialogueEvents));
        entries.AddRange(validAffordances);
        entries.Add(CharacterModelPlayerDialogueContextProjection.Project(request));
        CharacterModelContextSnapshot context = new(entries);

        return new CharacterModelGenerationRequest(request, context);
    }

    private static List<CharacterModelContextEntry> RetainExactKind(
        IEnumerable<CharacterModelContextEntry> entries,
        CharacterModelContextEntryKind requiredKind,
        string parameterName)
    {
        List<CharacterModelContextEntry> retainedEntries = new();
        foreach (CharacterModelContextEntry entry in entries)
        {
            if (entry is null || entry.Kind != requiredKind)
            {
                throw new ArgumentException(
                    $"Supplied entries must all have the {requiredKind} kind.",
                    parameterName);
            }

            retainedEntries.Add(entry);
        }

        return retainedEntries;
    }
}
