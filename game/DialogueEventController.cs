using Allrpcs.Application.Dialogue;
using Allrpcs.Domain.Dialogue;
using Godot;
using NumericsVector2 = System.Numerics.Vector2;

namespace Allrpcs.Game;

/// <summary>Owns the most recent in-world dialogue event selected from one hearing query.</summary>
public partial class DialogueEventController : Node
{
    /// <summary>Gets the exact most recently occurred dialogue event, if any.</summary>
    public DialogueEvent? LastOccurredEvent { get; private set; }

    /// <summary>
    /// Selects an audience and records an exact event when the hearing query is valid.
    /// </summary>
    public bool TryOccur(
        DialogueUtterance utterance,
        NumericsVector2 speakerPosition,
        float hearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(utterance);
        ArgumentNullException.ThrowIfNull(candidates);

        try
        {
            DialogueAudienceSnapshot audience = DialogueHearingSelector.Select(
                utterance.SpeakerCharacterId,
                speakerPosition,
                hearingRangePixels,
                candidates);
            LastOccurredEvent = new DialogueEvent(utterance, audience);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
