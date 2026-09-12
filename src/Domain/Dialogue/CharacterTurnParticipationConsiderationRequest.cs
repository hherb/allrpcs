using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Presents one exact scheduled listener with heard dialogue for a provider-neutral participation choice.
/// </summary>
public sealed record CharacterTurnParticipationConsiderationRequest
{
    /// <summary>
    /// Creates an immutable request for a scheduled character present in the exact event audience.
    /// </summary>
    public CharacterTurnParticipationConsiderationRequest(
        Character scheduledCharacter,
        DialogueEvent heardDialogueEvent,
        bool isDirectlyAddressed)
    {
        ArgumentNullException.ThrowIfNull(scheduledCharacter);
        ArgumentNullException.ThrowIfNull(heardDialogueEvent);

        if (!heardDialogueEvent.Audience.ListenerCharacterIds.Contains(scheduledCharacter.Identity.Id))
        {
            throw new ArgumentException(
                "The scheduled character must be a listener in the heard dialogue audience.",
                nameof(scheduledCharacter));
        }

        ScheduledCharacter = scheduledCharacter;
        HeardDialogueEvent = heardDialogueEvent;
        IsDirectlyAddressed = isDirectlyAddressed;
    }

    /// <summary>Gets the exact scheduled character asked to consider participation.</summary>
    public Character ScheduledCharacter { get; }

    /// <summary>Gets the exact in-world dialogue event heard by the scheduled character.</summary>
    public DialogueEvent HeardDialogueEvent { get; }

    /// <summary>Gets whether the scheduled character was directly addressed.</summary>
    public bool IsDirectlyAddressed { get; }
}
