using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Characters;

/// <summary>
/// Associates one character with one bounded non-wait intention without scheduling or executing it.
/// </summary>
public sealed record CharacterIndependentTaskAssignment
{
    /// <summary>Creates one immutable provider-neutral independent-task assignment.</summary>
    public CharacterIndependentTaskAssignment(
        CharacterIndependentTaskId taskId,
        CharacterId characterId,
        CharacterIntention intention)
    {
        ArgumentNullException.ThrowIfNull(taskId);
        ArgumentNullException.ThrowIfNull(characterId);
        ArgumentNullException.ThrowIfNull(intention);

        if (intention.Type == CharacterIntentionType.Wait)
        {
            throw new ArgumentException(
                "Independent tasks require a non-wait character intention.",
                nameof(intention));
        }

        TaskId = taskId;
        CharacterId = characterId;
        Intention = intention;
    }

    /// <summary>Gets the exact stable task identifier.</summary>
    public CharacterIndependentTaskId TaskId { get; }

    /// <summary>Gets the exact assigned character identifier.</summary>
    public CharacterId CharacterId { get; }

    /// <summary>Gets the exact non-wait intention proposed for the task.</summary>
    public CharacterIntention Intention { get; }
}
