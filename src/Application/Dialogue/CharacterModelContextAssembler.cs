using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Assembles already filtered typed context with exact untrusted player dialogue.
/// </summary>
public static class CharacterModelContextAssembler
{
    /// <summary>
    /// Returns a generation request retaining supplied filtered entries in stable order and
    /// appending the exact player dialogue with its untrusted in-world role.
    /// </summary>
    public static CharacterModelGenerationRequest Assemble(
        CharacterTurnRequest request,
        IEnumerable<CharacterModelContextEntry> filteredEntries)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(filteredEntries);

        List<CharacterModelContextEntry> entries = new();
        foreach (CharacterModelContextEntry entry in filteredEntries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Filtered model context cannot contain null entries.",
                    nameof(filteredEntries));
            }

            if (entry.Kind == CharacterModelContextEntryKind.UntrustedPlayerDialogue)
            {
                throw new ArgumentException(
                    "The assembler alone supplies untrusted player dialogue.",
                    nameof(filteredEntries));
            }

            entries.Add(entry);
        }

        entries.Add(CharacterModelPlayerDialogueContextProjection.Project(request));
        CharacterModelContextSnapshot context = new(entries);

        return new CharacterModelGenerationRequest(request, context);
    }
}
