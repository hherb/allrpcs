using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies observation of the exact applied typed continuation Move result.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationAppliedResultObservationSmokeVerifier
{
    /// <summary>Checks rejected results and one exact result with its task-installed authority.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator requests =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator");
        CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator start =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator");
        CharacterSabotageActionMoveCycleContinuationAppliedResultObservationCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationAppliedResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationAppliedResultObservationCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../ContinuedCharacterObjectSabotageMoveTaskController");
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
            [source.ContinuationCandidate]);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.ContinuationCandidate.Destination,
            source.ContinuationCandidate.Evidence);
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
            [source.ContinuationCandidate]);
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
            || continuedModel.CallCount != 1
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
                source.ContinuationCandidate.Evidence.Evidence.EvidenceId));
        bool observed = ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedCandidate, source.ContinuationCandidate)
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
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return observed
            && CharacterSabotageActionMoveCycleContinuationAppliedResultBindingSmokeVerifier
                .PassesMainScene(
                    scene,
                    source,
                    state,
                    result,
                    appliedPlacements,
                    appliedEvidence,
                    continuedModel,
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

    private static CharacterObjectSabotageMoveConsiderationResult CreateSelectedResult(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveCandidate candidate) =>
        new(
            request,
            new CharacterObjectSabotageMoveDecision(
                request.Character.Identity.Id,
                candidate.Evidence.Evidence.EvidenceId));
}
