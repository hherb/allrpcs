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
    /// supplied memories, recent local speech, exact validation affordances, and exact untrusted
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
        CharacterIntentionAffordanceSnapshot validAffordances,
        IEnumerable<CharacterModelContextEntry>? actionOutcomes = null)
    {
        CharacterModelContextAssemblyStatus status = TryAssemble(
            request, character, perception, worldObjects, knowledge, memoryEntries,
            recentDialogueEvents, validAffordances, out CharacterModelGenerationRequest? result, actionOutcomes);
        return status == CharacterModelContextAssemblyStatus.Success
            ? result!
            : throw new InvalidOperationException("Required model context exceeds the context budget.");
    }

    /// <summary>
    /// Fits optional context while preserving exact required constraints; reports an impossible
    /// mandatory budget without issuing a partial generation request. Invalid inputs still throw.
    /// </summary>
    public static CharacterModelContextAssemblyStatus TryAssemble(
        CharacterTurnRequest request,
        Character character,
        CharacterPerceptionSnapshot perception,
        WorldObjectSnapshot worldObjects,
        KnowledgeSnapshot knowledge,
        IEnumerable<CharacterModelContextEntry> memoryEntries,
        IEnumerable<DialogueEvent> recentDialogueEvents,
        CharacterIntentionAffordanceSnapshot validAffordances,
        out CharacterModelGenerationRequest? result,
        IEnumerable<CharacterModelContextEntry>? actionOutcomes = null)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(perception);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(knowledge);
        ArgumentNullException.ThrowIfNull(memoryEntries);
        ArgumentNullException.ThrowIfNull(recentDialogueEvents);
        ArgumentNullException.ThrowIfNull(validAffordances);

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
        List<CharacterModelContextEntry> outcomes = RetainExactKind(actionOutcomes ?? [],
            CharacterModelContextEntryKind.ActionOutcome, nameof(actionOutcomes));
        List<CharacterModelContextEntry> required = new(
            CharacterModelValidAffordanceContextProjection.Project(validAffordances));
        required.Add(CharacterModelPlayerDialogueContextProjection.Project(request));
        int remainingCount = CharacterModelContextSnapshot.MaximumEntryCount - required.Count;
        int remainingLength = CharacterModelContextSnapshot.MaximumTotalTextLength
            - required.Sum(entry => entry.Text.Length);
        // Projection validates all optional-input invariants even when mandatory context cannot fit.
        IReadOnlyList<CharacterModelContextEntry>[] categories =
        [
            CharacterModelPerceptionContextProjection.Project(character, perception, worldObjects),
            CharacterModelKnowledgeContextProjection.Project(character, knowledge),
            CharacterModelGoalContextProjection.Project(character),
            memories,
            outcomes,
            CharacterModelRecentSpeechContextProjection.Project(character, recentDialogueEvents),
        ];
        if (remainingCount < 0 || remainingLength < 0)
        {
            result = null;
            return CharacterModelContextAssemblyStatus.RequiredContextBudgetExceeded;
        }

        List<CharacterModelContextEntry>[] retained = categories.Select(_ => new List<CharacterModelContextEntry>())
            .ToArray();
        // Protect completed outcomes and authored goals before bulk perception/knowledge,
        // then prefer newest memory and local speech. Emit in the original category order.
        foreach (int category in new[] { 4, 2, 0, 1, 3, 5 })
        {
            Select(categories[category], retained[category], category is 3 or 4 or 5);
        }

        List<CharacterModelContextEntry> entries = retained.SelectMany(category => category).ToList();
        entries.AddRange(required);
        result = new CharacterModelGenerationRequest(request, new CharacterModelContextSnapshot(entries));
        return CharacterModelContextAssemblyStatus.Success;

        void Select(
            IEnumerable<CharacterModelContextEntry> candidates,
            List<CharacterModelContextEntry> selected,
            bool newestFirst)
        {
            foreach (CharacterModelContextEntry entry in newestFirst ? candidates.Reverse() : candidates)
            {
                if (remainingCount == 0)
                {
                    break;
                }

                if (entry.Text.Length > remainingLength)
                {
                    continue;
                }

                selected.Add(entry);
                remainingCount--;
                remainingLength -= entry.Text.Length;
            }

            if (newestFirst)
            {
                selected.Reverse();
            }
        }
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
