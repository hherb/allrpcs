using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Revalidates one exact sabotage admission at the ordinary turn start boundary.</summary>
public static class CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy
{
    /// <summary>Returns true only while the exact admission and ordinary turn remain current.</summary>
    public static bool CanSchedule(
        CharacterSabotageTurnCompletion? currentAdmission,
        CharacterSabotageTurnCompletion? expectedAdmission,
        CharacterTurnActivity? boundTurnActivity,
        CharacterTurnActivity? currentTurnActivity) =>
        CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
            currentAdmission,
            expectedAdmission)
        && ReferenceEquals(boundTurnActivity, currentTurnActivity)
        && CharacterTurnStartPolicy.CanStart(currentTurnActivity);
}
