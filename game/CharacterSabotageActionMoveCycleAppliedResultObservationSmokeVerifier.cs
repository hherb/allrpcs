using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies observation of the exact applied typed third-cycle Move result.</summary>
internal static class CharacterSabotageActionMoveCycleAppliedResultObservationSmokeVerifier
{
    /// <summary>Checks rejected results and one exact result with its fresh state.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleSpecificRequestCoordinator requests =
            scene.GetNode<CharacterSabotageActionMoveCycleSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleSpecificRequestCoordinator");
        CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator start =
            scene.GetNode<CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator>(
                "../CharacterSabotageActionMoveCycleSpecificRequestStartCoordinator");
        CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator");
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
        CharacterObjectSabotageMoveConsiderationResult rebuiltRequestResult =
            CreateAbstainingResult(rebuiltRequest);
        CharacterObjectSabotageMoveConsiderationResult abstainingResult =
            CreateAbstainingResult(request);
        if (target.ObservedResult is not null
            || target.ObservedPlacements is not null
            || target.ObservedEvidence is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltRequestResult)
            || target.TryObserveCompletedResult(abstainingResult)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        task._Process(0);
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { } result
            || state.Placements is not { } appliedPlacements
            || state.Evidence is not { } appliedEvidence)
        {
            return false;
        }

        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            request,
            new CharacterObjectSabotageMoveDecision(
                request.Character.Identity.Id,
                source.CycleCandidate.Evidence.Evidence.EvidenceId));
        bool observed = ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, source.CycleCandidate)
            && ReferenceEquals(task.ResolvedPlacements, appliedPlacements)
            && ReferenceEquals(task.ResolvedEvidence, appliedEvidence)
            && !ReferenceEquals(appliedPlacements, placements)
            && !ReferenceEquals(appliedEvidence, evidence)
            && !target.TryObserveCompletedResult(rebuiltResult)
            && !target.TryObserveCompletedResult(rebuiltRequestResult)
            && !target.TryObserveCompletedResult(abstainingResult)
            && target.TryObserveCompletedResult(result)
            && ReferenceEquals(target.ObservedResult, result)
            && ReferenceEquals(target.ObservedPlacements, appliedPlacements)
            && ReferenceEquals(target.ObservedEvidence, appliedEvidence)
            && !target.TryObserveCompletedResult(result)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Completed
            && ReferenceEquals(task.Result, result)
            && ReferenceEquals(start.StartedRequest, request)
            && ReferenceEquals(requests.Request, request)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return observed
            && CharacterSabotageActionMoveCycleAppliedResultBindingSmokeVerifier.PassesMainScene(
                scene,
                source,
                state,
                result,
                appliedPlacements,
                appliedEvidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }

    private static CharacterObjectSabotageMoveConsiderationResult CreateAbstainingResult(
        CharacterObjectSabotageMoveConsiderationRequest request) =>
        new(
            request,
            new CharacterObjectSabotageMoveDecision(request.Character.Identity.Id, null));
}
