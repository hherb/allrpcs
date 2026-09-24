using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies pure cycle-start admission after the fifth Move cycle.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionSmokeVerifier
{
    /// <summary>Checks substitute boundaries and admits only the exact current successor once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageCycleBoundary boundary,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel actionModel,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel cycleActionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel,
        RecordingMoveModel moveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionCoordinator");
        CharacterSabotageCycleBoundary rebuiltBoundary = new(boundary.Authorization);
        CharacterSabotageTurnCompletion completion = boundary.Authorization.Admission;
        CharacterSabotageCycleBoundary malformedBoundary = new(
            new CharacterSabotageOrdinaryTurnSchedulingAuthorization(
                new CharacterSabotageTurnCompletion(
                    completion.Actor,
                    completion.AuthorRequest,
                    CharacterSabotageActionKind.Conceal),
                boundary.Authorization.TurnActivity));

        return target.Admission is null
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
            && !target.IsCurrentAdmission(new CharacterSabotageActionAuthoringAdmission(boundary))
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
    }
}
