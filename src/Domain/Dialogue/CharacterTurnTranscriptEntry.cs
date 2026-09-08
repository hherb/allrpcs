using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one spoken statement or physical action as a structurally distinct transcript entry.
/// </summary>
public sealed record CharacterTurnTranscriptEntry
{
    /// <summary>Creates one transcript entry from exact spoken dialogue.</summary>
    public CharacterTurnTranscriptEntry(DialogueUtterance spokenStatement)
    {
        ArgumentNullException.ThrowIfNull(spokenStatement);

        Kind = CharacterTurnTranscriptEntryKind.SpokenStatement;
        CharacterId = spokenStatement.SpeakerCharacterId;
        SpokenStatement = spokenStatement;
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

    /// <summary>Gets the exact speech payload when this is a spoken statement.</summary>
    public DialogueUtterance? SpokenStatement { get; }

    /// <summary>Gets the exact intention payload when this is a physical action.</summary>
    public CharacterIntention? PhysicalAction { get; }
}
