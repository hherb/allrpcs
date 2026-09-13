using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one spoken statement or physical action as a structurally distinct transcript entry.
/// </summary>
public sealed record CharacterTurnTranscriptEntry
{
    /// <summary>Creates one transcript entry from one exact spoken delivery.</summary>
    public CharacterTurnTranscriptEntry(DialogueDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(delivery);

        Kind = CharacterTurnTranscriptEntryKind.SpokenStatement;
        CharacterId = delivery.Utterance.SpeakerCharacterId;
        Delivery = delivery;
    }

    /// <summary>Creates one transcript entry from an exact acting character and physical action.</summary>
    public CharacterTurnTranscriptEntry(
        CharacterId characterId,
        CharacterIntention physicalAction)
    {
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(physicalAction);

        if (physicalAction.Type == CharacterIntentionType.Wait)
        {
            throw new ArgumentException(
                "A wait intention is not a physical action.",
                nameof(physicalAction));
        }

        Kind = CharacterTurnTranscriptEntryKind.PhysicalAction;
        CharacterId = characterId;
        PhysicalAction = physicalAction;
    }

    /// <summary>Gets the exact defined transcript-entry kind.</summary>
    public CharacterTurnTranscriptEntryKind Kind { get; }

    /// <summary>Gets the exact identifier of the speaking or acting character.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact delivery when this is a spoken statement.</summary>
    public DialogueDelivery? Delivery { get; }

    /// <summary>Gets the exact delivered utterance when this is a spoken statement.</summary>
    public DialogueUtterance? SpokenStatement => Delivery?.Utterance;

    /// <summary>Gets the exact intention payload when this is a physical action.</summary>
    public CharacterIntention? PhysicalAction { get; }
}
