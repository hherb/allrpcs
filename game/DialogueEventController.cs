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

    /// <summary>Gets bounded session speech with its original hearing audiences.</summary>
    internal CharacterConversationHistory History { get; } = new();

    private SaveSnapshotRestorationController? restoration;
    private CharacterMemoryController? memory;

    /// <inheritdoc />
    public override void _Ready()
    {
        restoration = GetNodeOrNull<SaveSnapshotRestorationController>("../SaveSnapshotRestorationController");
        memory = GetNodeOrNull<CharacterMemoryController>("../CharacterMemoryController");
        if (restoration is not null) { restoration.PresentationReset += OnRestored; }
    }

    /// <inheritdoc />
    public override void _ExitTree()
    {
        if (restoration is not null) { restoration.PresentationReset -= OnRestored; }
    }

    private void OnRestored()
    {
        History.Clear();
        // The restored memory already holds each character's saved recent conversation.
        if (memory is not null) { History.RestoreFrom(memory.Memory); }
        LastOccurredEvent = null;
    }

    /// <summary>
    /// Selects an audience and records an exact event when the hearing query is valid.
    /// </summary>
    public bool TryOccur(
        DialogueDelivery delivery,
        NumericsVector2 speakerPosition,
        float ordinaryHearingRangePixels,
        float whisperHearingRangePixels,
        IReadOnlyList<DialogueHearingCandidate> candidates)
    {
        ArgumentNullException.ThrowIfNull(delivery);
        ArgumentNullException.ThrowIfNull(candidates);

        try
        {
            DialogueAudienceSnapshot audience = DialogueDeliveryHearingSelector.Select(
                delivery,
                speakerPosition,
                ordinaryHearingRangePixels,
                whisperHearingRangePixels,
                candidates);
            LastOccurredEvent = new DialogueEvent(delivery, audience);
            History.Record(LastOccurredEvent);
            return true;
        }
        catch (ArgumentException)
        {
            return false;
        }
    }
}
