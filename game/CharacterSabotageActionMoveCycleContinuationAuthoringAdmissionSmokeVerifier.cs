using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact continuation-cycle authoring admission.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationAuthoringAdmissionSmokeVerifier
{
    /// <summary>Checks substitute boundaries and admits only the exact current cycle once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageCycleBoundary boundary,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationAuthoringAdmissionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationAuthoringAdmissionCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationAuthoringAdmissionCoordinator");
        CharacterSabotageCycleBoundary rebuiltBoundary = new(boundary.Authorization);
        CharacterSabotageTurnCompletion completion = boundary.Authorization.Admission;
        CharacterSabotageCycleBoundary malformedBoundary = new(
            new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
                new CharacterSabotageTurnCompletion(
                    completion.Actor,
                    completion.AuthorRequest,
                    CharacterSabotageActionKind.Conceal),
                boundary.Authorization.TurnActivity));

        bool admittedSuccessfully = target.Admission is null
            && !target.TryAdmit(null)
            && !target.TryAdmit(rebuiltBoundary)
            && !target.TryAdmit(malformedBoundary)
            && target.TryAdmit(boundary)
            && target.Admission is { } admission
            && ReferenceEquals(admission.Boundary, boundary)
            && ReferenceEquals(admission.Actor, completion.Actor)
            && !target.TryAdmit(boundary)
            && ReferenceEquals(target.Admission, admission)
            && target.IsCurrentAdmission(admission)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return admittedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationAuthoringSmokeVerifier.PassesMainScene(
                scene,
                state,
                target.Admission!,
                recovered,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
