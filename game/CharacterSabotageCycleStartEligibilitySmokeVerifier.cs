using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Verifies pure exact-reference cycle-start eligibility in the focused Move smoke.</summary>
internal static class CharacterSabotageCycleStartEligibilitySmokeVerifier
{
    /// <summary>Checks rejected rebuilt graphs and the exact still-current empty-slot graph.</summary>
    internal static bool Passes(
        CharacterSabotageCycleBoundary boundary,
        CharacterTurnActivity activeTurn)
    {
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization =
            boundary.Authorization;
        CharacterSabotageTurnCompletion admission = authorization.Admission;
        CharacterSabotageTurnCompletion rebuiltAdmission = new(
            admission.Actor,
            admission.AuthorRequest,
            admission.ActionKind);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization rebuiltAuthorization = new(
            admission,
            authorization.TurnActivity);
        CharacterSabotageCycleBoundary rebuiltBoundary = new(authorization);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization activeAuthorization = new(
            admission,
            activeTurn);
        CharacterSabotageCycleBoundary activeBoundary = new(activeAuthorization);

        return !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                null, boundary, authorization, admission, authorization.TurnActivity)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, null, authorization, admission, authorization.TurnActivity)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, rebuiltBoundary, authorization, admission, authorization.TurnActivity)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, boundary, rebuiltAuthorization, admission, authorization.TurnActivity)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, boundary, authorization, rebuiltAdmission, authorization.TurnActivity)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, boundary, authorization, admission, activeTurn)
            && !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                activeBoundary, activeBoundary, activeAuthorization, admission, activeTurn)
            && CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary, boundary, authorization, admission, authorization.TurnActivity);
    }
}
