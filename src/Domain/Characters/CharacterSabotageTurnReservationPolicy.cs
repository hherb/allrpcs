using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether an exact current sabotage opportunity may reserve the shared turn slot.</summary>
public static class CharacterSabotageTurnReservationPolicy
{
    /// <summary>Returns true only for exact current opportunity and an otherwise available slot.</summary>
    public static bool CanReserve(
        CharacterSabotageTurnOpportunity opportunity,
        CharacterSabotageTurnOpportunity currentOpportunity,
        CharacterTurnActivity? currentActivity,
        CharacterSabotageTurnReservation? currentReservation)
    {
        ArgumentNullException.ThrowIfNull(opportunity);
        ArgumentNullException.ThrowIfNull(currentOpportunity);

        return ReferenceEquals(opportunity, currentOpportunity)
            && currentReservation is null
            && CharacterTurnStartPolicy.CanStart(currentActivity);
    }
}
