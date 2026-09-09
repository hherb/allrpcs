using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;
using Allrpcs.Domain.Knowledge;

namespace Allrpcs.Application.Dialogue;

/// <summary>Projects only authored knowledge referenced by one character into model context.</summary>
public static class CharacterModelKnowledgeContextProjection
{
    /// <summary>Returns fresh relevant-knowledge entries in stable character-reference order.</summary>
    public static IReadOnlyList<CharacterModelContextEntry> Project(
        Character character,
        KnowledgeSnapshot knowledge)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(knowledge);

        List<CharacterModelContextEntry> entries = new();
        foreach (KnowledgeId reference in character.Knowledge.KnowledgeReferences)
        {
            KnowledgeEntry entry = KnowledgeLookup.FindById(reference, knowledge)
                ?? throw new ArgumentException(
                    "Every character knowledge reference must exist in the authored snapshot.",
                    nameof(knowledge));

            entries.Add(new CharacterModelContextEntry(
                CharacterModelContextEntryKind.RelevantKnowledge,
                entry.Text));
        }

        return entries.AsReadOnly();
    }
}
