using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Retains one scheduled character's explicit provider-neutral participation choice.
/// </summary>
public sealed record CharacterTurnParticipationDecision
{
    /// <summary>
    /// Creates a choice whose motive shape agrees with the exact participation kind.
    /// </summary>
    public CharacterTurnParticipationDecision(
        CharacterId scheduledCharacterId,
        CharacterTurnParticipationKind kind,
        CharacterGoal? motive)
    {
        ArgumentNullException.ThrowIfNull(scheduledCharacterId);
        ArgumentNullException.ThrowIfNull(kind);

        if (kind == CharacterTurnParticipationKind.Respond && motive is not null)
        {
            throw new ArgumentException(
                "A direct response cannot retain an interjection or silence motive.",
                nameof(motive));
        }

        if (kind != CharacterTurnParticipationKind.Respond && motive is null)
        {
            throw new ArgumentException(
                "Interjection and meaningful silence require an explicit motive.",
                nameof(motive));
        }

        ScheduledCharacterId = scheduledCharacterId;
        Kind = kind;
        Motive = motive;
    }

    /// <summary>Gets the exact scheduled character identifier.</summary>
    public CharacterId ScheduledCharacterId { get; }

    /// <summary>Gets the exact closed participation kind.</summary>
    public CharacterTurnParticipationKind Kind { get; }

    /// <summary>Gets the exact existing motive for interjection or silence.</summary>
    public CharacterGoal? Motive { get; }
}
