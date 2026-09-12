using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Dialogue;

/// <summary>
/// Validates one proposed participation choice against exact authored character state.
/// </summary>
public static class CharacterTurnParticipationDecisionPolicy
{
    /// <summary>
    /// Returns true when the exact scheduled character may make the supplied explicit choice.
    /// </summary>
    public static bool CanAccept(
        CharacterTurnParticipationDecision decision,
        Character character,
        bool isDirectlyAddressed)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(character);

        if (!ReferenceEquals(decision.ScheduledCharacterId, character.Identity.Id))
        {
            return false;
        }

        if (decision.Kind == CharacterTurnParticipationKind.Respond)
        {
            return isDirectlyAddressed;
        }

        if (decision.Kind == CharacterTurnParticipationKind.Interject && isDirectlyAddressed)
        {
            return false;
        }

        return character.Goals.Entries.Any(goal => ReferenceEquals(goal, decision.Motive));
    }
}
