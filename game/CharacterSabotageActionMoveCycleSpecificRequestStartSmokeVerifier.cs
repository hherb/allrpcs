using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact typed third-cycle Move request.</summary>
internal static class CharacterSabotageActionMoveCycleSpecificRequestStartSmokeVerifier
{
    /// <summary>Checks rejected substitute requests and one explicit fresh typed start.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleSpecificRequestCoordinator requests =
            scene.GetNode<CharacterSabotageActionMoveCycleSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleSpecificRequestCoordinator");
        CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator>(
                "../CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator");
        TemporaryCharacterObjectSabotageMoveModelSource models =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
                "../CycledTemporaryCharacterObjectSabotageMoveModelSource");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../CycledCharacterObjectSabotageMoveTaskController");
        if (requests.Request is not { } request
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterObjectSabotageMoveConsiderationRequest rebuiltRequest = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [source.CycleCandidate]);
        RecordingMoveModel model = new();
        if (!models.TrySupplyModel(model)
            || target.StartedRequest is not null
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || target.TryStart(null)
            || target.TryStart(rebuiltRequest)
            || model.CallCount != 0
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || !target.TryStart(request)
            || !ReferenceEquals(target.StartedRequest, request)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1
            || target.TryStart(request)
            || actionModel.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleAppliedResultObservationSmokeVerifier
            .PassesMainScene(
                scene,
                source,
                state,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
