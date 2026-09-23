using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Revalidates one exact sabotage-cycle boundary before future action authoring.</summary>
public static class CharacterSabotageCycleStartEligibilityPolicy
{
    /// <summary>Returns true only while the complete exact boundary graph remains startable.</summary>
    public static bool CanStart(
        CharacterSabotageCycleBoundary? currentBoundary,
        CharacterSabotageCycleBoundary? expectedBoundary,
        CharacterSabotageOrdinaryTurnSchedulingAuthorization? currentAuthorization,
        CharacterSabotageTurnCompletion? currentAdmission,
        CharacterTurnActivity? currentTurnActivity) =>
        currentBoundary is not null
        && currentAuthorization is not null
        && ReferenceEquals(currentBoundary, expectedBoundary)
        && ReferenceEquals(currentBoundary.Authorization, currentAuthorization)
        && ReferenceEquals(currentAuthorization.Admission, currentAdmission)
        && ReferenceEquals(currentAuthorization.TurnActivity, currentTurnActivity)
        && CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
            currentAdmission,
            currentAuthorization.Admission)
        && CharacterTurnStartPolicy.CanStart(currentTurnActivity);
}
