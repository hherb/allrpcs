using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;

namespace Allrpcs.Game;

/// <summary>Verifies exact one-shot authoring admission in the focused Move smoke.</summary>
internal static class CharacterSabotageActionAuthoringAdmissionSmokeVerifier
{
    /// <summary>Checks rejected substitute graphs and one exact current admission.</summary>
    internal static bool Passes(
        CharacterSabotageActionAuthoringAdmissionCoordinator target,
        CharacterSabotageCycleBoundary boundary,
        Character actor,
        CharacterTurnActivity activeTurn)
    {
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization =
            boundary.Authorization;
        CharacterSabotageTurnCompletion completion = authorization.Admission;
        CharacterSabotageCycleBoundary rebuiltBoundary = new(authorization);
        CharacterSabotageCycleBoundary malformedBoundary = new(
            new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
                new CharacterSabotageTurnCompletion(
                    completion.Actor,
                    completion.AuthorRequest,
                    CharacterSabotageActionKind.Conceal),
                authorization.TurnActivity));

        if (target.Admission is not null
            || target.TryAdmit(null)
            || target.TryAdmit(rebuiltBoundary)
            || target.TryAdmit(malformedBoundary)
            || !CharacterSabotageCycleStartEligibilitySmokeVerifier.Passes(boundary, activeTurn)
            || !target.TryAdmit(boundary)
            || target.Admission is not { } admission
            || !ReferenceEquals(admission.Boundary, boundary)
            || !ReferenceEquals(admission.Actor, actor))
        {
            return false;
        }

        CharacterSabotageActionAuthoringAdmission retainedAdmission = admission;
        return !target.TryAdmit(boundary)
            && ReferenceEquals(target.Admission, retainedAdmission)
            && ReferenceEquals(retainedAdmission.Boundary, boundary)
            && ReferenceEquals(retainedAdmission.Actor, completion.Actor);
    }
}
