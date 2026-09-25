using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Reserves the shared character-turn slot for one exact current sabotage opportunity.</summary>
public static class CharacterSabotageTurnReservationTransition
{
    /// <summary>Returns one reservation or fails before publishing partial output.</summary>
    public static CharacterSabotageTurnReservation Reserve(
        CharacterSabotageTurnOpportunity opportunity,
        CharacterSabotageTurnOpportunity currentOpportunity,
        CharacterTurnActivity? currentActivity,
        CharacterSabotageTurnReservation? currentReservation)
    {
        if (!CharacterSabotageTurnReservationPolicy.CanReserve(
            opportunity,
            currentOpportunity,
            currentActivity,
            currentReservation))
        {
            throw new InvalidOperationException(
                "The supplied sabotage opportunity cannot reserve the shared turn slot.");
        }

        return new CharacterSabotageTurnReservation(opportunity);
    }
}
