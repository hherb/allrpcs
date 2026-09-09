using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Application.Dialogue;

/// <summary>
/// Projects exact player dialogue into a provider-neutral untrusted in-world context entry.
/// </summary>
public static class CharacterModelPlayerDialogueContextProjection
{
    /// <summary>
    /// Returns one fresh entry retaining the request's exact player dialogue text.
    /// </summary>
    public static CharacterModelContextEntry Project(CharacterTurnRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return new CharacterModelContextEntry(
            CharacterModelContextEntryKind.UntrustedPlayerDialogue,
            request.DialogueEvent.Utterance.Text);
    }
}
