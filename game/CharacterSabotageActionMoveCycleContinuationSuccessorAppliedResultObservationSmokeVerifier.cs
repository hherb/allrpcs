using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies observation of the exact applied typed fifth-cycle Move result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationSmokeVerifier
{
    /// <summary>Checks rejected results and one exact result with its task-installed authority.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator requests =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator start =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator
            target = scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../SuccessorCharacterObjectSabotageMoveTaskController");
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
            [source.SuccessorCandidate]);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.SuccessorCandidate.Destination,
            source.SuccessorCandidate.Evidence);
        CharacterObjectSabotageMoveConsiderationRequest foreignCandidateRequest = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [foreignCandidate]);
        CharacterObjectSabotageMoveConsiderationRequest foreignActorRequest = new(
            RepresentativeEnemyAgentCharacterSource.Create(),
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [source.SuccessorCandidate]);
        CharacterObjectSabotageMoveConsiderationResult rebuiltRequestResult =
            CreateAbstainingResult(rebuiltRequest);
        CharacterObjectSabotageMoveConsiderationResult foreignCandidateResult =
            CreateSelectedResult(foreignCandidateRequest, foreignCandidate);
        CharacterObjectSabotageMoveConsiderationResult foreignActorResult =
            CreateAbstainingResult(foreignActorRequest);
        CharacterObjectSabotageMoveConsiderationResult abstainingResult =
            CreateAbstainingResult(request);

        if (target.ObservedResult is not null
            || target.ObservedPlacements is not null
            || target.ObservedEvidence is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltRequestResult)
            || target.TryObserveCompletedResult(foreignCandidateResult)
            || target.TryObserveCompletedResult(foreignActorResult)
            || target.TryObserveCompletedResult(abstainingResult)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Running
            || task.Result is not null
            || !CountsRemainOne(actionModel, continuedActionModel, continuedMoveModel,
                cycleActionModel, priorActionModel, priorMoveModel, moveModel)
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

        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = CreateSelectedResult(
            request,
            source.SuccessorCandidate);
        bool observed = ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, source.SuccessorCandidate)
            && ReferenceEquals(task.ResolvedPlacements, appliedPlacements)
            && ReferenceEquals(task.ResolvedEvidence, appliedEvidence)
            && !ReferenceEquals(appliedPlacements, placements)
            && !ReferenceEquals(appliedEvidence, evidence)
            && !target.TryObserveCompletedResult(rebuiltResult)
            && !target.TryObserveCompletedResult(rebuiltRequestResult)
            && !target.TryObserveCompletedResult(foreignCandidateResult)
            && !target.TryObserveCompletedResult(foreignActorResult)
            && !target.TryObserveCompletedResult(abstainingResult)
            && target.TryObserveCompletedResult(result)
            && ReferenceEquals(target.ObservedResult, result)
            && ReferenceEquals(target.ObservedPlacements, appliedPlacements)
            && ReferenceEquals(target.ObservedEvidence, appliedEvidence)
            && !target.TryObserveCompletedResult(result)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Completed
            && ReferenceEquals(task.Result, result)
            && ReferenceEquals(task.ResolvedPlacements, appliedPlacements)
            && ReferenceEquals(task.ResolvedEvidence, appliedEvidence)
            && ReferenceEquals(start.StartedRequest, request)
            && ReferenceEquals(requests.Request, request)
            && CountsRemainOne(actionModel, continuedActionModel, continuedMoveModel,
                cycleActionModel, priorActionModel, priorMoveModel, moveModel);
        return observed
            && CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    result,
                    appliedPlacements,
                    appliedEvidence,
                    actionModel,
                    continuedActionModel,
                    continuedMoveModel,
                    cycleActionModel,
                    priorActionModel,
                    priorMoveModel,
                    moveModel);
    }

    private static CharacterObjectSabotageMoveConsiderationResult CreateAbstainingResult(
        CharacterObjectSabotageMoveConsiderationRequest request) =>
        new(request, new CharacterObjectSabotageMoveDecision(request.Character.Identity.Id, null));

    private static CharacterObjectSabotageMoveConsiderationResult CreateSelectedResult(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveCandidate candidate) =>
        new(
            request,
            new CharacterObjectSabotageMoveDecision(
                request.Character.Identity.Id,
                candidate.Evidence.Evidence.EvidenceId));

    private static bool CountsRemainOne(
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel actionModel,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel cycleActionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel,
        RecordingMoveModel moveModel) =>
        actionModel.CallCount == 1
        && continuedActionModel.CallCount == 1
        && continuedMoveModel.CallCount == 1
        && cycleActionModel.CallCount == 1
        && priorActionModel.CallCount == 1
        && priorMoveModel.CallCount == 1
        && moveModel.CallCount == 1;
}
