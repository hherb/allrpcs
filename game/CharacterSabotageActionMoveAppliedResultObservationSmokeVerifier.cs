using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one-shot observation of the exact applied typed follow-up Move result.</summary>
internal static class CharacterSabotageActionMoveAppliedResultObservationSmokeVerifier
{
    /// <summary>Checks rejected non-current results and one exact completed applied result.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveSpecificRequestCoordinator requests =
            scene.GetNode<CharacterSabotageActionMoveSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveSpecificRequestCoordinator");
        CharacterSabotageActionMoveSpecificRequestStartCoordinator start =
            scene.GetNode<CharacterSabotageActionMoveSpecificRequestStartCoordinator>(
                "../CharacterSabotageActionMoveSpecificRequestStartCoordinator");
        CharacterSabotageActionMoveAppliedResultObservationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveAppliedResultObservationCoordinator>(
                "../CharacterSabotageActionMoveAppliedResultObservationCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../ScheduledCharacterObjectSabotageMoveTaskController");
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
            [source.FollowUpCandidate]);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.FollowUpCandidate.Destination,
            source.FollowUpCandidate.Evidence);
        CharacterObjectSabotageMoveConsiderationRequest foreignCandidateRequest = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            placements,
            evidence,
            [foreignCandidate]);
        CharacterObjectSabotageMoveConsiderationResult rebuiltRequestResult =
            CreateAbstainingResult(rebuiltRequest);
        CharacterObjectSabotageMoveConsiderationResult foreignCandidateResult =
            CreateAbstainingResult(foreignCandidateRequest);
        CharacterObjectSabotageMoveConsiderationResult abstainingResult = new(
            request,
            new CharacterObjectSabotageMoveDecision(source.Character.Identity.Id, null));

        if (target.ObservedResult is not null
            || target.ObservedPlacements is not null
            || target.ObservedEvidence is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltRequestResult)
            || target.TryObserveCompletedResult(foreignCandidateResult)
            || target.TryObserveCompletedResult(abstainingResult)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        task._Process(0);
        if (task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || task.Result is not { } result
            || state.Placements is not { } appliedPlacements
            || state.Evidence is not { } appliedEvidence
            || !ReferenceEquals(task.ResolvedPlacements, appliedPlacements)
            || !ReferenceEquals(task.ResolvedEvidence, appliedEvidence))
        {
            return false;
        }

        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = CreateResult(
            request,
            source.FollowUpCandidate);
        if (!ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.SelectedCandidate, source.FollowUpCandidate)
            || ReferenceEquals(appliedPlacements, placements)
            || ReferenceEquals(appliedEvidence, evidence)
            || !HasExactAppliedState(
                request,
                source.FollowUpCandidate,
                appliedPlacements,
                appliedEvidence)
            || target.TryObserveCompletedResult(rebuiltResult)
            || target.TryObserveCompletedResult(rebuiltRequestResult)
            || target.TryObserveCompletedResult(foreignCandidateResult)
            || target.TryObserveCompletedResult(abstainingResult)
            || !target.TryObserveCompletedResult(result)
            || !ReferenceEquals(target.ObservedResult, result)
            || !ReferenceEquals(target.ObservedPlacements, appliedPlacements)
            || !ReferenceEquals(target.ObservedEvidence, appliedEvidence)
            || target.TryObserveCompletedResult(result)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || !ReferenceEquals(task.Result, result)
            || !ReferenceEquals(task.ResolvedPlacements, appliedPlacements)
            || !ReferenceEquals(task.ResolvedEvidence, appliedEvidence)
            || !ReferenceEquals(start.StartedRequest, request)
            || !ReferenceEquals(requests.Request, request)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveAppliedResultBindingSmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            model,
            actionModel);
    }

    private static CharacterObjectSabotageMoveConsiderationResult CreateResult(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveCandidate candidate) =>
        new(
            request,
            new CharacterObjectSabotageMoveDecision(
                request.Character.Identity.Id,
                candidate.Evidence.Evidence.EvidenceId));

    private static CharacterObjectSabotageMoveConsiderationResult CreateAbstainingResult(
        CharacterObjectSabotageMoveConsiderationRequest request) =>
        new(
            request,
            new CharacterObjectSabotageMoveDecision(request.Character.Identity.Id, null));

    private static bool HasExactAppliedState(
        CharacterObjectSabotageMoveConsiderationRequest request,
        CharacterObjectSabotageMoveCandidate candidate,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence) =>
        placements.Entries.Count == request.Placements.Entries.Count
        && placements.Entries.Count(entry =>
            ReferenceEquals(entry.WorldObject, candidate.Destination.Occurrence.Target)
            && ReferenceEquals(entry.Location, candidate.Destination.Destination)) == 1
        && evidence.Entries.Count == request.Evidence.Entries.Count + 1
        && request.Evidence.Entries
            .Select((entry, index) => ReferenceEquals(evidence.Entries[index], entry))
            .All(retained => retained)
        && ReferenceEquals(evidence.Entries[^1], candidate.Evidence.Evidence);
}
